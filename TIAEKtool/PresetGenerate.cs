using PLC.Types;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.Hmi.Screen;
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
        IList<ScreenPopupFolder> popupFolders;
        public PresetGenerate(TiaPortal portal, IEngineeringCompositionOrObject top)
        {
            InitializeComponent();
            FormClosing += FormClosingEventHandler;
            presetListView.AutoGenerateColumns = false;
            presetList = new PresetTagList();
            presetList.Culture = "sv-SE";
            presetListView.DataSource = presetList;

            writeButton.Enabled = false;
            parser = new TagParser(portal);
            parser.HandleTag += HandleTag;
            parser.ParseDone += ParseDone;
            parser.ParseAsync(top);

            IEngineeringCompositionOrObject node = top;
            while (node.Parent is PlcBlockGroup) node = node.Parent;
            PlcBlockGroup top_group = (PlcBlockGroup)node;
            resultGroup = top_group.Groups.Find("Preset");
            if (resultGroup == null)
            {
                resultGroup = top_group.Groups.Create("Preset");
            }

            node = top;
            while (node != null && !(node is DeviceUserGroup)) node = node.Parent;
            if (node != null)
            {
                DeviceUserGroup dev_group = (DeviceUserGroup)node;
                var hmi_list = new List<HmiTarget>();
                FindHMI.HandleDeviceFolder(hmi_list, dev_group);
                popupFolders = new List<ScreenPopupFolder>();
                foreach (HmiTarget hmi in hmi_list)
                {
                    popupFolders.Add(hmi.ScreenPopupFolder);
                }
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
        }

        private void PresetGenerate_Load(object sender, EventArgs e)
        {

        }


        private void writeButton_Click(object sender, EventArgs e)
        {
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


            // Create databases for all groups
            foreach (string group_name in tag_groups.Keys)
            {

                string db_name = "sDB_Preset_" + group_name;
                var tags = tag_groups[group_name];
                try
                {

                    PresetDB db;
                    PlcBlock block = resultGroup.Blocks.Find(db_name);
                    Constant preset_count = new GlobalConstant("PresetCount_" + group_name);
                    if (block != null)
                    {
                        XmlDocument block_doc = TIAutils.ExportPlcBlockXML(block);
                        db = new PresetDB(db_name, preset_count, block_doc);
                    }
                    else
                    {
                        db = new PresetDB(db_name, preset_count);
                    }
                    foreach (var tag in tags)
                    {
                        db.AddPath(tag.tagPath, tag.labels, tag.defaultValue);
                    }
                    TIAutils.ImportPlcBlockXML(db.Document, resultGroup);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to update preset DB: " + ex.Message);
                    return;
                }

                try
                {
                    string block_name = "PresetStore_" + group_name;
                    PresetSCL scl = new PresetSCL(block_name, db_name);
                    foreach (var tag in tags)
                    {
                        if (!tag.noStore)
                        {
                            scl.AddStore(tag.tagPath);
                        }
                    }
                    TIAutils.ImportPlcBlockXML(scl.Document, resultGroup);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to update preset store SCL block: " + ex.Message);
                    return;
                }

                try
                {
                    string block_name = "PresetRecall_" + group_name;
                    PresetSCL scl = new PresetSCL(block_name, db_name);
                    foreach (var tag in tags)
                    {
                        scl.AddRecall(tag.tagPath);
                    }
                    TIAutils.ImportPlcBlockXML(scl.Document, resultGroup);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to update preset recall SCL block: " + ex.Message);
                    return;
                }


                // Create popups
                foreach (ScreenPopupFolder folder in popupFolders)
                {
                    try
                    {

                        string popup_name = "PresetPopup_" + group_name;
                        ScreenPopup popup = folder.ScreenPopups.Find(popup_name);

                        if (popup != null)
                        {

                            XmlDocument popup_doc = TIAutils.ExportScreenPopupXML(popup);


                            TIAutils.ImportScreenPopupXML(popup_doc, folder);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Failed to update popup screen: " + ex.Message);
                        return;
                    }

                }
            }
        }
    }

 
}
