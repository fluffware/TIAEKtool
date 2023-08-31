using Siemens.Engineering;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.HmiConnections;
using Siemens.Engineering.HmiUnified.HmiTags;
using System;
using System.Collections.Generic;
using TIAEKtool.Alarms;
using TIAEKtool.Plc;

namespace TIAEKtool
{
    public class CreateAlarmUnifiedHmiTagsTask : SequentialTask
    {
        readonly TiaPortal portal;
        readonly HmiSoftware hmi_software;
        readonly IList<AlarmTag> tags;
        readonly Dictionary<PathComponent, String> plc_to_hmi; // Maps PLC tags to HMI tags
        public CreateAlarmUnifiedHmiTagsTask(TiaPortal portal, HmiSoftware hmi, IList<AlarmTag> tags, Dictionary<PathComponent, String> plc_to_hmi)
        {
            this.portal = portal;
            this.tags = tags;
            this.hmi_software = hmi;
            this.plc_to_hmi = plc_to_hmi;
            Description = TIAutils.FindParentDeviceName(hmi) + ": Create missing HMI alarm tags ";
        }
        private HmiTag ModifyTag(HmiTagComposition tags, string name, string plc_tag, string plc_connection)
        {
            var tag = tags.Find(name);
            if (tag != null) tag.Delete();
            tag = tags.Create(name);
            tag.Connection = plc_connection;
            tag.PlcTag = plc_tag;
            return tag;
        }
        private HmiTag ModifyInternalTag(HmiTagComposition tags, string name, string type, object value)
        {
            var tag = tags.Find(name);
            if (tag != null) tag.Delete();
            tag = tags.Create(name);
           
            tag.DataType= type;
            tag.InitialValue = value;
            tag.Connection = "<Internal tag>";
            return tag;
        }
        const String ERROR_TAG_TABLE = "ErrorTags";
        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {



                    
                    HmiConnection plc_connection = HmiUtils.FindPlcConnection(hmi_software, log, out string connection_id, out _);
                    if (plc_connection != null)
                    {
                        if (hmi_software.TagTables.Find(ERROR_TAG_TABLE) == null)
                        {
                            hmi_software.TagTables.Create(ERROR_TAG_TABLE);
                        }

                        foreach (var alarm in tags)
                        {
                            if (alarm.targets.Contains(connection_id))
                            {
                                Console.WriteLine("Find " + alarm.plcTag.ToString() + ": " + alarm.plcTag.GetHashCode());
                                if (plc_to_hmi.TryGetValue(alarm.plcTag, out string hmi_name))
                                {
                                    alarm.hmiTag = hmi_name;
                                }
                                else
                                {
                                    string hmi_tag_name = alarm.plcTag.ToHmiTagName();
                                    var tag = hmi_software.Tags.Create(hmi_tag_name, ERROR_TAG_TABLE);
                                    tag.Connection = plc_connection.Name;
                                    tag.PlcTag = alarm.plcTag.ToString();
                                    plc_to_hmi.Add(alarm.plcTag, hmi_tag_name);
                                    alarm.hmiTag = hmi_tag_name;

                                  
                                    var ack_tag = hmi_software.Tags.Create(hmi_tag_name+"_ack", ERROR_TAG_TABLE);
                                    ack_tag.Connection = plc_connection.Name;
                                    ack_tag.PlcTag = alarm.plcTag.ToString()+"_ack";

                                    var ctrl_tag = hmi_software.Tags.Create(hmi_tag_name + "_ctrl", ERROR_TAG_TABLE);
                                    ctrl_tag.Connection = plc_connection.Name;
                                    ctrl_tag.PlcTag = alarm.plcTag.ToString() + "_ack";

                                    LogMessage(MessageLog.Severity.Info, "Created alarm HMI tag " + hmi_tag_name);
                                }
                            }
                        }


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