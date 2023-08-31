using Siemens.Engineering;
using Siemens.Engineering.Hmi.Tag;
using System;
using System.Collections.Generic;
using System.Xml;
using TIAEktool.Plc.Types;
using TIAEKtool.Plc;

namespace TIAEKtool
{
    public class CreatePresetHmiTagsTask : SequentialTask
    {
        readonly TiaPortal portal;
        readonly TagFolder folder;
        readonly string tableName;
        readonly string groupName;
        readonly string dbName;
        readonly string hmiDbName;
        readonly int nPresets;
        readonly IList<PresetTag> tags;
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
            Description = TIAutils.FindParentDeviceName(folder) + ":Create HMI tag table " + tableName;
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
                            editor.AddIndexedTag("PresetEnable_" + groupName + "_", index, tag.readTagPath.PrependPath(enable_selected).ToString());
                            editor.AddIndexedTag("PresetValue_" + groupName + "_", index, tag.readTagPath.PrependPath(preset_selected).ToString(), tag.readTagPath.Type, tag.min, tag.max);

                            index++;
                        }

                        ARRAY name_array = new ARRAY
                        {
                            MemberType = new STRING()
                        };
                        PathComponent preset_names = new MemberComponent("Names", name_array, new MemberComponent(dbName, new STRUCT()));
                        for (int p = 1; p <= nPresets; p++)
                        {
                            PathComponent name = new IndexComponent(new int[1] { p }, preset_names);
                            editor.AddIndexedTag("PresetName_" + groupName + "_", p, name.ToString());
                        }

                        ARRAY color_array = new ARRAY
                        {
                            MemberType = INT.Type
                        };
                        PathComponent preset_colors = new MemberComponent("Colors", color_array, new MemberComponent(dbName, new STRUCT()));
                        for (int p = 1; p <= nPresets; p++)
                        {
                            PathComponent color = new IndexComponent(new int[1] { p }, preset_colors);
                            editor.AddIndexedTag("PresetColor_" + groupName + "_", p, color.ToString());
                        }
                        TIAutils.ImportHMITagTableXML(table_doc, folder);
                    }
                    else
                    {
                        LogMessage(MessageLog.Severity.Warning, "No tag table named '" + tableName + "' was found, skipping.");
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