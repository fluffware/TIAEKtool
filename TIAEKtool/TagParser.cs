    using PLC.Types;
    using Siemens.Engineering;
    using Siemens.Engineering.SW.Blocks;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
using TIAtool;

namespace TIAEKtool
{

    public class TagComponent

    {
        public TagComponent Parent { get; protected set; }
        public string Name { get; protected set; }
        public DataType Type { get; protected set; }

        public TagComponent(string name, DataType type, TagComponent parent = null)
        {
            Name = name;
            Type = type;
            Parent = parent;
        }

        public override string ToString()
        {
            return ((Parent != null)?Parent.ToString() + ".": "") + Name;
        }
    }


    public class ArrayComponent : TagComponent
    {
        public int[] Indices;

        public ArrayComponent(string name, DataType type, int[] indices, TagComponent parent = null) : base(name, type, parent)
        {
            Indices = indices;
        }


        public override string ToString()
        {
            StringBuilder str = new StringBuilder(base.ToString());
            str.Append("[");
            if (Indices.Length >= 1)
            {
                str.Append(Indices[0]);
                for (int i = 1; i < Indices.Length; i++)
                {
                    str.Append(",");
                    str.Append(Indices[i]);
                }
            }
            str.Append("]");
            return str.ToString();
        }


    }



    public class TagParser
    {


        public class ParseDoneEventArgs : EventArgs
        {

        }

        public event EventHandler<ParseDoneEventArgs> ParseDone;

        public class HandleTagEventArgs
        {
            public TagComponent Path;
            public MultilingualText Comment;
        }

        public event EventHandler<HandleTagEventArgs> HandleTag;

        TiaPortal portal;

        BackgroundWorker worker;
        public TagParser(TiaPortal portal)
        {
            this.portal = portal;
        }

        class ParseCtxt
        {

            SynchronizationContext callback_ctxt;
            SendOrPostCallback handle_tag;


            XmlNamespaceManager nsmgr;

            public ParseCtxt(SynchronizationContext callback_ctxt, SendOrPostCallback handle_tag)
            {
                this.callback_ctxt = callback_ctxt;
                this.handle_tag = handle_tag;

                NameTable nt = new NameTable();
                nsmgr = new XmlNamespaceManager(nt);
                nsmgr.AddNamespace("if", "http://www.siemens.com/automation/Openness/SW/Interface/v3");
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
                FileInfo path = TempFile.File("block_", "xml");
                try
                {
                    Console.WriteLine("Exporting: " + block.Name);
                    block.Export(path, ExportOptions.WithDefaults);
                    ParseXML_DB(path);
                }
                catch (Siemens.Engineering.EngineeringTargetInvocationException e)
                {
                    Console.WriteLine("Failed to export block " + block.Name + ": " + e.Message);
                }
                finally
                {
                    try
                    {
                        path.Delete();
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("Failed to delete temporary file: " + e.Message);
                    }
                }

            }



            MultilingualText readComment(XmlElement comment_elem)
            {

                MultilingualText comment = new MultilingualText();
                XmlNodeList text_elems = comment_elem.SelectNodes("if:MultiLanguageText", nsmgr);
                foreach (XmlNode t in text_elems)
                {
                    XmlElement mlt = t as XmlElement;
                    string lang = mlt.GetAttribute("Lang");

                    string text = mlt.InnerText;
                    comment.AddText(lang, text);
                }
                return comment;

            }


            protected TagComponent SubstituteIndices(TagComponent path, IEnumerator<int> indices)
            {
                TagComponent parent_copy;
                if (path.Parent != null)
                {
                    parent_copy = SubstituteIndices(path.Parent, indices);
                } else
                {
                    parent_copy = null;
                }
               
                if (path is ArrayComponent)
                {
                    ArrayComponent array = (ArrayComponent)path;
                    ArrayComponent copy = new ArrayComponent(array.Name, array.Type, new int[array.Indices.Length], parent_copy);
                    for (int i = 0; i < array.Indices.Length; i++)
                    {
                        indices.MoveNext();
                        copy.Indices[i] = indices.Current;
                    }
                    return copy;
                } else
                {
                  return new TagComponent(path.Name, path.Type, parent_copy);
                }
            }

            static readonly char[] path_sep = new char[] { ',' };
            protected void readSubelement(XmlElement subelement, TagComponent parent)
            {

                string indices_str = subelement.GetAttribute("Path");
                string[] index_strings = indices_str.Split(path_sep);
                int[] indices = new int[index_strings.Length];
                for (int i = 0; i < index_strings.Length; i++)
                {
                    indices[i] = int.Parse(index_strings[i]);
                }
                TagComponent subs = SubstituteIndices(parent, (indices as IList<int>).GetEnumerator());
                XmlElement comment_elem = subelement.SelectSingleNode("if:Comment", nsmgr) as XmlElement;
                if (comment_elem != null)
                {
                    MultilingualText comment = readComment(comment_elem);
                    callback_ctxt.Post(handle_tag, new HandleTagEventArgs()
                    {
                        Path = subs,
                        Comment = comment
                    });
                }


            }





            protected TagComponent readMember(XmlElement member_elem, TagComponent parent)
            {
                string name = member_elem.GetAttribute("Name");
                
              
               
                string type_str = member_elem.GetAttribute("Datatype");
                string left;
                DataType type = DataTypeParser.Parse(type_str, out left);
                TagComponent info;
               
                if (type is ARRAY)
                {
                    ARRAY array =  (ARRAY)type;
                    info = new ArrayComponent(name, type, new int[array.Limits.Count], parent);

                } else
                {
                    info = new TagComponent(name, type, parent);
                }


                XmlElement comment_elem = member_elem.SelectSingleNode("if:Comment", nsmgr) as XmlElement;
                if (comment_elem != null)
                {
                    MultilingualText comment = readComment(comment_elem);
                    callback_ctxt.Post(handle_tag, new HandleTagEventArgs()
                    {
                        Path = info,
                        Comment = comment
                    });
                }

                XmlNodeList member_elems = member_elem.SelectNodes("if:Member", nsmgr);
                foreach (XmlNode m in member_elems)
                {
                    TagComponent subinfo = readMember(m as XmlElement, info);
                    if (info.Type is STRUCT)
                    {
                        STRUCT struct_type = (STRUCT)info.Type;
                        struct_type.Members.Add(new StructMember() { Name = subinfo.Name, MemberType = subinfo.Type });
                    }
                    else if (info.Type is ARRAY)
                    {
                        ARRAY array_type = (ARRAY)info.Type;
                        if (array_type.MemberType is STRUCT)
                        {
                            STRUCT struct_type = array_type.MemberType as STRUCT;
                            struct_type.Members.Add(new StructMember() { Name = subinfo.Name, MemberType = subinfo.Type }); 
                        }
                    }
                }

                XmlNodeList sub_elems = member_elem.SelectNodes("if:Subelement", nsmgr);
                foreach (XmlNode s in sub_elems)
                {

                    readSubelement(s as XmlElement, info);

                }


                return info;

            }

            protected void readStaticSection(XmlNode section, TagComponent parent)
            {
                XmlNode member_node = section.FirstChild;
                while (member_node != null)
                {
                    if (member_node.NodeType == XmlNodeType.Element && member_node.Name == "Member")
                    {
                        readMember(member_node as XmlElement, parent);
                    }
                    member_node = member_node.NextSibling;
                }
            }

            void ParseXML_DB(FileInfo path)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path.ToString());
                XmlNode block_attrs = doc.SelectSingleNode("/Document/SW.Blocks.GlobalDB/AttributeList", nsmgr);
                if (block_attrs == null) return;

                XmlNode block_name_node = block_attrs.SelectSingleNode("Name", nsmgr);
                if (block_name_node == null) throw new XmlException("Missing block name");
                string block_name = block_name_node.InnerText;
                XmlNode static_section = block_attrs.SelectSingleNode("./Interface/if:Sections/if:Section[@Name='Static']", nsmgr);
                if (static_section == null) throw new XmlException("Missing static section of block");
                TagComponent parent = new TagComponent(block_name, new STRUCT());

                readStaticSection(static_section, parent);
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
        public void ParseAsync(IEngineeringCompositionOrObject top)
        {
            if (worker != null && worker.IsBusy) return;

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += RunWorkerCompleted;
            ParseCtxt parse = new ParseCtxt(SynchronizationContext.Current, OnHandleTag);
            WorkerArg arg = new WorkerArg(portal, top, parse);
            worker.RunWorkerAsync(arg);
        }

        public static void DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArg arg = (WorkerArg)e.Argument;

            lock (arg.portal)
            {

                if (arg.top is PlcBlockGroup)
                {
                    arg.ctxt.HandleBlockFolder((PlcBlockGroup)arg.top);
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
    }
}
