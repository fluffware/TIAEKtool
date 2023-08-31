using Siemens.Engineering;
using Siemens.Engineering.Hmi.TextGraphicList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TIAEKtool.Properties;

namespace TIAEKtool
{
    public class CreateHmiTextListTask : SequentialTask
    {
        readonly TiaPortal portal;
        readonly TextListComposition text_lists;
        readonly string list_name;
        readonly IDictionary<int, MultilingualText> texts;
        private readonly IEnumerable<String> cultures;
       


        public CreateHmiTextListTask(TiaPortal portal, string list_name, TextListComposition text_lists, IDictionary<int, MultilingualText> texts)
        {
            this.portal = portal;
            this.text_lists = text_lists;
            this.list_name = list_name;
            this.texts = texts;

            Project proj = portal.Projects[0];
            LanguageAssociation langs = proj.LanguageSettings.ActiveLanguages;
            cultures = langs.Select(l => l.Culture.Name);


            Description = "Create HMI text list " + list_name;
        }

        protected XmlElement ListEntry(XmlDocument doc, int value, bool def, MultilingualText text)
        {
            XmlElement entry = doc.CreateElement("Hmi.TextGraphicList.TextListEntry");
            entry.SetAttribute("ID", "0");
            entry.SetAttribute("CompositionName", "Entries");

            XmlElement attr_list = doc.CreateElement("AttributeList");
            entry.AppendChild(attr_list);

            XmlElement default_entry = doc.CreateElement("DefaultEntry");
            default_entry.InnerText = def ? "true" : "false";
            attr_list.AppendChild(default_entry);

            XmlElement entry_type = doc.CreateElement("EntryType");
            entry_type.InnerText = "SingleValue";
            attr_list.AppendChild(entry_type);

            XmlElement from = doc.CreateElement("From");
            from.InnerText = value.ToString();
            attr_list.AppendChild(from);

            XmlElement to = doc.CreateElement("To");
            to.InnerText = value.ToString();
            attr_list.AppendChild(to);

            XmlElement obj_list = doc.CreateElement("ObjectList");
            entry.AppendChild(obj_list);

            XmlElement multi_text = doc.CreateElement("MultilingualText");
            multi_text.SetAttribute("ID", "0");
            multi_text.SetAttribute("CompositionName", "Text");
            obj_list.AppendChild(multi_text);

            XmlElement obj_list2 = doc.CreateElement("ObjectList");
            multi_text.AppendChild(obj_list2);

            foreach (string culture in text.Cultures)
            {
                XmlElement text_item = doc.CreateElement("MultilingualTextItem");
                text_item.SetAttribute("ID", "0");
                text_item.SetAttribute("CompositionName", "Items");
                obj_list2.AppendChild(text_item);

                XmlElement attr_list2 = doc.CreateElement("AttributeList");
                text_item.AppendChild(attr_list2);

                XmlElement culture_elem = doc.CreateElement("Culture");
                culture_elem.InnerText = culture;
                attr_list2.AppendChild(culture_elem);

                List<ParseText.FieldInfo> fields = null;
                XmlElement parsed_text = ParseText.ParseTextToTextElement(doc, text[culture], ref fields);
                attr_list2.AppendChild(parsed_text);


            }
            return entry;
        }

        protected override void DoWork()
        {
            lock (portal)
            {
               
                try
                {
                    IntSet idset = new IntSet();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(Resources.InitialHMITextList);
                    XMLUtil.CollectID(doc.DocumentElement, idset);


                    XmlElement name_attr = doc.SelectSingleNode("/Document/Hmi.TextGraphicList.TextList/AttributeList/Name") as XmlElement;
                    name_attr.InnerText = list_name;

                    XmlElement entry_list = doc.SelectSingleNode("/Document/Hmi.TextGraphicList.TextList/ObjectList") as XmlElement;


                    {
                        MultilingualText text = new MultilingualText();
                        foreach (string culture in cultures)
                        {
                            text.AddText(culture, "?");
                        }
                        XmlElement entry = ListEntry(doc, 0, true, text);

                    }

                    List<int> keys = new List<int>(texts.Keys);
                    keys.Sort();
                    foreach (int k in keys)
                    {
                        XmlElement entry = ListEntry(doc, k, false, texts[k]);
                        entry_list.AppendChild(entry);
                        XMLUtil.ReplaceID(entry, idset);
                    }

                    TIAutils.ImportTextListXML(doc, text_lists);
                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed create HMI text list:\n" + ex.Message);
                    return;
                }

            }

        }

    }
}