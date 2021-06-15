using PLC.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TIAEKtool
{
    class HMITagTable
    {
        IntSet idset = new IntSet();
        XmlElement tag_list;
        public HMITagTable(XmlDocument doc)
        {
            XMLUtil.CollectID(doc.DocumentElement, idset);
            tag_list = doc.SelectSingleNode("/Document/Hmi.Tag.TagTable/ObjectList") as XmlElement;
            if (tag_list == null) throw new Exception("No list of tags found in tag table XML");
        }


        /// <summary>
        /// Update a tag named prefix + index
        /// Create one if it doesn't exist, using tag prefix + (index-1) as template.
        /// </summary>
        /// <param name="prefix">Start of tag name. Typically ends with '_'</param>
        /// <param name="index">Index added to end of name</param>
        /// <param name="plc_tag">PLC tag to connect to</param>
        public void AddIndexedTag(string prefix, int index, string plc_tag, DataType type = null, double? min = null, double? max = null)
        {
            string tag_name = prefix + index.ToString();
            XmlElement tag = tag_list.SelectSingleNode("Hmi.Tag.Tag[AttributeList/Name/text()='" + tag_name+"']") as XmlElement;
            if (tag == null)
            {
                string template_name = prefix + (index - 1).ToString();
                XmlElement template = tag_list.SelectSingleNode("Hmi.Tag.Tag[AttributeList/Name/text()='" + template_name + "']") as XmlElement;
                if (template == null) throw new Exception("No tag " + template_name + " to use as template for tag " + tag_name);
                tag = template.CloneNode(true) as XmlElement;
                tag_list.InsertAfter(tag, template);
                XMLUtil.ReplaceID(tag, idset);

                // Change name
                XmlElement name_elem = tag.SelectSingleNode("AttributeList/Name") as XmlElement;
                name_elem.InnerText = tag_name;

            }

            // Erase all type information and let the import figure it out
            XmlElement attr_list = tag.SelectSingleNode("AttributeList") as XmlElement;
            XmlElement length_elem = attr_list.SelectSingleNode("Length") as XmlElement;
            if (length_elem != null)
            {
                attr_list.RemoveChild(length_elem);
            }
            if (type != null)
            {
                XmlElement coding_elem = attr_list.SelectSingleNode("Coding") as XmlElement;
                if (coding_elem != null)
                {
                    if (type is REAL || type is LREAL)
                    {
                        coding_elem.InnerText = "IEEE754Float";
                    } else
                    {
                        coding_elem.InnerText = "Binary";
                    }
                }
            }


            XmlElement upper_elem = attr_list.SelectSingleNode("LimitUpper2") as XmlElement;
            if (upper_elem != null)
            {
                attr_list.RemoveChild(upper_elem);
            }
            if (max is double max_limit)
            {
                upper_elem = attr_list.OwnerDocument.CreateElement("LimitUpper2");
                upper_elem.SetAttribute("Type", "Siemens.Engineering.Hmi.ConstValue");
                upper_elem.InnerText = max_limit.ToString();
                attr_list.AppendChild(upper_elem);
            }

            XmlElement lower_elem = attr_list.SelectSingleNode("LimitLower2") as XmlElement;
            if (lower_elem != null)
            {
                attr_list.RemoveChild(lower_elem);
            }
            if (min is double min_limit)
            {
                lower_elem = attr_list.OwnerDocument.CreateElement("LimitLower2");
                lower_elem.SetAttribute("Type", "Siemens.Engineering.Hmi.ConstValue");
                lower_elem.InnerText = min_limit.ToString();
                attr_list.AppendChild(lower_elem);
            }

            XmlElement link_list = tag.SelectSingleNode("LinkList") as XmlElement;
            XmlElement hmi_type_elem = link_list.SelectSingleNode("HmiDataType") as XmlElement;
            if (hmi_type_elem != null)
            {
                link_list.RemoveChild(hmi_type_elem);
            }
            XmlElement type_elem = link_list.SelectSingleNode("DataType") as XmlElement;
            if (type_elem != null)
            {
                link_list.RemoveChild(type_elem);
            }


            // Set PLC tag
            XmlElement controller_tag_elem = tag.SelectSingleNode("LinkList/ControllerTag/Name") as XmlElement;
            if (controller_tag_elem == null) throw new Exception("No PLC tag for HMI tag " + tag_name);
            controller_tag_elem.InnerText = plc_tag;
        }

    }
}
