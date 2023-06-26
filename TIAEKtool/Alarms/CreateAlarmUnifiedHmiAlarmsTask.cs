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
    public class CreateAlarmUnifiedHmiAlarmsTask : SequentialTask
    {
        TiaPortal portal;
        HmiSoftware hmi_software;
        IList<AlarmTag> tags;
        Dictionary<PathComponent, String> plc_to_hmi; // Maps PLC tags to HMI tags
        Language lang;
        public CreateAlarmUnifiedHmiAlarmsTask(TiaPortal portal, HmiSoftware hmi, IList<AlarmTag> tags, Dictionary<PathComponent, String> plc_to_hmi, Language lang)
        {
            this.portal = portal;
            this.tags = tags;
            this.hmi_software = hmi;
            this.plc_to_hmi = plc_to_hmi;
            this.lang = lang;
            Description = TIAutils.FindParentDeviceName(hmi) + ": Create HMI alarms";
        }
        
        const String ERROR_TAG_TABLE = "ErrorTags";
        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {
                    foreach (var alarm in tags)
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
                            hmi_alarm.RaisedStateTag = alarm.plcTag.ToHmiTagName();
                            Console.WriteLine("Before: "+hmi_alarm.EventText.Items.Find(lang).Text);
                           
                            List<ParseTextUnified.FieldInfo> fields = null;
                            string parsed_text = ParseTextUnified.ParseTextToText(alarm.eventText[lang.Culture.Name], ref fields);
                            //string parsed_text = hmi_alarm.EventText.Items.Find(lang).Text;
                            hmi_alarm.EventText.Items.Find(lang).Text = parsed_text;
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