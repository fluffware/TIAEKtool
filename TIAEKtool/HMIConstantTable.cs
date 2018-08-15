using PLC.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TIAEKtool.Properties;

namespace TIAEKtool
{
    class HMIConstantTable
    {
        IntSet idset = new IntSet();
        XmlElement tag_list;
        XmlDocument doc;
        XmlElement template_int;

        public XmlDocument Document { get => doc; }

        public HMIConstantTable(string table_name)
        {
            doc = new XmlDocument();
            doc.LoadXml(Resources.InitialHMIConstantTags);
            XmlElement name_elem = doc.SelectSingleNode("/Document/Hmi.Tag.TagTable/AttributeList/Name") as XmlElement;
            name_elem.InnerText = table_name;
            tag_list = doc.SelectSingleNode("/Document/Hmi.Tag.TagTable/ObjectList") as XmlElement;
            if (tag_list == null) throw new Exception("No list of tags found in tag table XML");
            XmlElement tag;
            tag = tag_list.SelectSingleNode("Hmi.Tag.Tag[AttributeList/Name/text()='TemplateInt']") as XmlElement;
            if (tag == null) throw new Exception("Failed to find a tag named TemplateInt");
            template_int = tag_list.RemoveChild(tag) as XmlElement;
            XMLUtil.CollectID(doc.DocumentElement, idset);


        }

        

        public void AddIntegerConstant(string tag_name, int value)
        {
            XmlElement tag = template_int.Clone() as XmlElement;
            // Change name
            XmlElement name_elem = tag.SelectSingleNode("AttributeList/Name") as XmlElement;
            name_elem.InnerText = tag_name;
            // Change start value
            XmlElement start_value_elem = tag.SelectSingleNode("AttributeList/StartValue") as XmlElement;
            start_value_elem.InnerText = value.ToString();

            XMLUtil.ReplaceID(tag, idset);
            tag_list.AppendChild(tag);
        }
    }
}
