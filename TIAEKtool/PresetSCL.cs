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
        protected XmlElement structured_text;
       
        protected Builder builder;
        protected string block_name;
        protected XmlDocument doc;
        public XmlDocument Document { get => doc; }
        protected XmlNamespaceManager nsmgr;
        protected const string StructuredTextNS = "http://www.siemens.com/automation/Openness/SW/NetworkSource/StructuredText/v1";
        protected const string InterfaceNS = "http://www.siemens.com/automation/Openness/SW/Interface/v3";
        public PresetSCL(string block_name)
        {
            NameTable nt = new NameTable();
            nsmgr = new XmlNamespaceManager(nt);
            nsmgr.AddNamespace("if", InterfaceNS);
            nsmgr.AddNamespace("st", StructuredTextNS);
            this.block_name = block_name;
           
          
          
        }

        protected void SetDocument(XmlDocument doc)
        {
            this.doc = doc;
            builder = new Builder(doc);
            structured_text =
               (XmlElement)doc.SelectSingleNode("/Document/SW.Blocks.FC/ObjectList/SW.Blocks.CompileUnit/AttributeList/NetworkSource/st:StructuredText", nsmgr);
            if (structured_text == null) throw new Exception("No 'StructuredText' in XML");
            XmlElement name_elem =
            (XmlElement)doc.SelectSingleNode("/Document/SW.Blocks.FC/AttributeList/Name", nsmgr);
            name_elem.InnerText = block_name;
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

        protected void AddComponentWithLocalIndex(string member, string index_variable)
        {
            builder.Push();
            builder.LocalVariable();
            builder.Down();
            builder.Symbol();
            builder.Down();
            builder.Component(index_variable);
            builder.Pop();
            builder.Pop();
            XmlElement[] index = new XmlElement[1] { builder.Last };
            builder.Pop();


            builder.Component(member, index);
        }
        
 
    }
}
