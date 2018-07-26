using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TIAEKtool
{
    public class PresetPopup
    {
        XmlDocument doc;
        IntSet idset = new IntSet();
        XmlElement screen_objects;
        public PresetPopup(XmlDocument doc)
        {
            this.doc = doc;
            XMLUtil.CollectID(doc.DocumentElement, idset);
            screen_objects = doc.DocumentElement.SelectSingleNode("/Document/Hmi.Screen.ScreenPopup/ObjectList") as XmlElement;
            if (screen_objects == null) throw new Exception("List of layers not found in screen popup XML.");
        }

        const string ENABLE_SWITCH_PREFIX = "PresetEnable_";
        const string DESCRIPTION_FIELD_PREFIX = "PresetDescription_";
        public void AddEnableSelection(string group, int index, MultilingualText description)
        {
            int max_size = 5;

            string field_name = DESCRIPTION_FIELD_PREFIX + group + "_" + index.ToString();
            XmlElement description_field = screen_objects.SelectSingleNode("Hmi.Screen.ScreenLayer//Hmi.Screen.TextField[AttributeList/ObjectName/text()='" + field_name + "']") as XmlElement;
            if (description_field == null)
            {
                string name = DESCRIPTION_FIELD_PREFIX + group + "_" + (index - 1).ToString();
                XmlElement template_field = screen_objects.SelectSingleNode("Hmi.Screen.ScreenLayer//Hmi.Screen.TextField[AttributeList/ObjectName/text()='" + name + "']") as XmlElement;
                if (template_field == null) throw new Exception("Couldn't find " + name + " to use as template for " + field_name);
                description_field = (XmlElement)template_field.CloneNode(true);
                template_field.ParentNode.InsertAfter(description_field, template_field);
                XMLUtil.ReplaceID(description_field, idset);
                XmlElement name_attr = description_field.SelectSingleNode("AttributeList/ObjectName") as XmlElement;
                name_attr.InnerText = field_name;

                XmlNodeList font_sizes = description_field.SelectNodes("ObjectList/Hmi.Globalization.MultiLingualFont/ObjectList/Hmi.Globalization.FontItem/AttributeList/FontSize");

                foreach (XmlNode fs in font_sizes)
                {
                    int size;
                    if (int.TryParse(fs.InnerText, out size))
                    {
                        if (size > max_size) max_size = size;
                    }
                }
                XmlElement top_attr = description_field.SelectSingleNode("AttributeList/Top") as XmlElement;
                int top_coord;
                if (top_attr != null && int.TryParse(top_attr.InnerText, out top_coord))
                {
                    top_attr.InnerText = (top_coord + max_size * 3 / 2).ToString();
                }
            }
            foreach (string culture in description.Cultures)
            {
                XmlElement text_p_elem = description_field.SelectSingleNode("ObjectList/MultilingualText/ObjectList/MultilingualTextItem/AttributeList[Culture/text()='" + culture + "']/Text/body/p") as XmlElement;
                if (text_p_elem != null)
                {
                    text_p_elem.InnerText = description[culture];
                }
            }

            string enable_name = ENABLE_SWITCH_PREFIX + group + "_" + index.ToString();
            XmlElement enable_switch = screen_objects.SelectSingleNode("Hmi.Screen.ScreenLayer//Hmi.Screen.Switch[AttributeList/ObjectName/text()='" + enable_name + "']") as XmlElement;
            if (enable_switch == null)
            {
                string name = ENABLE_SWITCH_PREFIX + group + "_" + (index - 1).ToString();
                XmlElement template = screen_objects.SelectSingleNode("Hmi.Screen.ScreenLayer//Hmi.Screen.Switch[AttributeList/ObjectName/text()='" + name + "']") as XmlElement;
                if (template == null) throw new Exception("Couldn't find " + name + " to use as template for " + enable_name);
                enable_switch = (XmlElement)template.CloneNode(true);
                template.ParentNode.InsertAfter(enable_switch, template);
                XMLUtil.ReplaceID(enable_switch, idset);
                XmlElement name_attr = enable_switch.SelectSingleNode("AttributeList/ObjectName") as XmlElement;
                name_attr.InnerText = enable_name;


                XmlElement top_attr = enable_switch.SelectSingleNode("AttributeList/Top") as XmlElement;
                int top_coord;
                if (top_attr != null && int.TryParse(top_attr.InnerText, out top_coord))
                {
                    top_attr.InnerText = (top_coord + max_size * 3 / 2).ToString();
                }
            }

            XmlElement tag_name_elem = enable_switch.SelectSingleNode("ObjectList/Hmi.Screen.Property/ObjectList/Hmi.Dynamic.TagConnectionDynamic/LinkList/Tag/Name") as XmlElement;
            if (tag_name_elem != null)
            {
                tag_name_elem.InnerText = enable_name;
            }


        }
    }
}
