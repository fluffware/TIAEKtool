using Siemens.Engineering;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.HmiTags;
using System;
using System.Collections.Generic;
using TIAEktool.Plc.Types;
using TIAEKtool.Plc;

namespace TIAEKtool
{
    public class CreatePresetUnifiedHmiTagsTask : SequentialTask
    {
        TiaPortal portal;
        HmiTagTableGroupComposition tag_table_groups;
        string tableName;
        string groupName;
        string dbName;
        string hmiDbName;
        string culture;
        IList<PresetTag> tags;
        public CreatePresetUnifiedHmiTagsTask(TiaPortal portal, IList<PresetTag> tags, HmiTagTableGroupComposition tag_table_groups, string table_name, string group_name, string db_name, string hmi_db_name, string culture)
        {
            this.portal = portal;
            this.tags = tags;
            this.tag_table_groups = tag_table_groups;
            this.tableName = table_name;
            groupName = group_name;
            dbName = db_name;
            hmiDbName = hmi_db_name;
            this.culture = culture;
            Description = TIAutils.FindParentDeviceName(tag_table_groups) + ": Create HMI tag table " + tableName;
        }
        private HmiTag modify_tag(HmiTagComposition tags, string name, string plc_tag, string plc_connection)
        {
            var tag = tags.Find(name);
            if (tag != null) tag.Delete();
            tag = tags.Create(name);
            tag.Connection = plc_connection;
            tag.PlcTag = plc_tag;
            return tag;
        }
        private HmiTag modify_internal_tag(HmiTagComposition tags, string name, string type, object value)
        {
            var tag = tags.Find(name);
            if (tag != null) tag.Delete();
            try
            {
                tag = tags.Create(name);
            } catch(Exception ex)
            {
                throw new Exception("Failed to create HMI tag '" + name + ": " + ex.Message + "'");
            }
            tag.DataType= type;
            tag.InitialValue = value;
            tag.Connection = "<Internal tag>";
            return tag;
        }

        const string TagGroupName = "Preset";
        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {
                    // Create Preset group if it doesn't exist
                    HmiTagTableGroup group = tag_table_groups.Find(TagGroupName);
                    if (group == null)
                    {
                        group = tag_table_groups.Create(TagGroupName);
                    }
                    HmiTagTableComposition tag_tables = group.TagTables;
                    // Create tag table if it doesn't exist
                    HmiTagTable table = tag_tables.Find(tableName);
                    if (table == null)
                    {
                        table = tag_tables.Create(tableName);
                    }

                    // Find PLC connection
                    var hmi_software = (HmiSoftware)tag_table_groups.Parent;
                    string connection_name = null;
                    foreach (var connection in hmi_software.Connections) {
                        if (connection.CommunicationDriver.StartsWith("SIMATIC S7"))
                        {
                            if (connection_name != null)
                            {
                                LogMessage(MessageLog.Severity.Error, "More than one PLC connection found, don't know which one to use.");
                                return;
                            }
                            connection_name = connection.Name;
                        }
                    }
                    if (connection_name == null)
                    {
                        LogMessage(MessageLog.Severity.Error, "No PLC connection found.");
                    }


                    PathComponent enable_selected = new MemberComponent("EnableSelected", new STRUCT(), new MemberComponent(hmiDbName, new STRUCT()));
                    PathComponent preset_selected = new MemberComponent("PresetSelected", new STRUCT(), new MemberComponent(hmiDbName, new STRUCT()));

                    int index = 1;
                    foreach (var tag in tags)
                    {
                        modify_tag(table.Tags, "PresetEnable_" + groupName + "_" + index, tag.readTagPath.PrependPath(enable_selected).ToString(), connection_name);
                        var value = modify_tag(table.Tags, "PresetValue_" + groupName + "_" + index, tag.readTagPath.PrependPath(preset_selected).ToString(), connection_name);
                        if (tag.max != null) {
                            value.InitialMaxValue.ValueType = HmiLimitValueType.Constant;
                            value.InitialMaxValue.Value = tag.max.ToString();
                        }
                        if (tag.min != null) {
                            value.InitialMinValue.ValueType = HmiLimitValueType.Constant;
                            value.InitialMinValue.Value = tag.min.ToString();
                        }
                        index++;

                       
                    }
                    modify_internal_tag(table.Tags, "PresetValueCount_" + groupName, "int", tags.Count);
                    // Remove unused tags
                    while (true)
                    {
                        var tag = table.Tags.Find("PresetEnable_" + groupName + "_" + index);
                        if (tag == null) break;
                        tag.Delete();
                    }
                    while (true)
                    {
                        var tag = table.Tags.Find("PresetValue_" + groupName + "_" + index);
                        if (tag == null) break;
                        tag.Delete();
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