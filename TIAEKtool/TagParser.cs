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

    public abstract class PathComponent

    {
        public PathComponent Parent { get; protected set; }
     
        public DataType Type { get; protected set; }

        public PathComponent(DataType type, PathComponent parent = null)
        {
           
            Type = type;
            Parent = parent;
        }

        public abstract PathComponent CloneComponent();

        public PathComponent ClonePath()
        {
            PathComponent clone = CloneComponent();
            if (Parent != null)
            {
                clone.Parent = Parent.ClonePath();
            }
            return clone;
        }
        public PathComponent PrependPath(PathComponent prefix)
        {

            PathComponent clone = CloneComponent();
            if (Parent != null)
            {
                clone.Parent = Parent.PrependPath(prefix);
            } else
            {
                clone.Parent = prefix; 
            }
            return clone;
        }
    }

    public class MemberComponent : PathComponent
    {
        public string Name { get; protected set; }


        public MemberComponent(string name, DataType type, PathComponent parent = null) : base(type, parent)
        {
            Name = name;
        }

        public override string ToString()
        {
            return ((Parent != null) ? Parent.ToString() + "." : "") + Name;
        }
        public override PathComponent CloneComponent()
        {
            return new MemberComponent(Name, Type, Parent); 
        }
    }

        public class IndexComponent : PathComponent
    {
        public int[] Indices { get; protected set; }

        public IndexComponent(int[] indices, DataType type, PathComponent parent = null) : base(type, parent)
        {
            Indices = indices;
        }


        public override string ToString()
        {
            StringBuilder str = new StringBuilder(Parent.ToString());
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

        public override PathComponent CloneComponent()
        {
            return new IndexComponent(Indices, Type, Parent);
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
            public PathComponent Path;
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

            public MessageLog Log = null;

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


            // Substitute all indices in path with th low limit of the corresponding array
            protected PathComponent SubstituteIndicesLow(PathComponent path)
            {
                PathComponent parent_copy;
                if (path.Parent != null)
                {
                    parent_copy = SubstituteIndicesLow(path.Parent);
                }
                else
                {
                    parent_copy = null;
                }

                if (path is IndexComponent)
                {
                    IndexComponent ic = (IndexComponent)path;
                    int[] indices = new int[ic.Indices.Length];
                    if (!(ic.Parent is MemberComponent && ic.Parent.Type is ARRAY)) throw new Exception("Parent of index component is nor an array");
                    ARRAY array_type = (ARRAY)ic.Parent.Type;
                    for (int l = 0; l < array_type.Limits.Count; l++)
                    {
                        Constant low = array_type.Limits[l].LowLimit;
                        if (!(low is IntegerLiteral)) throw new Exception("Low limity of array is not an integer constant.");
                        int low_limit = ((IntegerLiteral)low).Value;
                        indices[l] = low_limit;
                        
                    }

                    return new IndexComponent(indices, ic.Type, parent_copy);
                }
                else
                {
                    MemberComponent member = (MemberComponent)path;
                    return new MemberComponent(member.Name, member.Type, parent_copy);
                }
            }

            
            /// <summary>
            /// Makes a copy of the path with the indices substitutes
            /// </summary>
            /// <param name="path">Original path</param>
            /// <param name="substituted">Copy of path with new indices</param>
            /// <param name="indices">Indices to substitute</param>
            /// <returns>Number of indices in path</returns>
            protected int SubstituteIndices(PathComponent path, out PathComponent substituted, IEnumerator<int> indices)
            {
                PathComponent parent_copy;
                int subs_count;
                if (path.Parent != null)
                {
                     subs_count = SubstituteIndices(path.Parent, out parent_copy, indices);
                    
                }
                else
                {
                    parent_copy = null;
                    subs_count = 0;
                }

                if (path is IndexComponent)
                {
                    IndexComponent ic = (IndexComponent)path;
                    IndexComponent copy = new IndexComponent(new int[ic.Indices.Length], ic.Type, parent_copy);
                    for (int i = 0; i < ic.Indices.Length; i++)
                    {
                        if (!indices.MoveNext()) break;
                        copy.Indices[i] = indices.Current;
                       
                    }
                    subs_count += ic.Indices.Length;
                    substituted = copy;
                    return subs_count;
                }
                else
                {
                    MemberComponent member = (MemberComponent)path;
                    substituted = new MemberComponent(member.Name, member.Type, parent_copy);
                    return subs_count;
                }
            }

            static readonly char[] path_sep = new char[] { ',' };
            protected void readSubelement(XmlElement subelement, PathComponent parent)
            {

                string indices_str = subelement.GetAttribute("Path");
                string[] index_strings = indices_str.Split(path_sep);
                int[] indices = new int[index_strings.Length];
                for (int i = 0; i < index_strings.Length; i++)
                {
                    indices[i] = int.Parse(index_strings[i]);
                }
                PathComponent subs;
                int subs_count = SubstituteIndices(parent, out subs, (indices as IList<int>).GetEnumerator());
                if (subs_count != indices.Length)
                {
                    if (!(subs is IndexComponent) 
                        || (subs_count != (indices.Length + ((IndexComponent)subs).Indices.Length)))
                    {
                        throw new Exception("Length of path in subelement doesn't match number of indices in path");
                    }
                    // It's the path of the array itself not an array item.
                    subs = subs.Parent;
                }
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





            protected MemberComponent readMember(XmlElement member_elem, PathComponent parent)
            {
                string name = member_elem.GetAttribute("Name");
                
              
               
                string type_str = member_elem.GetAttribute("Datatype");
                string left;
                DataType type = DataTypeParser.Parse(type_str, out left);
                MemberComponent member = new MemberComponent(name, type, parent);
                PathComponent child_path = member;
                if (type is ARRAY)
                {
                    ARRAY array =  (ARRAY)type;
                    child_path = new IndexComponent(new int[array.Limits.Count], array.MemberType, member);

                }


                XmlElement comment_elem = member_elem.SelectSingleNode("if:Comment", nsmgr) as XmlElement;
                if (comment_elem != null)
                {
                    MultilingualText comment = readComment(comment_elem);
                    callback_ctxt.Post(handle_tag, new HandleTagEventArgs()
                    {
                        Path = SubstituteIndicesLow(member),
                        Comment = comment
                    });
                }

                XmlNodeList member_elems = member_elem.SelectNodes("if:Member", nsmgr);
                foreach (XmlNode m in member_elems)
                {
                    MemberComponent submember = readMember((XmlElement)m, child_path);
                    if (child_path.Type is STRUCT)
                    {
                        STRUCT struct_type = (STRUCT)child_path.Type;
                        struct_type.Members.Add(new StructMember() { Name = submember.Name, MemberType = submember.Type });
                    }
                }

                XmlNodeList sub_elems = member_elem.SelectNodes("if:Subelement", nsmgr);
                foreach (XmlNode s in sub_elems)
                {

                    readSubelement(s as XmlElement, child_path);

                }


                return member;

            }

            protected void readStaticSection(XmlNode section, PathComponent parent)
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
                PathComponent parent = new MemberComponent(block_name, new STRUCT());

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
        public void ParseAsync(IEngineeringCompositionOrObject top, MessageLog log = null)
        {
            if (worker != null && worker.IsBusy) return;

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += RunWorkerCompleted;
            ParseCtxt parse = new ParseCtxt(SynchronizationContext.Current, OnHandleTag);
            parse.Log = log;
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
