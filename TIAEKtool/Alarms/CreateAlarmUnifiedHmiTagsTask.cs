using PLC.Types;
using Siemens.Engineering;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.HmiConnections;
using Siemens.Engineering.HmiUnified.HmiTags;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.Xml;

namespace TIAEKtool
{
    public class CreateAlarmUnifiedHmiTagsTask : SequentialTask
    {
        TiaPortal portal;
        HmiSoftware hmi_software;
        IList<AlarmTag> tags;
        Dictionary<PathComponent, String> plc_to_hmi; // Maps PLC tags to HMI tags
        public CreateAlarmUnifiedHmiTagsTask(TiaPortal portal, HmiSoftware hmi, IList<AlarmTag> tags, Dictionary<PathComponent, String> plc_to_hmi)
        {
            this.portal = portal;
            this.tags = tags;
            this.hmi_software = hmi;
            this.plc_to_hmi = plc_to_hmi;
            Description = TIAutils.FindParentDeviceName(hmi) + ": Create missing HMI alarm tags ";
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




                    // Find PLC connection

                    HmiConnection plc_connection = null;
                   
                    foreach (var connection in hmi_software.Connections) {
                        if (connection.CommunicationDriver.StartsWith("SIMATIC S7"))
                        {
                            if (plc_connection != null)
                            {
                                LogMessage(MessageLog.Severity.Error, "More than one PLC connection found, don't know which one to use.");
                                return;
                            }
                            plc_connection = connection;
                            
                            
                           
                        }
                    }
                    
                    if (plc_connection == null)
                    {
                        LogMessage(MessageLog.Severity.Error, "No PLC connection found.");
                        return;
                    }
                    string connection_id = null;
                    CommentParser.Parse(plc_connection.Comment, (type, data) => { if (type == "alarm_hmi") { connection_id = data.Trim(); } });
                    if (connection_id == null)
                    {
                        LogMessage(MessageLog.Severity.Error, "The PLC connection "+plc_connection.Name+ " has no HMI ID (alarm_hmi)");
                        return;
                    }

                    LogMessage(MessageLog.Severity.Info, "Using connection " + connection_id);
                  
                    
                    if (hmi_software.TagTables.Find(ERROR_TAG_TABLE) == null)
                    {
                        hmi_software.TagTables.Create(ERROR_TAG_TABLE);
                    }

                    foreach (var alarm in tags)
                    {
                        if (alarm.sinks.Contains(connection_id))
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
                                LogMessage(MessageLog.Severity.Info, "Created alarm HMI tag " + hmi_tag_name);
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