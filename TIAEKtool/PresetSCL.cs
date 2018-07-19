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
        protected const string StructuredTextNS = "http://www.siemens.com/automation/Openness/SW/NetworkSource/StructuredText/v1";
        public PresetSCL(string block_name, XmlDocument doc = null)
        {
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
        }

        class Builder
        {
            XmlDocument doc;
            public XmlElement Component(string name, XmlElement [] index)
            {
                XmlElement elem = doc.CreateElement("Component", StructuredTextNS);
                elem.SetAttribute("Name", name);
                if (index != null)
                {
                    elem.AppendChild(Token("["));
                    if (index.Length >= 1)
                        elem.AppendChild(index[0]);
                    for (int i = 1; i < index.Length; i++)
                    {
                        elem.AppendChild(Token(","));
                        elem.AppendChild(index[i]);
                    }
                    elem.AppendChild(Token("]"));
                }
                return elem;
            }
            public XmlElement Token(string text)
            {
                XmlElement elem = doc.CreateElement("Token", StructuredTextNS);
                elem.SetAttribute("Text", text);
                return elem;
            }
            
            public XmlElement LiteralConstant(string value)
            {
                XmlElement access = doc.CreateElement("Access", StructuredTextNS);
                access.SetAttribute("Scope", "LiteralConstant");
                XmlElement constant = doc.CreateElement("Constant", StructuredTextNS);
                XmlElement constant_value = doc.CreateElement("ConstantValue", StructuredTextNS);
                constant_value.InnerText = value;
                constant.AppendChild(constant_value);
                access.AppendChild(constant);
                return access;
            }

            public XmlElement Blank(int num = 1)
            {
                XmlElement elem = doc.CreateElement("Blank", StructuredTextNS);
                elem.SetAttribute("Num", num.ToString());
                return elem;
            }

            public XmlElement NewLine(int num = 1)
            {
                XmlElement elem = doc.CreateElement("NewLine", StructuredTextNS);
                elem.SetAttribute("Num", num.ToString());
                return elem;
            }
            public XmlElement Symbol()
            {
                XmlElement symbol = doc.CreateElement("Symbol", StructuredTextNS);
                return symbol;
            }

            // Add a '.' if previous element was a component
            public void AddComponentSep(XmlElement symbol)
            {
                XmlNode prev = symbol.LastChild;
                if (prev != null && prev is XmlElement)
                {
                    XmlElement elem = (XmlElement)prev;
                    if (elem.Name == "Component")
                    {
                        symbol.AppendChild(Token("."));
                    }
                }
            }

            public void SymbolAddComponent(XmlElement symbol, XmlElement component)
            {
                AddComponentSep(symbol);
                symbol.AppendChild(component);
            }

            public void SymbolAddComponents(XmlElement symbol, TagComponent component)
            {
                
                LinkedList<TagComponent> list = new LinkedList<TagComponent>();
                while (component != null)
                {
                    list.AddFirst(component);
                    component = component.Parent;
                }
                bool first = true;
                foreach (TagComponent c in list)
                {
                    XmlElement[] xml_indices = null;
                    if (!first)
                    {
                        symbol.AppendChild(Token("."));
                    } else
                    {
                        AddComponentSep(symbol);
                        first = false;
                    }
                  
                    if (c is ArrayComponent)
                    {
                        int[] indices = null;
                        indices = ((ArrayComponent)c).Indices;
                        xml_indices = new XmlElement[indices.Length];
                        for (int i = 0; i < indices.Length; i++)
                        {
                            xml_indices[i] = LiteralConstant(indices[i].ToString());
                        }
                    }
                  
                    symbol.AppendChild( Component(c.Name, xml_indices));
                }
               
            }
        }
    }
}
