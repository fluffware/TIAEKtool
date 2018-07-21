using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TIAEKtool.Properties;

namespace TIAEKtool
{
    public class PresetSCL
    {
        XmlElement structured_text;
        protected XmlDocument doc;
        public XmlDocument Document { get => doc; }
        XmlNamespaceManager nsmgr;
        Builder builder;
        string preset_db_name;
       
        protected const string StructuredTextNS = "http://www.siemens.com/automation/Openness/SW/NetworkSource/StructuredText/v1";
        public PresetSCL(string block_name, string db_name, XmlDocument doc = null)
        {
            preset_db_name = db_name;
            NameTable nt = new NameTable();
            nsmgr = new XmlNamespaceManager(nt);
            nsmgr.AddNamespace("st", StructuredTextNS);

            if (doc == null)
            {
                doc = new XmlDocument();
                doc.LoadXml(Resources.InitialPresetSCL);
            }
            this.doc = doc;
            structured_text =
                (XmlElement)doc.SelectSingleNode("/Document/SW.Blocks.FC/ObjectList/SW.Blocks.CompileUnit/AttributeList/NetworkSource/st:StructuredText", nsmgr);
            if (structured_text == null) throw new Exception("No 'StructuredText' in XML");
            XmlElement name_elem =
            (XmlElement)doc.SelectSingleNode("/Document/SW.Blocks.FC/AttributeList/Name", nsmgr);
            name_elem.InnerText = block_name;
            builder = new Builder(doc);
           
            /*
            preset_db_enable = doc.CreateDocumentFragment();
            preset_db_enable.AppendChild(builder.Component(db_name));
            preset_db_enable.AppendChild(builder.Token("."));
            preset_db_enable.AppendChild(builder.Component("Enable", index));*/
        }

        protected class Builder
        {
            public class XmlBuildException : Exception
            {
                public XmlBuildException(string msg) : base(msg)
                { }
            }
            XmlDocument doc;
            protected Stack<XmlElement> stack = new Stack<XmlElement>();
            public XmlElement Parent { get => stack.Peek(); }
            public XmlElement Last { get; protected set; }
            int uid = 1;

            public Builder(XmlDocument doc)
            {
                this.doc = doc;
                stack.Push(null);
                Last = null;
            }

            public void Add(XmlElement child)
            {
                if (Parent != null)
                {
                    Parent.AppendChild(child);
                }
                Last = child;
            }
            public void Push (XmlElement parent = null)
            {

                stack.Push(parent);
                Last = null;
            }

            public void Pop()
            {
                Last = stack.Pop();
            }

            public void Down()
            {
                if (Last == null) throw new XmlBuildException("No last elemnt to go down into");

                Push(Last);
               
            }

            public void UidElem(string name)
            {
                XmlElement elem = doc.CreateElement(name, StructuredTextNS);
                elem.SetAttribute("UId", uid.ToString());
                Add(elem);
                uid++;
            }

            public void Component(string name, XmlElement [] index = null)
            {
                UidElem("Component");
                Last.SetAttribute("Name", name);
                Down();
                if (index != null)
                {
                    Token("[");
                    if (index.Length >= 1)
                        Add(index[0]);
                    for (int i = 1; i < index.Length; i++)
                    {
                        Token(",");
                        Add(index[i]);
                    }
                    Token("]");
                }
                Pop();
            }

            public void Token(string text)
            {
                UidElem("Token");
                Last.SetAttribute("Text", text);
               
            }
            
            public void LiteralConstant(string value)
            {
               UidElem("Access");
                Last.SetAttribute("Scope", "LiteralConstant");
                Down();
                UidElem("Constant");
                Down();
                UidElem("ConstantValue");
                Last.InnerText = value;
                Pop();
                Pop();
            }

            public void GlobalVariable()
            {
               UidElem("Access");
                Last.SetAttribute("Scope", "GlobalVariable");
              
            }

            public void LocalVariable()
            {
                 UidElem("Access");
                Last.SetAttribute("Scope", "LocalVariable");
               
            }

            public void Blank(int num = 1)
            {
                UidElem("Blank");
                Last.SetAttribute("Num", num.ToString());
            }

            public void NewLine(int num = 1)
            {
                UidElem("NewLine");
                Last.SetAttribute("Num", num.ToString());
            }
            public void Symbol()
            {
                UidElem("Symbol");
            }

            // Add a '.' if previous element was a component
            public void AddComponentSep()
            {

                if (Last != null)
                {
                    if (Last.Name == "Component")
                    {
                        Token(".");
                    }
                }
            }


            public void SymbolAddComponents(PathComponent component)
            {
                
                LinkedList<PathComponent> list = new LinkedList<PathComponent>();
                while (component != null)
                {
                    list.AddFirst(component);
                    component = component.Parent;
                }
                bool first = true;
                foreach (PathComponent c in list)
                {
                    if (c is MemberComponent)
                    {
                        
                        if (!first)
                        {
                            Token(".");
                        }
                        else
                        {
                            AddComponentSep();
                            first = false;
                        }
                        Component(((MemberComponent)c).Name, null);
                    }

                    if (c is IndexComponent)
                    {
                        // Patch last component with index

                        Down();
                        Token("[");
                        int[] indices = ((IndexComponent)c).Indices;
                        if (indices.Length >= 1)
                        {
                            LiteralConstant(indices[0].ToString());
                            for (int i = 1; i < indices.Length; i++)
                            {
                                Token(",");
                                LiteralConstant(indices[i].ToString());

                            }
                            Token("]");
                          
                        }
                        Pop();
                    }
                   
                }
               
            }
        }
        public void AddStore(PathComponent comp)
        {
            builder.Push(structured_text);

            builder.GlobalVariable();
            builder.Down();
            builder.Symbol();
            builder.Down();

            builder.Component(preset_db_name);
            builder.Token(".");

            builder.Push();
            builder.LocalVariable();
            builder.Down();
            builder.Symbol();
            builder.Down();
            builder.Component("Index");
            builder.Pop();
            builder.Pop();
            XmlElement[] index = new XmlElement[1] { builder.Last };
            builder.Pop();

           
            builder.Component("Preset", index);

            builder.Token(".");
            builder.SymbolAddComponents(comp);
            builder.Pop(); // End symbol
            builder.Pop(); // End GlobalVariable

            builder.Blank();
            builder.Token(":=");
            builder.Blank();

            builder.GlobalVariable();
            builder.Down();
            builder.Symbol();
            builder.Down();
            builder.SymbolAddComponents(comp);
            builder.Pop();
            builder.Pop();

            builder.Token(";");
            builder.NewLine();

        }
    }
}
