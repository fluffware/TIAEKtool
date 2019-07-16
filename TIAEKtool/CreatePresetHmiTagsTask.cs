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
        string hmiDbName;
        int nPresets;
        IList<PresetTag> tags;
        public CreatePresetHmiTagsTask(TiaPortal portal, IList<PresetTag> tags, TagFolder folder, string table_name, string group_name, string db_name, string hmi_db_name, int nPresets)
        {
            this.portal = portal;
            this.tags = tags;
            this.folder = folder;
            this.tableName = table_name;
            groupName = group_name;
            dbName = db_name;
            hmiDbName = hmi_db_name;
            this.nPresets = nPresets;
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
                        PathComponent enable_selected = new MemberComponent("EnableSelected", new STRUCT(), new MemberComponent(hmiDbName, new STRUCT()));
                        PathComponent preset_selected = new MemberComponent("PresetSelected", new STRUCT(), new MemberComponent(hmiDbName, new STRUCT()));

                        int index = 1;
                        foreach (var tag in tags)
                        {
                            editor.AddIndexedTag("PresetEnable_" + groupName + "_", index, tag.tagPath.PrependPath(enable_selected).ToString());
                            editor.AddIndexedTag("PresetValue_" + groupName + "_", index, tag.tagPath.PrependPath(preset_selected).ToString(), tag.tagPath.Type);

                            index++;
                        }

                        ARRAY name_array = new ARRAY();
                        name_array.MemberType = new STRING();
                        PathComponent preset_names = new MemberComponent("Names", name_array, new MemberComponent(dbName, new STRUCT()));
                        ARRAY color_array = new ARRAY();
                        color_array.MemberType = INT.Type;
                        PathComponent preset_colors = new MemberComponent("Colors", color_array, new MemberComponent(dbName, new STRUCT()));
                        for (int p = 1; p <= nPresets; p++)
                        {
                            PathComponent name = new IndexComponent(new int[1] { p }, new STRING(), preset_names);
                            editor.AddIndexedTag("PresetName_" + groupName + "_", p, name.ToString());
                            PathComponent color = new IndexComponent(new int[1] { p }, INT.Type, preset_colors);
                            editor.AddIndexedTag("PresetColor_" + groupName + "_", p, color.ToString());
                        }
                        TIAutils.ImportHMITagTableXML(table_doc, folder);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to update tag table:\n" + ex.Message);
                    return;
                }

            }

        }
    }
}