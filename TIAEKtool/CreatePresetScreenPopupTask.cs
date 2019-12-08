using PLC.Types;
using Siemens.Engineering;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.Xml;

namespace TIAEKtool
{
    public class CreatePresetScreenPopupTask : SequentialTask
    {
        TiaPortal portal;
       ScreenPopupFolder folder;
        string popupName;
        string groupName;
        XmlDocument templates;
        IList<PresetTag> tags;
        public CreatePresetScreenPopupTask(TiaPortal portal, IList<PresetTag> tags, ScreenPopupFolder folder, XmlDocument templates, string popup_name, string group_name)
        {
            this.portal = portal;
            this.tags = tags;
            this.folder = folder;
            this.templates = templates;
            popupName = popup_name;
            groupName = group_name;
            Description = "Create preset popup screen " + popupName;
        }

        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {

                 
                    ScreenPopup popup = folder.ScreenPopups.Find(popupName);

                    if (popup != null)
                    {

                        XmlDocument popup_doc = TIAutils.ExportScreenPopupXML(popup);
                        PresetPopup editor = new PresetPopup(popup_doc, templates);
                        int index = 1;

                        foreach (var tag in tags)
                        {
                            DataType type = tag.tagPath.Type;
                            string template;
                            if (tag.state_labels != null)
                            {
                                template = "PresetGroupState";
                            }
                            else if (type is Integer || type is BitString || type is REAL || type is LREAL)
                            {
                                template = "PresetGroupNumber";
                            }
                            else if (type is BOOL)
                            {
                                template = "PresetGroupBool";
                            }
                            else
                            {
                                template = "PresetGroupNoValue";
                            }


                            editor.AddEnableSelection(template, groupName, index, tag.labels, tag.unit, tag.precision);
                            index++;
                        }


                        TIAutils.ImportScreenPopupXML(popup_doc, folder);
                    }
                    else
                    {
                        LogMessage(MessageLog.Severity.Info, "Couldn't find popup " + popupName+", skipping.");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to update popup screen:\n" + ex.Message);
                    return;
                }
            
            }
        }
    }
}