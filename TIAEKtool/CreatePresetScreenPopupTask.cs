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
       
        IList<PresetTag> tags;
        public CreatePresetScreenPopupTask(TiaPortal portal, IList<PresetTag> tags, ScreenPopupFolder folder, string popup_name, string group_name)
        {
            this.portal = portal;
            this.tags = tags;
            this.folder = folder;
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
                        PresetPopup editor = new PresetPopup(popup_doc);
                        int index = 1;

                        foreach (var tag in tags)
                        {
                            editor.AddEnableSelection(groupName, index, tag.labels, tag.unit, tag.precision);
                            index++;
                        }


                        TIAutils.ImportScreenPopupXML(popup_doc, folder);
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