﻿using Siemens.Engineering;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Xml;
using TIAEktool.Plc.Types;
using TIAEKtool.Plc;

namespace TIAEKtool
{


    public class TagParser : IDisposable
    {
        public enum Options
        {
            AllowNoComment = 1, // Also handle tags without comments
            NoSubelement = 2, // Don't parse subelements. This means most comments are not parsed.
        }

        public class ParseDoneEventArgs : EventArgs
        {

        }

        public event EventHandler<ParseDoneEventArgs> ParseDone;

        public class HandleTagEventArgs :EventArgs
        {
            public PathComponent Path;
            public MultilingualText Comment;
        }

        public event EventHandler<HandleTagEventArgs> HandleTag;

      
        readonly TiaPortal portal;

        BackgroundWorker worker;
        public TagParser(TiaPortal portal)
        {
            this.portal = portal;
        }

        class ParseCtxt
        {

          

            public  delegate void HandleTag(HandleTagEventArgs args);
            protected HandleTag handle_tag;

         
            protected Options options;
            public MessageLog Log = null;

            public ParseCtxt(HandleTag handle_tag, Options options = 0)
            {
                this.handle_tag = handle_tag;
                this.options = options;

            }

            private void IterBlockFolder(PlcBlockUserGroupComposition folders)
            {
                foreach (PlcBlockUserGroup folder in folders)
                {
                    HandleBlockFolder(folder);
                }
            }

            private void IterDataBlock(PlcBlockComposition blocks)
            {
                foreach (PlcBlock block in blocks)
                {
                    HandleDataBlock(block);
                }
            }

            public void HandleBlockFolder(PlcBlockGroup folder)
            {

                IterDataBlock(folder.Blocks);
                IterBlockFolder(folder.Groups);
            }

            public void HandleDataBlock(PlcBlock block)
            {
                if (block.ProgrammingLanguage != ProgrammingLanguage.DB) return;
                try
                {

                    BlockType instance_of = (BlockType)block.GetAttribute("InstanceOfType");
                    if (instance_of != BlockType.UDT) return;
                }
                catch (EngineeringNotSupportedException)
                {

                }
                if (!block.IsConsistent)
                {
                    Log?.LogMessage(MessageLog.Severity.Warning, "Skipped block " + block.Name + " because it is inconsistent. Compile block to make it consistent.");
                    return;
                }
                FileInfo path = TempFile.File("block_", "xml");
                try
                {
                    Console.WriteLine("Exporting: " + block.Name);
                    block.Export(path, ExportOptions.WithDefaults);
                    ParseXML_DB(path);
                }
                catch (Siemens.Engineering.EngineeringTargetInvocationException e)
                {
                    Log?.LogMessage(MessageLog.Severity.Error, "Failed to export block " + block.Name + ": " + e.Message);
                }
                finally
                {
                    try
                    {
                        path.Delete();
                    }
                    catch (IOException e)
                    {
                        Log?.LogMessage(MessageLog.Severity.Error, "Failed to delete temporary file: " + e.Message);
                    }
                }

            }



            MultilingualText ReadComment(XmlElement comment_elem)
            {

                MultilingualText comment = new MultilingualText();
                XmlNodeList text_elems = comment_elem.SelectNodes("if:MultiLanguageText", XMLUtil.nameSpaces);
                foreach (XmlNode t in text_elems)
                {
                    XmlElement mlt = t as XmlElement;
                    string lang = mlt.GetAttribute("Lang");

                    string text = mlt.InnerText;
                    comment.AddText(lang, text);
                }
                return comment;

            }


                    
            static readonly char[] path_sep = new char[] { ',' };
            protected void ReadSubelement(XmlElement subelement, PathComponent parent)
            {

                string indices_str = subelement.GetAttribute("Path");
                string[] index_strings = indices_str.Split(path_sep);
                int[] indices = new int[index_strings.Length];
                for (int i = 0; i < index_strings.Length; i++)
                {
                    indices[i] = int.Parse(index_strings[i]);
                }
                
                int subs_count = PathComponentUtils.SubstituteIndices(parent, out PathComponent subs, (indices as IList<int>).GetEnumerator());
                if (subs_count != indices.Length)
                {
                    if (!(subs is IndexComponent ic)
                        || (subs_count != (indices.Length + ic.Indices.Length)))
                    {
                        throw new Exception("Length of path in subelement doesn't match number of indices in path");
                    }
                    // It's the path of the array itself not an array item.
                    subs = subs.Parent;
                }
              

                MultilingualText comment = null;
                if (subelement.SelectSingleNode("if:Comment", XMLUtil.nameSpaces) is XmlElement comment_elem)
                {
                    comment = ReadComment(comment_elem);
                }
                if (((options & Options.AllowNoComment) != 0) || comment != null) {
                    handle_tag(new HandleTagEventArgs()
                    {
                        Path = subs,
                        Comment = comment
                    });
                   
                }


            }





            protected MemberComponent ReadMember(XmlElement member_elem, PathComponent parent)
            {
                string name = member_elem.GetAttribute("Name");



                string type_str = member_elem.GetAttribute("Datatype");
                DataType type = DataTypeParser.Parse(type_str, out string left);
                MemberComponent member = new MemberComponent(name, type, parent);
                PathComponent child_path = member;
                if (type is ARRAY array) { 
                
                    child_path = new IndexComponent(new int[array.Limits.Count], member);

                    if ((options & Options.NoSubelement) != 0)
                    {
                        if (member != child_path)
                        {
                            handle_tag(new HandleTagEventArgs()
                            {
                                Path = PathComponentUtils.SubstituteIndicesLow(child_path),
                                Comment = null
                            });
                        }
                    }

                }


               
                MultilingualText comment = null;
                if (member_elem.SelectSingleNode("if:Comment", XMLUtil.nameSpaces) is XmlElement comment_elem)
                {
                    comment = ReadComment(comment_elem);
                }
                if (((options & Options.AllowNoComment) != 0) || comment != null)
                {
                    handle_tag(new HandleTagEventArgs()
                    {
                        Path = PathComponentUtils.SubstituteIndicesLow(member),
                        Comment = comment
                    });

                }

                XmlNodeList member_elems = member_elem.SelectNodes("if:Member", XMLUtil.nameSpaces);
                foreach (XmlNode m in member_elems)
                {
                    MemberComponent submember = ReadMember((XmlElement)m, child_path);
                    if (child_path.Type is STRUCT struct_type)
                    {
                        struct_type.Members.Add(new StructMember() { Name = submember.Name, MemberType = submember.Type });
                    }
                }

                if ((options & Options.NoSubelement) == 0)
                {
                    XmlNodeList sub_elems = member_elem.SelectNodes("if:Subelement", XMLUtil.nameSpaces);
                    foreach (XmlNode s in sub_elems)
                    {

                        ReadSubelement(s as XmlElement, child_path);

                    }
                }

                return member;

            }

            protected void ReadStaticSection(XmlNode section, PathComponent parent)
            {
                XmlNode member_node = section.FirstChild;
                while (member_node != null)
                {
                    if (member_node.NodeType == XmlNodeType.Element && member_node.Name == "Member")
                    {
                        ReadMember(member_node as XmlElement, parent);
                    }
                    member_node = member_node.NextSibling;
                }
            }

            void ParseXML_DB(FileInfo path)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path.ToString());
                XmlNode block_attrs = doc.SelectSingleNode("/Document/SW.Blocks.GlobalDB/AttributeList", XMLUtil.nameSpaces);
                if (block_attrs == null) return;

                XmlNode block_name_node = block_attrs.SelectSingleNode("Name", XMLUtil.nameSpaces);
                if (block_name_node == null) throw new XmlException("Missing block name");
                string block_name = block_name_node.InnerText;
                XmlNode static_section = block_attrs.SelectSingleNode("./Interface/if:Sections/if:Section[@Name='Static']", XMLUtil.nameSpaces);
                if (static_section == null) throw new XmlException("Missing static section of block");
                PathComponent parent = new MemberComponent(block_name, new STRUCT());

                ReadStaticSection(static_section, parent);
            }
        }

        class WorkerArg
        {
            public TiaPortal portal;
            public IEngineeringCompositionOrObject top;
            public ParseCtxt ctxt;

            public WorkerArg(TiaPortal portal,
            IEngineeringCompositionOrObject top,
            ParseCtxt ctxt)
            {
                this.portal = portal;
                this.top = top;
                this.ctxt = ctxt;
            }
        }
        public void Parse(IEngineeringCompositionOrObject top, MessageLog log = null, Options options = 0)
        {
            ParseCtxt parse = new ParseCtxt(OnHandleTag, options)
            {
                Log = log
            };
           
            lock (portal)
            {

                if (top is PlcBlockGroup group)
                {
                    parse.HandleBlockFolder(group);
                }
                else
                {
                    parse.HandleDataBlock((PlcBlock)top);
                }
            }
        }

        SynchronizationContext callback_ctxt;

        protected void HandleTagAsync(HandleTagEventArgs arg)
        {
            callback_ctxt.Post(OnHandleTag, arg);
        }

        public void ParseAsync(IEngineeringCompositionOrObject top, MessageLog log = null, Options options = 0)
        {
            if (worker != null && worker.IsBusy) return;

            worker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
            };
            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += RunWorkerCompleted;
        
            callback_ctxt = SynchronizationContext.Current;
            ParseCtxt parse = new ParseCtxt(HandleTagAsync, options)
            {
                Log = log
            };
            
            WorkerArg arg = new WorkerArg(portal, top, parse);
            worker.RunWorkerAsync(arg);
        }

        public static void DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArg arg = (WorkerArg)e.Argument;

            lock (arg.portal)
            {

                if (arg.top is PlcBlockGroup top)
                {
                    arg.ctxt.HandleBlockFolder(top);
                }
                else
                {
                    arg.ctxt.HandleDataBlock((PlcBlock)arg.top);
                }
            }
        }

        public void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnParseDone();
            worker = null;
        }

        protected virtual void OnParseDone()
        {
            ParseDone?.Invoke(this, new ParseDoneEventArgs());
        }

        public void OnHandleTag(object state)
        {
            OnHandleTag((HandleTagEventArgs)state);
        }

        public void OnHandleTag(HandleTagEventArgs arg)
        {
            HandleTag?.Invoke(this, arg);
        }


        public void CancelParse()
        {
            if (worker != null)
            {
                worker.CancelAsync();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    worker.Dispose();
                    worker = null;
                }

                disposedValue = true;
            }
        }

        ~TagParser() {
           // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
           Dispose(false);
        }

      
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
