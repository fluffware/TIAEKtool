using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TIAEKtool
{
    public class ParseTextUnified
    {
        public enum AlignmentType { Right,Left};
        public class FieldInfo
        {
            public string TagName;
            public string DisplayType;
            public int Length;
            public int Precision;
            public AlignmentType Alignment;
            public bool ZeroPadding;
           
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
                    && field.Precision == Precision
                    && field.Alignment == Alignment
                    && field.ZeroPadding == ZeroPadding;
            }
            public override int GetHashCode()
            {
                return TagName.GetHashCode() ^ DisplayType.GetHashCode() ^ Length
                       ^ Precision ^ Alignment.GetHashCode()
                       ^ ZeroPadding.GetHashCode();
            }

        }
        /*
        <body><p>Larm 3
        <field ref="0" />
        </p></body>
        <fieldinfos>
        <fieldinfo name = "0" domaintype="HMIAlarmParameter1WithoutCommonTextList">
        <reference TargetID = "@OpenLink" >
        < name > Siemens.Simatic.Hmi.Utah.Alarm.HmiDiscreteAlarm:sDB_Larm_Test_Larm3</name>
        </reference>
        <subreference />
        <domaindata><format displaytype = "Decimal" length="5" precision="3" alignment="Right" zeropadding="True" /></domaindata>
        </fieldinfo>
        </fieldinfos>
        */
        static string GetAttributeDefault(XmlElement elem, string name, string def)
        {
            string v = elem.GetAttribute(name);
            if (v !="") return v;
            else return def;
        }
        public static string ParseTextToText(string text, ref List<FieldInfo> fields)
        {
            var doc = new XmlDocument();
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
                throw new Exception("All languages must have the same number of <hmitag> elements. Failed for '"+text+"'. "+
                    "Expected "+ fields.Count + " tags, found "+ tag_elems.Count);
            }
            foreach (XmlNode tag_node in tag_elems) {
                XmlElement tag_elem = (XmlElement)tag_node;
                FieldInfo field = new FieldInfo();
                field.TagName = tag_elem.GetAttribute("name");
                field.DisplayType = GetAttributeDefault(tag_elem,"type","Decimal");
                field.Length = int.Parse(GetAttributeDefault(tag_elem,"length","5"));
                field.Precision = int.Parse(GetAttributeDefault(tag_elem, "precision", "0"));
                field.Alignment = GetAttributeDefault(tag_elem, "alignment","left").ToLower() == "right" ? AlignmentType.Right : AlignmentType.Left;
                field.ZeroPadding = bool.Parse(GetAttributeDefault(tag_elem, "zeropadding", "false"));
          

                XmlElement field_elem = doc.CreateElement("field");
              
                if (existing_fields)
                {
                    refno = fields.IndexOf(field);
                    if (refno < 0)
                    {
                        throw new Exception("All languages must have compatible <hmitag> elements. Failed for '" + text + "'.");
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
                    fieldinfo_elem.SetAttribute("domaintype", "HMIAlarmParameter1WithoutCommonTextList");
                    XmlElement reference_elem = doc.CreateElement("reference");
                    fieldinfo_elem.AppendChild(reference_elem);
                    XmlElement subreference_elem = doc.CreateElement("subreference");
                    fieldinfo_elem.AppendChild(subreference_elem);
                    reference_elem.SetAttribute("TargetID", "@OpenLink");
                    XmlElement name_elem = doc.CreateElement("name");
                    reference_elem.AppendChild(name_elem);
                    name_elem.InnerText = "Siemens.Simatic.Hmi.Utah.Alarm.HmiDiscreteAlarm:" + field.TagName;
                    XmlElement domaindata_elem = doc.CreateElement("domaindata");
                    fieldinfo_elem.AppendChild(domaindata_elem);
                    XmlElement format_elem = doc.CreateElement("format");
                    domaindata_elem.AppendChild(format_elem);
                    format_elem.SetAttribute("displaytype", field.DisplayType);
                    format_elem.SetAttribute("length", field.Length.ToString());
                    format_elem.SetAttribute("alignment", field.Alignment.ToString()); 
                    format_elem.SetAttribute("precision", field.Precision.ToString());
                    format_elem.SetAttribute("zeropadding", field.ZeroPadding.ToString());
                }
            }
            /*
            // Wrap all texts with <p> </p>
            foreach (XmlNode node in body_elem.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    XmlElement p_elem = doc.CreateElement("p");
                    XmlNode text_node = body_elem.ReplaceChild(p_elem, node);
                    p_elem.AppendChild(text_node);
                    
                }
            }
          */
                return text_elem.InnerXml;
        }
    }
}
