using PLC.Types;
using Siemens.Engineering;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.Xml;

namespace TIAEKtool
{
    public class CreatePresetHmiTagsTask : SequentialTask
    {
        TiaPortal portal;
        TagFolder folder;
        string tableName;
        string groupName;
        string dbName;
        IList<PresetTag> tags;
        public CreatePresetHmiTagsTask(TiaPortal portal, IList<PresetTag> tags, TagFolder folder, string table_name, string group_name, string db_name)
        {
            this.portal = portal;
            this.tags = tags;
            this.folder = folder;
            this.tableName = table_name;
            groupName = group_name;
            dbName = db_name;
            Description = "Create HMI tag table " + tableName;
        }

        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {


                    TagTable table = folder.TagTables.Find(tableName);

                    if (table != null)
                    {

                        XmlDocument table_doc = TIAutils.ExportHMITagTableXML(table);
                        HMITagTable editor = new HMITagTable(table_doc);
                        PathComponent enable_selected = new MemberComponent("EnableSelected", new STRUCT(), new MemberComponent(dbName, new STRUCT()));
                        PathComponent preset_selected = new MemberComponent("PresetSelected", new STRUCT(), new MemberComponent(dbName, new STRUCT()));
                        int index = 1;
                        foreach (var tag in tags)
                        {
                            editor.AddIndexedTag("PresetEnable_" + groupName + "_", index, tag.tagPath.PrependPath(enable_selected).ToString());
                            editor.AddIndexedTag("PresetValue_" + groupName + "_", index, tag.tagPath.PrependPath(preset_selected).ToString(), tag.tagPath.Type);
                             
                            index++;
                        }


                        TIAutils.ImportHMITagTableXML(table_doc, folder);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(Severity.Error, "Failed to update tag table:\n" + ex.Message);
                    return;
                }

            }

        }
    }
}