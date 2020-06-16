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
        XmlElement template_objects;
        XmlDocument obj_template;
        public PresetPopup(XmlDocument doc, XmlDocument obj_template)
        {
            this.doc = doc;
            this.obj_template = obj_template;
            XMLUtil.CollectID(doc.DocumentElement, idset);
            screen_objects = doc.DocumentElement.SelectSingleNode("/Document/Hmi.Screen.ScreenPopup/ObjectList") as XmlElement;
            if (screen_objects == null) throw new Exception("List of layers not found in screen popup XML.");
            template_objects = obj_template.SelectSingleNode("/Document/Hmi.Screen.ScreenTemplate/ObjectList") as XmlElement;
            if (template_objects == null) throw new Exception("List of layers not found in ´template XML.");
        }

        const string GROUP_PREFIX = "PresetGroup_";
        const string ENABLE_SWITCH_PREFIX = "PresetEnable_";
        const string VALUE_FIELD_PREFIX = "PresetValue_";
        const string DESCRIPTION_FIELD_PREFIX = "PresetDescription_";

        bool GetElementIntValue(XmlElement parent, string path, out int value)
        {
            XmlElement value_elem = parent.SelectSingleNode(path) as XmlElement;
            if (value_elem == null || value_elem.InnerText == null)
            {
                value = 0;
                return false;
            }
            return int.TryParse(value_elem.InnerText, out value);
        }

        void AdjustElementIntValue(XmlElement parent, string path, int offset)
        {
            XmlElement value_elem = parent.SelectSingleNode(path) as XmlElement;
            if (value_elem == null || value_elem.InnerText == null)
            {
                return;
            }
            if (!int.TryParse(value_elem.InnerText, out int value)) return;
            value_elem.InnerText = (value + offset).ToString();
        }

        class BoundingBox
        {
            public int Right { get; private set; } = int.MinValue;
            public int Bottom { get; private set; } = int.MinValue;

            public int Left { get; private set; } = int.MaxValue;
            public int Top { get; private set; } = int.MaxValue;
            public int Width { get => Right - Left; }
            public int Height { get => Bottom - Top; }

            public BoundingBox()
            {
            }

            public BoundingBox(int left, int top, int width, int height)
            {
                Left = left;
                Top = top;
                Right = left + width;
                Bottom = top + height;
            }
            public void Expand(int left, int top, int width, int height)
            {
                if (top < Top) Top = top;
                if (top + height > Bottom) Bottom = top + height;
                if (left < Left) Left = left;
                if (left + width > Right) Right = left + width;
            }

            public void Expand(BoundingBox bbox)
            {
                if (bbox.Top < Top) Top = bbox.Top;
                if (bbox.Bottom > Bottom) Bottom = bbox.Bottom;
                if (bbox.Left < Left) Left = bbox.Left;
                if (bbox.Right > Right) Right = bbox.Right;
            }

        }
        BoundingBox CalculateBoundingBox(XmlElement obj)
        {

            if (obj.Name == "Hmi.Screen.Group")
            {
                XmlElement list = obj.SelectSingleNode("ObjectList") as XmlElement;
                if (list != null)
                {
                    BoundingBox group_box = new BoundingBox();
                    foreach (XmlNode node in list.ChildNodes)
                    {
                        XmlElement child = node as XmlElement;
                        if (child != null)
                        {
                            BoundingBox bbox = CalculateBoundingBox(child);

                            if (bbox == null)
                            {
                                return null;
                            }
                            group_box.Expand(bbox);
                        }
                    }
                    return group_box;
                }
            }
            else
            {
                XmlElement attrs = obj.SelectSingleNode("AttributeList") as XmlElement;

                if (attrs != null)
                {
                    int top;
                    int left;
                    int width;
                    int height;
                    if (GetElementIntValue(attrs, "Top", out top)
                        && GetElementIntValue(attrs, "Left", out left)
                         && GetElementIntValue(attrs, "Width", out width)
                          && GetElementIntValue(attrs, "Height", out height))
                    {


                        // Expand bounding box if necessary

                        return new BoundingBox(left, top, width, height);
                    }
                }
            }
            return null;
        }

        void SetObjectName(XmlElement obj, string name)
        {
            XmlElement name_elem = obj.SelectSingleNode("AttributeList/ObjectName") as XmlElement;
            if (name_elem != null)
            {
                name_elem.InnerXml = name;
            }
        }

        static readonly char[] digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        void SetObjectNameSuffix(XmlElement obj, string suffix)
        {
            XmlElement name_elem = obj.SelectSingleNode("AttributeList/ObjectName") as XmlElement;
            if (name_elem != null)
            {
                name_elem.InnerText = name_elem.InnerText.TrimEnd(digits) + suffix;
            }
        }

        void SetChildNameSuffix(XmlElement parent, string suffix)
        {
            if (parent.Name == "Hmi.Screen.Group")
            {
                XmlElement list = parent.SelectSingleNode("ObjectList") as XmlElement;
                if (list != null)
                {
                    foreach (XmlNode node in list.ChildNodes)
                    {
                        XmlElement child = node as XmlElement;
                        if (child != null)
                        {
                            SetObjectNameSuffix(child, suffix);
                            SetChildNameSuffix(child, suffix);
                        }
                    }
                }
            }
        }

        void MoveObject(XmlElement obj, int x_offset, int y_offset)
        {
            XmlElement attrs = obj.SelectSingleNode("AttributeList") as XmlElement;

            if (attrs != null)
            {
                AdjustElementIntValue(attrs, "Top", y_offset);
                AdjustElementIntValue(attrs, "Left", x_offset);
            }
        }

        void MoveChildren(XmlElement parent, int x_offset, int y_offset)
        {
            if (parent.Name == "Hmi.Screen.Group")
            {
                XmlElement list = parent.SelectSingleNode("ObjectList") as XmlElement;
                if (list != null)
                {
                    foreach (XmlNode node in list.ChildNodes)
                    {
                        XmlElement child = node as XmlElement;
                        if (child != null)
                        {
                            MoveObject(child, x_offset, y_offset);
                            MoveChildren(child, x_offset, y_offset);
                        }
                    }
                }
            }
        }



        public void AddEnableSelection(string template, string preset_group, int index, MultilingualText description, string unit, int precision)
        {

            XmlElement template_group = template_objects.SelectSingleNode("Hmi.Screen.ScreenLayer/ObjectList/Hmi.Screen.Group[AttributeList/ObjectName/text()='" + template + "']") as XmlElement;
            if (template_group == null) throw new Exception("No group named " + template + "was found in ObjectTemplate");
            XmlElement new_group = doc.ImportNode(template_group, true) as XmlElement;
            BoundingBox new_bbox = CalculateBoundingBox(new_group);
            if (new_bbox == null) throw new Exception("Failed to calculate bounding box of new preset group");

            string name_suffix = preset_group + "_" + index.ToString();
            string group_name = GROUP_PREFIX + name_suffix;
            SetObjectName(new_group, group_name);
            SetChildNameSuffix(new_group, name_suffix);

            /* TODO change name of all objects in new_group */
            XmlElement old_group = screen_objects.SelectSingleNode("Hmi.Screen.ScreenLayer//Hmi.Screen.Group[AttributeList/ObjectName/text()='" + group_name + "']") as XmlElement;
            if (old_group != null)
            {
                BoundingBox old_bbox = CalculateBoundingBox(old_group);
                if (old_bbox == null) throw new Exception("Failed to calculate bounding box of old preset group");
                MoveChildren(new_group, old_bbox.Left - new_bbox.Left, old_bbox.Top - new_bbox.Top);

                old_group.ParentNode.ReplaceChild(new_group, old_group);

            }
            else
            {
                string prev_group_name = GROUP_PREFIX + preset_group + "_" + (index - 1).ToString();
                XmlElement prev_group = screen_objects.SelectSingleNode("Hmi.Screen.ScreenLayer//Hmi.Screen.Group[AttributeList/ObjectName/text()='" + prev_group_name + "']") as XmlElement;
                if (prev_group != null)
                {
                    BoundingBox prev_bbox = CalculateBoundingBox(prev_group);
                    MoveChildren(new_group, prev_bbox.Left - new_bbox.Left, prev_bbox.Bottom - new_bbox.Top);
                    prev_group.ParentNode.InsertAfter(new_group, prev_group);

                }
                else
                {
                    XmlElement first_layer = screen_objects.SelectSingleNode("Hmi.Screen.ScreenLayer") as XmlElement;
                    if (first_layer == null) throw new Exception("No layers found");
                    first_layer.AppendChild(new_group);

                }
            }
            XMLUtil.ReplaceID(new_group, idset);

            string field_name = DESCRIPTION_FIELD_PREFIX + name_suffix;
            XmlElement description_field = new_group.SelectSingleNode(".//Hmi.Screen.TextField[AttributeList/ObjectName/text()='" + field_name + "']") as XmlElement;
            if (description_field == null)
            {
                throw new Exception("No text field named " + field_name + " found in group " + group_name);

            }
            List<ParseText.FieldInfo> fields = null;
            foreach (string culture in description.Cultures)
            {
                XmlElement text_elem = description_field.SelectSingleNode("ObjectList/MultilingualText/ObjectList/MultilingualTextItem/AttributeList[Culture/text()='" + culture + "']/Text") as XmlElement;
                if (text_elem != null)
                {
                    XmlElement parsed = ParseText.ParseTextToTextElement(doc, description[culture], ref fields);
                    text_elem.ParentNode.ReplaceChild(parsed, text_elem);
                }
            }

            string enable_name = ENABLE_SWITCH_PREFIX + name_suffix;
            XmlElement enable_switch = new_group.SelectSingleNode(".//Hmi.Screen.Switch[AttributeList/ObjectName/text()='" + enable_name + "']") as XmlElement;
            if (enable_switch == null)
            {
                throw new Exception("No switch named " + enable_name + " found in group " + group_name);
            }

            XmlElement tag_name_elem = enable_switch.SelectSingleNode("ObjectList/Hmi.Screen.Property/ObjectList/Hmi.Dynamic.TagConnectionDynamic/LinkList/Tag/Name") as XmlElement;
            if (tag_name_elem != null)
            {
                tag_name_elem.InnerText = enable_name;
            }
            else
            {
                throw new Exception("Tag name element not found for switch " + enable_name + " in group " + group_name);
            }

            string value_name = VALUE_FIELD_PREFIX + name_suffix;
            XmlElement value_field = new_group.SelectSingleNode(".//node()[AttributeList/ObjectName/text()='" + value_name + "']") as XmlElement;
            if (value_field != null)
            {

                tag_name_elem = value_field.SelectSingleNode("ObjectList/Hmi.Screen.Property/ObjectList/Hmi.Dynamic.TagConnectionDynamic/LinkList/Tag/Name") as XmlElement;
                if (tag_name_elem != null)
                {
                    tag_name_elem.InnerText = value_name;
                }
                else
                {
                    throw new Exception("Tag name element not found for " + value_name + " in group " + group_name);
                }


                XmlElement unit_elem = value_field.SelectSingleNode("AttributeList/Unit") as XmlElement;
                if (unit_elem != null)
                {
                    if (unit == null)
                    {
                        unit_elem.IsEmpty = true;
                    }
                    else
                    {
                        unit_elem.InnerText = unit;
                    }
                }

                string format_pattern = "999999";
                if (precision > 0)
                {
                    if (precision > 5) precision = 5;
                    format_pattern += ".99999".Substring(0, precision + 1);
                }
                XmlElement format_elem = value_field.SelectSingleNode("AttributeList/FormatPattern") as XmlElement;
                if (format_elem != null)
                {

                    format_elem.InnerText = format_pattern;
                }
                XmlElement field_length_elem = value_field.SelectSingleNode("AttributeList/FieldLength") as XmlElement;
                if (field_length_elem != null)
                {

                    field_length_elem.InnerText = (format_pattern.Length).ToString();
                }

                XmlElement text_list_elem = value_field.SelectSingleNode("LinkList/TextList/Name") as XmlElement;
                if (text_list_elem != null)
                {
                    text_list_elem.InnerText = "PresetTextList_" + name_suffix;
                }
            }
        }
        public String RemoveEnableSelection(string preset_group, int index)
        {
            string name_suffix = preset_group + "_" + index.ToString();
            string group_name = GROUP_PREFIX + name_suffix;

            XmlElement old_group = screen_objects.SelectSingleNode("Hmi.Screen.ScreenLayer//Hmi.Screen.Group[AttributeList/ObjectName/text()='" + group_name + "']") as XmlElement;
            if (old_group == null) return null;
            old_group.ParentNode.RemoveChild(old_group);
            return group_name;
        }
    }
}
