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
        public void AddIndexedTag(string prefix, int index, string plc_tag)
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

            // Set PLC tag
            XmlElement controller_tag_elem = tag.SelectSingleNode("LinkList/ControllerTag/Name") as XmlElement;
            controller_tag_elem.InnerText = plc_tag;
        }

    }
}
