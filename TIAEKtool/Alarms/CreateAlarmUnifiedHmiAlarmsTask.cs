using Siemens.Engineering;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.HmiAlarm.HmiAlarmCommon;
using Siemens.Engineering.HmiUnified.HmiConnections;
using System;
using System.Collections.Generic;
using TIAEKtool.Alarms;
using TIAEKtool.Plc;

namespace TIAEKtool
{
    public class CreateAlarmUnifiedHmiAlarmsTask : SequentialTask
    {
        readonly TiaPortal portal;
        readonly HmiSoftware hmi_software;
        readonly IList<AlarmTag> tags;
        readonly Dictionary<PathComponent, String> plc_to_hmi; // Maps PLC tags to HMI tags
       
        readonly Language lang;
        public CreateAlarmUnifiedHmiAlarmsTask(TiaPortal portal, HmiSoftware hmi, IList<AlarmTag> tags, Dictionary<PathComponent, String> plc_to_hmi, Language lang)
        {
            this.portal = portal;
            this.tags = tags;
            this.hmi_software = hmi;
            this.plc_to_hmi = plc_to_hmi;
            this.lang = lang;
            Description = TIAutils.FindParentDeviceName(hmi) + ": Create HMI alarms";
        }
        
       
        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {
                    HmiConnection plc_connection = HmiUtils.FindPlcConnection(hmi_software, log, out string connection_id, out _);
                    if (plc_connection != null)
                    {
                        foreach (var alarm in tags)
                        {
                            if (alarm.targets.Contains(connection_id))
                            {
                                if (plc_to_hmi.TryGetValue(alarm.plcTag, out string hmi_name))
                                {
                                    alarm.hmiTag = hmi_name;
                                    var hmi_alarm = hmi_software.DiscreteAlarms.Find(hmi_name);
                                    if (hmi_alarm == null)
                                    {
                                        hmi_alarm = hmi_software.DiscreteAlarms.Create(hmi_name);
                                    }
                                    hmi_alarm.Id = (uint)alarm.id;
                                    hmi_alarm.Priority = (byte)alarm.priority;
                                    hmi_alarm.AlarmClass = alarm.alarmClass;
                                    string hmi_tag_name = alarm.plcTag.ToHmiTagName();
                                    hmi_alarm.RaisedStateTag = hmi_tag_name;
                                    hmi_alarm.AcknowledgmentStateTag = hmi_tag_name + "_ack";
                                    hmi_alarm.AcknowledgmentStateTagBitNumber = 0;
                                    hmi_alarm.AcknowledgmentControlTag = hmi_tag_name + "_ctrl";
                                    hmi_alarm.AcknowledgmentControlTagBitNumber = 1;
                                    hmi_alarm.TriggerMode = (alarm.edge == AlarmTag.Edge.Rising) ?
                                        HmiDiscreteAlarmTriggerMode.OnRisingEdge :
                                        HmiDiscreteAlarmTriggerMode.OnFallingEdge;

                                    Console.WriteLine("Before: " + hmi_alarm.EventText.Items.Find(lang).Text);

                                    List<ParseTextUnified.FieldInfo> fields = null;
                                    string parsed_text = ParseTextUnified.ParseTextToText(alarm.eventText[lang.Culture.Name], ref fields);
                                    //string parsed_text = hmi_alarm.EventText.Items.Find(lang).Text;
                                    hmi_alarm.EventText.Items.Find(lang).Text = parsed_text;
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to create discete alarms:\n" + ex.Message);
                    return;
                }

            }

        }
    }
}