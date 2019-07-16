using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TIAEKtool
{
    public class ParseText
    {
        public class FieldInfo
        {
            public string TagName;
            public string DisplayType;
            public int Length;
            public string FormatPattern;
            public override bool Equals(object obj)
            {
                if (!(obj is FieldInfo))
                {
                    return false;
                }
                FieldInfo field = (FieldInfo)obj;
                return field.TagName == TagName
                    && field.DisplayType == DisplayType
                    && field.Length == Length
                    && field.FormatPattern == FormatPattern;
            }
            public override int GetHashCode()
            {
                return TagName.GetHashCode() ^ DisplayType.GetHashCode() ^ Length ^ FormatPattern.GetHashCode();
            }

        }

        public static XmlElement ParseTextToTextElement(XmlDocument doc, string text, ref List<FieldInfo> fields)
        {

            XmlElement text_elem = doc.CreateElement("Text");
            XmlElement body_elem = doc.CreateElement("body");
            text_elem.AppendChild(body_elem);
            XmlElement p_elem = doc.CreateElement("p");
            body_elem.AppendChild(p_elem);
            p_elem.InnerXml = text;
            int refno = 0;
            bool existing_fields = true;
            if (fields == null)
            {
                fields = new List<FieldInfo>();
                existing_fields = false;
            }
            XmlNodeList tag_elems = p_elem.SelectNodes("hmitag");
            if (existing_fields && tag_elems.Count != fields.Count)
            {
                throw new Exception("All languages must have the same number of <hmitag> elements.");
            }
            foreach (XmlNode tag_node in tag_elems) {
                XmlElement tag_elem = (XmlElement)tag_node;
                FieldInfo field = new FieldInfo();
                field.TagName = tag_elem.GetAttribute("name");
                field.DisplayType = tag_elem.GetAttribute("type");
                field.Length = int.Parse(tag_elem.GetAttribute("length"));
                field.FormatPattern = tag_elem.GetAttribute("pattern");
                if (field.FormatPattern == "")
                {
                    switch (field.DisplayType.ToLower())
                    {
                        case "text":
                            field.FormatPattern = new string('?', field.Length);
                            break;
                        case "int":
                            field.FormatPattern = "s" + new string('9', field.Length);
                            break;
                    }
                }

                XmlElement field_elem = doc.CreateElement("field");
              
                if (existing_fields)
                {
                    refno = fields.IndexOf(field);
                    if (refno < 0)
                    {
                        throw new Exception("All languages must have compatible <hmitag> elements.");
                    }
                }
                else
                {
                    fields.Add(field);
                   
                }
                field_elem.SetAttribute("ref", refno.ToString());
                p_elem.ReplaceChild(field_elem, tag_elem);
                refno++;
            }
            if (fields.Count > 0)
            {
                refno = 0;
                XmlElement fieldinfos_elem = doc.CreateElement("fieldinfos");
                text_elem.AppendChild(fieldinfos_elem);
                foreach (FieldInfo field in fields)
                {
                    XmlElement fieldinfo_elem = doc.CreateElement("fieldinfo");
                    fieldinfos_elem.AppendChild(fieldinfo_elem);
                    fieldinfo_elem.SetAttribute("name", refno.ToString());
                    refno++;
                    fieldinfo_elem.SetAttribute("domaintype", "HMICommonTagDisplayFormat");
                    XmlElement reference_elem = doc.CreateElement("reference");
                    fieldinfo_elem.AppendChild(reference_elem);
                    reference_elem.SetAttribute("TargetID", "@OpenLink");
                    XmlElement name_elem = doc.CreateElement("name");
                    reference_elem.AppendChild(name_elem);
                    name_elem.InnerText = "Siemens.Simatic.Hmi.Utah.Tag.HmiTag:" + field.TagName;
                    XmlElement domaindata_elem = doc.CreateElement("domaindata");
                    fieldinfo_elem.AppendChild(domaindata_elem);
                    XmlElement format_elem = doc.CreateElement("format");
                    domaindata_elem.AppendChild(format_elem);
                    format_elem.SetAttribute("displaytype", field.DisplayType);
                    format_elem.SetAttribute("length", field.Length.ToString());
                    format_elem.SetAttribute("formatpattern", field.FormatPattern);
                }
            }
            return text_elem;
        }
    }
}
