using PLC.Types;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Tag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.Hmi.TextGraphicList;

namespace TIAEKtool
{
    public partial class PresetGenerate : Form
    {
        static class FindHMI
        {
            // Device items
            private static void handleDeviceItem(IList<HmiTarget> hmi, DeviceItem item)
            {

                SoftwareContainer sw_cont = item.GetService<SoftwareContainer>();
                if (sw_cont != null)
                {
                    HmiTarget hmi_target = sw_cont.Software as HmiTarget;
                    if (hmi_target != null)
                    {
                        hmi.Add(hmi_target);

                    }
                }
                IterDeviceItem(hmi, item.DeviceItems);
            }

            private static void IterDeviceItem(IList<HmiTarget> hmi, DeviceItemComposition items)
            {
                foreach (DeviceItem item in items)
                {
                    handleDeviceItem(hmi, item);
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
        protected PresetTagList presetList;
        TagParser parser;
        PlcBlockGroup resultGroup;
        PlcTypeGroup typeGroup;
        PlcSoftware plcSoftware;

        IList<HmiTarget> hmiTargets;
        TaskDialog task_dialog;
        TiaPortal tiaPortal;
        MessageLog log = new MessageLog();
        public PresetGenerate(TiaPortal portal, IEngineeringCompositionOrObject top, string culture)
        {
            InitializeComponent();
            tiaPortal = portal;
            FormClosing += FormClosingEventHandler;
            presetListView.AutoGenerateColumns = false;
            presetList = new PresetTagList();
            presetList.Culture = culture;
            presetListView.DataSource = presetList;

            writeButton.Enabled = false;
            parser = new TagParser(portal);
            parser.HandleTag += HandleTag;
            parser.ParseDone += ParseDone;
            parser.ParseAsync(top, log);

            IEngineeringCompositionOrObject node = top;
            while (node.Parent is PlcBlockGroup) node = node.Parent;
            PlcBlockGroup top_group = (PlcBlockGroup)node;
            resultGroup = top_group.Groups.Find("Preset");
            if (resultGroup == null)
            {
                resultGroup = top_group.Groups.Create("Preset");
            }
            while (node != null && !(node is PlcSoftware)) node = node.Parent;
            if (node == null) throw new Exception("No PlcSoftware node found");
            plcSoftware = (PlcSoftware)node;
            typeGroup = plcSoftware.TypeGroup.Groups.Find("Preset");
            if (typeGroup == null)
            {
                typeGroup = plcSoftware.TypeGroup.Groups.Create("Preset");
            }
            hmiTargets = new List<HmiTarget>();
            node = top;
            while (node != null && !(node is DeviceUserGroup)) node = node.Parent;

            if (node != null)
            {
                DeviceUserGroup dev_group = (DeviceUserGroup)node;

                FindHMI.HandleDeviceFolder(hmiTargets, dev_group);
               
            }
        }


        protected void FormClosingEventHandler(object sender, FormClosingEventArgs e)
        {
            parser.CancelParse();
        }

        public void HandleTag(object source, TagParser.HandleTagEventArgs ev)
        {
            PresetTag preset = new PresetTag() { tagPath = ev.Path };
           
            foreach (string c in ev.Comment.Cultures)
            {
                PresetCommentParser.Parse(ev.Comment[c], c, preset);
             
            }
            if (preset.labels != null)
            {
                presetList.AddTag(preset);
            }
        }

        public void ParseDone(object source, TagParser.ParseDoneEventArgs ev)
        {
            writeButton.Enabled = resultGroup != null;
            if (log.HighestSeverity >= MessageLog.Severity.Warning)
            {
                LogDialog dialog = new LogDialog(log);
                dialog.ShowDialog();
            }
        }

        private void PresetGenerate_Load(object sender, EventArgs e)
        {

        }


        private void writeButton_Click(object sender, EventArgs e)
        {
            if (task_dialog == null)
            {
                task_dialog = new TaskDialog();
            }
            task_dialog.Clear();
            // Sort the groups into separate lists of tags
            Dictionary<string, List<PresetTag>> tag_groups = new Dictionary<string, List<PresetTag>>();
            foreach (PresetTagList.Row r in presetList)
            {
                List<PresetTag> tags;
                if (!tag_groups.TryGetValue(r.Tag.presetGroup, out tags))
                {
                    tags = new List<PresetTag>();

                    tag_groups[r.Tag.presetGroup] = tags;
                }
                tags.Add(r.Tag);
            }

            ConstantLookup constants = new ConstantLookup();
            constants.Populate(tiaPortal, plcSoftware);
            foreach (HmiTarget hmi in hmiTargets)
            {

              

                // Create HMI tags
                TagFolder preset_tag_folder = hmi.TagFolder.Folders.Find("Preset");
                if (preset_tag_folder != null)
                {
                    task_dialog.AddTask(new CreateHmiPresetConstantTagsTask(tiaPortal,preset_tag_folder,constants));
                }
            }

                // Create databases for all groups
                foreach (string group_name in tag_groups.Keys)
            {

                
                string db_name = "sDB_Preset_" + group_name;
                var tags = tag_groups[group_name];

                string value_type_name = "PresetValueType_" + group_name;
                string enable_type_name = "PresetEnableType_" + group_name;

                task_dialog.AddTask(new CreatePresetTypesTask(tiaPortal, tags,typeGroup, value_type_name, enable_type_name));
                string recall_block_name = "PresetRecall_" + group_name;
                task_dialog.AddTask(new CreatePresetRecallBlockTask(tiaPortal, tags, resultGroup, recall_block_name, value_type_name, enable_type_name));
                string store_block_name = "PresetStore_" + group_name;
                task_dialog.AddTask(new CreatePresetStoreBlockTask(tiaPortal, tags, resultGroup, store_block_name, value_type_name, enable_type_name));

                foreach (HmiTarget hmi in hmiTargets)
                {

                    // Create text lists 
                    TextListComposition hmi_text_lists = hmi.TextLists;
                    int count = 1;
                    foreach (PresetTag tag in tags)
                    {
                        if (tag.state_labels != null)
                        {
                            string list_name = "PresetTextList_" + group_name + "_" + count;
                            task_dialog.AddTask(new CreateHmiTextListTask(tiaPortal, list_name, hmi_text_lists, tag.state_labels));
                        }
                        count++;
                    }

                    // Create HMI tags
                    TagFolder preset_tag_folder = hmi.TagFolder.Folders.Find("Preset");
                    if (preset_tag_folder != null)
                    {
                        string table_name = "Preset_" + group_name;
                       
                            task_dialog.AddTask(new CreatePresetHmiTagsTask(tiaPortal, tags, preset_tag_folder, table_name, group_name, db_name));
                      
                    }
                    // Load template screen

                    ScreenTemplate obj_templ = hmi.ScreenTemplateFolder.ScreenTemplates.Find("ObjectTemplate");
                    if (obj_templ != null)
                    {
                        XmlDocument templates = TIAutils.ExportScreenTemplateXML(obj_templ);

                        // Create popups
                        string popup_name = "PresetPopup_" + group_name;

                        ScreenPopupFolder popup_folder = hmi.ScreenPopupFolder;


                        task_dialog.AddTask(new CreatePresetScreenPopupTask(tiaPortal, tags, popup_folder, templates, popup_name, group_name));
                    }
                    else
                    {
                        MessageBox.Show("No template screen named ObjectTemplate found for HMI " + hmi.Name+". Some screens will not be updated.");
                    }
                }

            }
           

            task_dialog.Show();
        }
    }

 
}
