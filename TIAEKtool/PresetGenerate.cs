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
using static TIAEKtool.PresetDocument;
using Siemens.Engineering.HmiUnified;

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
        IList<HmiSoftware> hmiSoftware;
        TaskDialog task_dialog;
        TiaPortal tiaPortal;
        MessageLog log = new MessageLog();
        public PresetGenerate(TiaPortal portal, IEngineeringCompositionOrObject top, List<HmiTarget> hmiTargets, List<HmiSoftware> hmiSoftware, string culture)
        {
            InitializeComponent();
            tiaPortal = portal;
            FormClosing += FormClosingEventHandler;
            presetListView.AutoGenerateColumns = false;
            presetList = new PresetTagList
            {
                Culture = culture
            };
            presetListView.DataSource = presetList;

            writeButton.Enabled = false;
            exportButton.Enabled = false;
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
            this.hmiTargets = hmiTargets;
            this.hmiSoftware = hmiSoftware;

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
            PresetTag preset = new PresetTag() { readTagPath = ev.Path };
           
            foreach (string c in ev.Comment.Cultures)
            {
                PresetCommentParser.Parse(ev.Comment[c], c, preset);
             
            }
            if (preset.writeTagPath == null)
            {
                preset.writeTagPath = preset.readTagPath;
            }
            if (preset.labels != null)
            {
                presetList.AddTag(preset);
            }
          
        }

        public void ParseDone(object source, TagParser.ParseDoneEventArgs ev)
        {
          
            writeButton.Enabled = resultGroup != null;
            exportButton.Enabled = resultGroup != null;
            if (log.HighestSeverity >= MessageLog.Severity.Warning)
            {
                LogDialog dialog = new LogDialog(log);
                dialog.ShowDialog();
            }
        }

        private void PresetGenerate_Load(object sender, EventArgs e)
        {

        }

        private Dictionary<string, List<PresetTag>> tagGroups(PresetTagList presets)
        {
            Dictionary<string, List<PresetTag>> tag_groups = new Dictionary<string, List<PresetTag>>();
            foreach (PresetTagList.Row r in presets)
            {
                List<PresetTag> tags;
                // Add the tags to all groups in the tag
                foreach (string group_name in r.Tag.presetGroups) {
                    if (!tag_groups.TryGetValue(group_name, out tags))
                    {
                        tags = new List<PresetTag>();
                    }
                    tags.Add(r.Tag);
                    tags.Sort();
                    tag_groups[group_name] = tags;
                }

            }
            return tag_groups;
        }
        const string PRESET_DB_PREFIX = "sDB_Preset_";
        const string PRESET_HMI_DB_PREFIX = "sDB_HMI_Preset_";
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
            foreach (PresetTagList.Row row in presetList)
            {
                row.Tag.labels.AddMissingCultures(cultures, default_culture);
                if (row.Tag.state_labels != null)
                {
                    foreach (MultilingualText text in row.Tag.state_labels.Values)
                    {
                        text.AddMissingCultures(cultures, default_culture);
                    }
                }
            }
            // Sort the groups into separate lists of tags
            Dictionary<string, List<PresetTag>> tag_groups = tagGroups(presetList);
          

            ConstantLookup constants = new ConstantLookup();
            constants.Populate(tiaPortal, plcSoftware);

            // Create databases for all groups
            foreach (string group_name in tag_groups.Keys)
            {

                string db_name = PRESET_DB_PREFIX + group_name;
                string hmi_db_name = PRESET_HMI_DB_PREFIX + group_name;
                var tags = tag_groups[group_name];

                string value_type_name = "PresetValueType_" + group_name;
                string enable_type_name = "PresetEnableType_" + group_name;

                task_dialog.AddTask(new CreatePresetTypesTask(tiaPortal, tags, typeGroup, value_type_name, enable_type_name));
                string recall_block_name = "PresetRecall_" + group_name;
                task_dialog.AddTask(new CreatePresetRecallBlockTask(tiaPortal, tags, resultGroup, recall_block_name, value_type_name, enable_type_name));
                string store_block_name = "PresetStore_" + group_name;
                string store_enabled_block_name = "PresetStoreEnabled_" + group_name;
                task_dialog.AddTask(new CreatePresetStoreBlockTask(tiaPortal, tags, resultGroup, store_block_name, store_enabled_block_name, value_type_name, enable_type_name));
            }

            task_dialog.AddTask(new CreatePlcCompileTask(tiaPortal, plcSoftware));

            foreach (HmiTarget hmi in hmiTargets)
            {
               // Create HMI tags
                TagFolder preset_tag_folder = hmi.TagFolder.Folders.Find("Preset");
                if (preset_tag_folder != null)
                {
                    task_dialog.AddTask(new CreateHmiPresetConstantTagsTask(tiaPortal,preset_tag_folder,constants));
                }
            }

            // Create HMI for all groups
            foreach (string group_name in tag_groups.Keys)
            {
                var tags = tag_groups[group_name];
                string db_name = PRESET_DB_PREFIX + group_name;
                string hmi_db_name = PRESET_HMI_DB_PREFIX + group_name;

                // Get number of presets configured
                string count_entry_name = "PresetCount_" + group_name;
                ConstantLookup.Entry count_entry = constants.Lookup(count_entry_name);
                if (count_entry == null)
                {
                    throw new Exception("Global constant " + count_entry_name + " not found");
                }


                int nPresets = int.Parse(count_entry.value);

                Dictionary<int, MultilingualText> preset_names = new Dictionary<int, MultilingualText>();
                // Create preset name list


                {
                    for (int p = 1; p <= nPresets; p++)
                    {
                        string name_string = "<hmitag length='20' type='Text' name='PresetName_" + group_name + "_" + p + "'>Preset " + p + "</hmitag>";
                        MultilingualText text = new MultilingualText();
                        foreach (string c in cultures)
                        {
                            text.AddText(c, name_string);
                        }
                        preset_names.Add(p, text);
                    }

                   
                }
                foreach (HmiTarget hmi in hmiTargets)
                {
                    string popup_name = "PresetPopup_" + group_name;
                    ScreenPopupFolder popup_folder = hmi.ScreenPopupFolder;
                    ScreenPopup popup = popup_folder.ScreenPopups.Find(popup_name);
                    if (popup == null)
                    {
                        task_dialog.AddTask(new MessageTask("Skipping preset group " + group_name + " for HMI " + hmi.Name,
                            MessageLog.Severity.Info, 
                            "Assuming preset group " + group_name + " is not used by this HMI since the pop-up screen "+popup_name+" was not found"));
                        continue;
                    }

                    String list_prefix = "PresetTextList_" + group_name + "_";
                    TextListComposition hmi_text_lists = hmi.TextLists;

                    // Text list that are candidates for deletion
                    List<String> delete_lists = new List<string>();
                    // Find all preset text lists
                    foreach (var list in hmi_text_lists)
                    {
                        if (list.Name.StartsWith(list_prefix))
                        {
                            delete_lists.Add(list.Name);
                        }
                    }
                    
                    // Create text lists 
                    
                    int count = 1;
                    foreach (PresetTag tag in tags)
                    {
                        if (tag.state_labels != null)
                        {
                            string list_name = list_prefix + count;
                            delete_lists.Remove(list_name); // Don't delete this list
                            task_dialog.AddTask(new CreateHmiTextListTask(tiaPortal, list_name, hmi_text_lists, tag.state_labels));
                        }
                        count++;
                    }

                    // Delete old textlists
                    task_dialog.AddTask(new DeleteHmiTextListTask(tiaPortal, list_prefix, hmi_text_lists, delete_lists));
                    {
                        string list_name = "PresetNameList_" + group_name;
                        task_dialog.AddTask(new CreateHmiTextListTask(tiaPortal, list_name, hmi_text_lists, preset_names));
                    }
                    // Create HMI tags
                    TagFolder preset_tag_folder = hmi.TagFolder.Folders.Find("Preset");
                    if (preset_tag_folder != null)
                    {
                        string table_name = "Preset_" + group_name;
                        task_dialog.AddTask(new CreatePresetHmiTagsTask(tiaPortal, tags, preset_tag_folder, table_name, group_name, db_name, hmi_db_name, nPresets));
                    }
                    else
                    {
                        MessageBox.Show("No HMI tag group name 'Preset' was found for HMI " + hmi.Name + ". No tag tables will be updated.");
                    }

                     // Load template screen

                        ScreenTemplate obj_templ = hmi.ScreenTemplateFolder.ScreenTemplates.Find("ObjectTemplate");
                    if (obj_templ != null)
                    {
                        XmlDocument templates = TIAutils.ExportScreenTemplateXML(obj_templ);

                        // Create popups
                        task_dialog.AddTask(new CreatePresetScreenPopupTask(tiaPortal, tags, popup_folder, templates, popup_name, group_name));
                    }
                    else
                    {
                        MessageBox.Show("No template screen named ObjectTemplate found for HMI " + hmi.Name+". Some screens will not be updated.");
                    }
                }

                foreach (HmiSoftware hmi in hmiSoftware)
                {
                    // Create HMI tags


                    var preset_tag_table = hmi.TagTables;
                    string table_name = "Preset_" + group_name;
                    task_dialog.AddTask(new CreatePresetUnifiedHmiTagsTask(tiaPortal, tags, preset_tag_table, table_name, group_name, db_name, hmi_db_name, presetList.Culture));
                    string popup_name = "PresetSettingsPopup_" + group_name;
                    task_dialog.AddTask(new CreatePresetUnifiedSettingsPopupTask(tiaPortal, tags, hmi.Screens, popup_name, group_name, presetList.Culture));
                }
            }
           

            task_dialog.Show();
        }

        private PlcBlock findPlcBlockName(string name, PlcBlockGroup blocks)
        {
            PlcBlock block = blocks.Blocks.Find(name);
            if (block == null)
            {
                foreach (PlcBlockGroup group in blocks.Groups)
                {
                    block = findPlcBlockName(name, group);
                    if (block != null) break;
                }
            }
            return block;
        }
        private PlcBlock findPlcBlock(PathComponent path, PlcBlockGroup blocks)
        {
            while (path.Parent != null)
            {
                path = path.Parent;
            }
            if (!(path is MemberComponent)) return null;
            string name = ((MemberComponent)path).Name;
            Console.WriteLine("Name "+name);
            return findPlcBlockName(name,blocks);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {

            if (savePresetList.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    ConstantLookup constants = new ConstantLookup();
                    constants.Populate(tiaPortal, plcSoftware);

                    Dictionary<string, List<PresetTag>> tag_groups = tagGroups(presetList);

                    PlcBlockGroup plc_preset_group = plcSoftware.BlockGroup.Groups.Find("Preset");
                    if (plc_preset_group == null)
                    {
                        MessageBox.Show("No group named Preset found for PLC " + plcSoftware.Name);
                        return;
                    }
                    Dictionary<string, PresetGroup> preset_groups = new Dictionary<string, PresetGroup>();


                    foreach (string group_name in tag_groups.Keys)
                    {
                        PresetGroup group = new PresetGroup();
                        string preset_db_name = "sDB_Preset_" + group_name;
                        PlcBlock preset_db = plc_preset_group.Blocks.Find(preset_db_name);
                        if (preset_db == null)
                        {
                            MessageBox.Show("No block named " + preset_db_name + " found for PLC " + plcSoftware.Name);
                            return;
                        }
                        XmlDocument doc;
                        try
                        {
                            doc = TIAutils.ExportPlcBlockXML(preset_db);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to export block " + preset_db_name + ": " + ex.Message);
                            return;
                        }

                        if (doc.DocumentElement.SelectSingleNode("/Document/SW.Blocks.GlobalDB//if:Section[@Name='Static']", XMLUtil.nameSpaces) is XmlElement static_elem)
                        {
                            group.preset_names = PresetValueParser.GetPresetNames(static_elem, constants);
                            group.preset_colors = PresetValueParser.GetPresetColors(static_elem, constants);
                            group.presets = new List<PresetDocument.PresetInfo>();
                            var tags = tag_groups[group_name];
                            foreach (var tag in tags)
                            {
                                var values = PresetValueParser.GetPresetValue(static_elem, tag.readTagPath, constants);
                                var enabled = PresetValueParser.GetPresetEnabled(static_elem, tag.readTagPath, constants);
                                Console.WriteLine(tag.readTagPath + ":" + (string.Join(",", values)));
                                group.presets.Add(new PresetDocument.PresetInfo(){ tag = tag, values = values, enabled = enabled});
                            }

                            preset_groups[group_name] = group;
                        }
                        else
                        {
                            MessageBox.Show("No static section found for " + preset_db_name);
                            return;
                        }
                        
                    }
                   


                    PresetDocument.Save(savePresetList.FileName, preset_groups, cultureComboBox.SelectedItem.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to export preset list: " + ex.Message);
                }

            }
        }

        private void cultureComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            presetList.Culture = cultureComboBox.SelectedItem.ToString();
        }
    }

 
}
