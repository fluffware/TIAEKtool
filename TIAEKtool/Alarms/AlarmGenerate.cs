using Siemens.Engineering;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.HmiConnections;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using TIAEKtool.Alarms;
using TIAEKtool.Plc;

namespace TIAEKtool
{
    public partial class AlarmGenerate : Form
    {
        static class FindHMI
        {
            // Device items
            private static void HandleDeviceItem(IList<HmiTarget> hmi, DeviceItem item)
            {

                SoftwareContainer sw_cont = item.GetService<SoftwareContainer>();
                if (sw_cont != null)
                {
                    if (sw_cont.Software is HmiTarget hmi_target) {
                        hmi.Add(hmi_target);
                    }
                }
                IterDeviceItem(hmi, item.DeviceItems);
            }

            private static void IterDeviceItem(IList<HmiTarget> hmi, DeviceItemComposition items)
            {
                foreach (DeviceItem item in items)
                {
                    HandleDeviceItem(hmi, item);
                }
            }

            private static void HandleDevice(IList<HmiTarget> hmi, Device device)
            {
                IterDeviceItem(hmi, device.DeviceItems);
            }

            private static void IterDevice(IList<HmiTarget> hmi, DeviceComposition devices)
            {
                foreach (Device d in devices)
                {
                    HandleDevice(hmi, d);
                }

            }
            public static void HandleDeviceFolder(IList<HmiTarget> hmi, DeviceUserGroup folder)
            {
                IterDeviceFolder(hmi, folder.Groups);
                IterDevice(hmi, folder.Devices);

            }
            private static void IterDeviceFolder(IList<HmiTarget> hmi, DeviceUserGroupComposition folders)
            {
                foreach (DeviceUserGroup f in folders)
                {
                    HandleDeviceFolder(hmi, f);
                }
            }
        }
        protected AlarmTagList alarmList;
        protected AlarmTargetList sinkList;
        readonly TagParser parser;
        readonly PlcSoftware plcSoftware;

        readonly IList<HmiSoftware> hmiSoftware;
        TaskDialog task_dialog;
        readonly TiaPortal tiaPortal;
        readonly MessageLog log = new MessageLog();
        readonly ConstantLookup constants;
        public AlarmGenerate(TiaPortal portal, IEngineeringCompositionOrObject top, List<HmiSoftware> hmiSoftware, ConstantLookup constants, string culture)
        {
            InitializeComponent();
            tiaPortal = portal;
            FormClosing += FormClosingEventHandler;
            alarmListView.AutoGenerateColumns = false;
            alarmList = new AlarmTagList
            {
                Culture = culture
            };  
            alarmListView.DataSource = alarmList;

            sinkList = new AlarmTargetList();
            sinkListView.DataSource = sinkList;

            writeButton.Enabled = false;
            exportButton.Enabled = false;
            parser = new TagParser(portal);
            parser.HandleTag += HandleTag;
            parser.ParseDone += ParseDone;
            parser.ParseAsync(top, log);

            IEngineeringCompositionOrObject node = top;   
            while (node != null && !(node is PlcSoftware)) node = node.Parent;
            if (node == null) throw new Exception("No PlcSoftware node found");
            plcSoftware = (PlcSoftware)node;

            this.hmiSoftware = hmiSoftware;
            this.constants = constants;

            Project proj = tiaPortal.Projects[0];
            LanguageAssociation langs = proj.LanguageSettings.ActiveLanguages;
           
                cultureComboBox.Items.Clear();
                cultureComboBox.Items.AddRange(langs.Select(l => l.Culture.Name).ToArray());
            cultureComboBox.SelectedItem = culture;
        }


        protected void FormClosingEventHandler(object sender, FormClosingEventArgs e)
        {
            parser.CancelParse();
        }

        public void HandleTag(object source, TagParser.HandleTagEventArgs ev)
        {
            
           
            foreach (string c in ev.Comment.Cultures)
            {
                AlarmCommentParser.Parse(ev.Comment[c], c, out AlarmTag alarm_tag, out AlarmTarget alarm_target);
                if (alarm_tag != null && alarm_tag.alarmClass != null)
                {
                    alarm_tag.plcTag = ev.Path;
                    alarmList.AddTag(alarm_tag);
                }
                if (alarm_target != null)
                {
                    if (alarm_target is AlarmTargetTag sink_tag)
                    {
                        sink_tag.plcTag = ev.Path;
                    }
                    sinkList.AddSink(alarm_target);
                }
            }
        }

        public void ParseDone(object source, TagParser.ParseDoneEventArgs ev)
        {

            foreach (HmiSoftware hmi in hmiSoftware)
            {
                HmiConnection plc_connection = Alarms.HmiUtils.FindPlcConnection(hmi, log, out string target_id, out string target_label);
                if (plc_connection != null)
                {
                    AlarmTarget target= new AlarmTargetHmi(target_id, target_label)
                    {
                        hmiName = hmi.Name
                    };
                    sinkList.AddSink(target);
                }
            }
            writeButton.Enabled = true;
            exportButton.Enabled = true;
            if (log.HighestSeverity >= MessageLog.Severity.Warning)
            {
                LogDialog dialog = new LogDialog(log);
                dialog.ShowDialog();
            }
        }

      
        private void WriteButton_Click(object sender, EventArgs e)
        {
            if (task_dialog == null)
            {
                task_dialog = new TaskDialog();
            }
            task_dialog.Clear();

            Project proj = tiaPortal.Projects[0];
            LanguageAssociation langs = proj.LanguageSettings.ActiveLanguages;

            string[] cultures = langs.Select(l => l.Culture.Name).ToArray();
            string default_culture = proj.LanguageSettings.ReferenceLanguage.Culture.Name;

            Dictionary<string, List<PresetTag>> tag_groups = new Dictionary<string, List<PresetTag>>();
            List<AlarmTag> alarm_tags = new List<AlarmTag>();
            foreach (AlarmTagList.Row r in alarmList)
            {
                    alarm_tags.Add(r.AlarmTag);
                    

            }
            alarm_tags.Sort();

            List<AlarmTarget> alarm_targets = new List<AlarmTarget>();
            foreach (AlarmTargetList.Row r in sinkList)
            {
                alarm_targets.Add(r.AlarmSink);
            }


            foreach (HmiSoftware hmi in hmiSoftware)
            {
                Dictionary<PathComponent, String> plc_to_hmi = new Dictionary<PathComponent, string>();

                // Build a map from a PLC tag to the coorresponding HMI tag
                foreach (var hmi_tag in hmi.Tags)
                {
                    if (hmi_tag.PlcTag != null && hmi_tag.PlcTag.Length > 0)
                    {
                        PathComponent plc_tag = PathComponentUtils.ParsePath(hmi_tag.PlcTag);
                        if (plc_tag != null && !plc_to_hmi.ContainsKey(plc_tag))
                        {
                            plc_to_hmi.Add(plc_tag, hmi_tag.Name);
                        }
                    }

                }

                var lang = proj.LanguageSettings.Languages.Find(new CultureInfo(alarmList.Culture));

                task_dialog.AddTask(new CreateTagAlarmsTask(tiaPortal, plcSoftware, alarm_targets, alarm_tags));
                task_dialog.AddTask(new UpdateAckTagsTask(tiaPortal, plcSoftware.BlockGroup, alarm_tags,alarmList.Culture,constants));
                // Create HMI tags
                task_dialog.AddTask(new CreateAlarmUnifiedHmiTagsTask(tiaPortal, hmi, alarm_tags, plc_to_hmi));

                // Create HMI alarms
                task_dialog.AddTask(new CreateAlarmUnifiedHmiAlarmsTask(tiaPortal, hmi, alarm_tags, plc_to_hmi, lang));

                
            }
            
           

            task_dialog.Show();
        }

       

        private void ExportButton_Click(object sender, EventArgs e)
        {

            if (saveAlarmList.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<AlarmTag> alarm_tags = new List<AlarmTag>();
                foreach (AlarmTagList.Row r in alarmList)
                {
                    alarm_tags.Add(r.AlarmTag);


                }
                List<AlarmTarget> alarm_targets = new List<AlarmTarget>();
                foreach (AlarmTargetList.Row r in sinkList)
                {
                    alarm_targets.Add(r.AlarmSink);


                }
                AlarmDocument.Save(saveAlarmList.FileName, alarm_tags, alarm_targets, cultureComboBox.SelectedItem.ToString());

            }
        }

        private void CultureComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            alarmList.Culture = cultureComboBox.SelectedItem.ToString();
        }

    }

 
}
