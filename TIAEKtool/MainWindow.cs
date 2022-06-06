using System;
using Siemens.Engineering;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Communication;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TIAEKtool;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using System.Threading;
using static TIAEKtool.PresetDocument;
using System.Xml;
using Siemens.Engineering.HmiUnified;

namespace TIAtool
{
    public partial class MainForm : Form
    {

        protected string culture = null;
        protected TaskDialog task_dialog = null;

        protected TIAAsyncWrapper tiaThread;
        protected TiaPortal tiaPortal = null;

        protected TIATree.TreeNodeBuilder builder;

        public MainForm()
        {
            InitializeComponent();
            disconnectToolStripMenuItem.Enabled = false;
            btn_disconnect.Enabled = false;

            FormClosing += FormClosingEventHandler;

            // Project tree
            AutoExpandMaxChildren = -1;

            tiaThread = new TIAAsyncWrapper();
        }

        static bool ProjectDescend(object obj)
        {
            return obj is Project || obj is DeviceUserGroup || obj is Device || obj is DeviceItem;

        }

        static bool ProjectLeaf(object obj)
        {
            if (!(obj is DeviceItem)) return false;
            SoftwareContainer sw_cont = ((DeviceItem)obj).GetService<SoftwareContainer>();
            return sw_cont != null;
        }

        protected void LangClicked(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                culture = (string)item.Tag;
            }
        }

        protected void LangDropDownOpened(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                foreach (ToolStripDropDownItem item in ((ToolStripDropDownItem)sender).DropDownItems)
                {
            
                    if (item is ToolStripMenuItem menu_item)
                    {
                        menu_item.Checked = (culture.Equals(item.Tag));
                    }
                }
            }
        }

        // Updates all child tree nodes recursively.
        private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                node.Checked = nodeChecked;

                // If the current node has child nodes, call the CheckAllChildsNodes method recursively.
                this.CheckAllChildNodes(node, nodeChecked);

            }
        }

        // After a tree node's Checked property is changed, all its child nodes are updated to the same value.
        private void NodeAfterCheck(object sender, TreeViewEventArgs e)
        {
            // The code only executes if the user caused the checked state to change.
            if (e.Action != TreeViewAction.Unknown)
            {
                /* Calls the CheckAllChildNodes method, passing in the current 
                Checked value of the TreeNode whose checked state changed. */
                this.CheckAllChildNodes(e.Node, e.Node.Checked);

            }
        }

        protected void PortalConnected()
        {
            builder = new TIATree.TreeNodeBuilder(tiaThread, tiaPortal);
            builder.BuildDone += TreeDone;
            builder.Descend = ProjectDescend;
            builder.Leaf = ProjectLeaf;

            projectTreeView.Nodes.Clear();
            projectTreeView.AfterCheck += NodeAfterCheck;
            builder.StartBuild(projectTreeView.Nodes);

            if (tiaPortal.Projects.Count > 0)
            {
                languageToolStripMenuItem.DropDownItems.Clear();
                Project proj = tiaPortal.Projects[0];
                LanguageAssociation langs = proj.LanguageSettings.ActiveLanguages;
                culture = proj.LanguageSettings.EditingLanguage.Culture.Name;
                foreach (Language l in langs)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(l.Culture.Name)
                    {
                        Text = l.Culture.Name,

                    };
                    item.Click += LangClicked;

                    languageToolStripMenuItem.DropDownItems.Add(item);
                }
                languageToolStripMenuItem.DropDownOpened += LangDropDownOpened;

            }


        }

        protected void PortalDisconnected()
        {
            builder.CancelBuild();
            projectTreeView.Nodes.Clear();
            builder = null;
        }



        public TIATree.Filter Descend
        {
            get { return builder.Descend; }
            set { builder.Descend = value; }
        }

        public TIATree.Filter Leaf
        {
            get { return builder.Leaf; }
            set { builder.Leaf = value; }
        }


        public int AutoExpandMaxChildren { get; set; }



        protected void TreeDone(object sender, TIATree.BuildDoneEventArgs e)
        {
            if (AutoExpandMaxChildren > 0)
            {
                TIATree.TreeNodeBuilder.Expand(projectTreeView.Nodes, AutoExpandMaxChildren);
            }

        }


        public Object SelectedObject { get; protected set; }

        private void BlockTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = projectTreeView.SelectedNode;
            if (Leaf(node.Tag))
            {
                SelectedObject = node.Tag;
            }
            else
            {

            }
        }

        protected void FormClosingEventHandler(object sender, FormClosingEventArgs e)
        {
            TIATree.TreeNodeBuilder b = builder;
            if (b != null)
            {
                b.CancelBuild();
            }
        }
        PortalSelect select_dialog = null;
        private void ConnectToolStripMenuItemClick(object sender, EventArgs e)
        {



            if (select_dialog == null)
            {
                select_dialog = new PortalSelect(tiaThread);
            }
            if (select_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TiaPortalProcess proc = select_dialog.selectedProcess();
                if (proc != null)
                {
                    WaitConnect wait = new WaitConnect();
                    wait.Show();
                    Application.DoEvents();
                    try
                    {
                        tiaPortal = (TiaPortal)tiaThread.RunSync((_) => { return proc.Attach(); }, null);
                        connectToolStripMenuItem.Enabled = false;
                        btn_connect.Enabled = false;
                        disconnectToolStripMenuItem.Enabled = true;
                        btn_disconnect.Enabled = true;
                        PortalConnected();
                    }
                    catch (EngineeringException ex)
                    {
                        MessageBox.Show("Failed to connect to TIAPortal: " + ex.Message);
                    }
                    wait.Hide();
                    wait.Dispose();

                }
            }


        }

        private void DisconnectToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (tiaPortal != null)
            {
                PortalDisconnected();
                tiaPortal.Dispose();
                tiaPortal = null;

                browse_dialog = null;
                connectToolStripMenuItem.Enabled = true;
                disconnectToolStripMenuItem.Enabled = false;
                btn_connect.Enabled = true;
                btn_disconnect.Enabled = false;

            }

        }




        InfoDialog browse_dialog;
        private void BrowseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tiaPortal != null)
            {
                if (browse_dialog == null)
                {
                    browse_dialog = new InfoDialog(tiaThread, tiaPortal)
                    {
                        AutoExpandMaxChildren = 1,
                        Text = "Browse TIA portal"
                    };
                }
                if (browse_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                }
            }
        }

        PresetGenerate presetGenerate;


        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void BtnConnectClick(object sender, EventArgs e)
        {
            ConnectToolStripMenuItemClick(sender, e);
        }

        private void BtnDisconnectClick(object sender, EventArgs e)
        {
            DisconnectToolStripMenuItemClick(sender, e);
        }


        private bool FindPlc(TreeNodeCollection nodes, ref PlcSoftware plc)
        {
            foreach (TreeNode n in nodes)
            {
                if (!FindPlc(n.Nodes, ref plc)) return false;
                if (!n.Checked) continue;
                if (!(n.Tag is DeviceItem)) continue;
                SoftwareContainer sw_cont = ((DeviceItem)n.Tag).GetService<SoftwareContainer>();

                if (sw_cont != null)
                {
                    if (sw_cont.Software is PlcSoftware controller)
                    {
                        if (plc != null)
                        {
                            return false;
                        }
                        plc = controller;

                    }

                }

            }
            return true;
        }

        private void FindPlcs(TreeNodeCollection nodes, ref List<PlcSoftware> plcs)
        {
            foreach (TreeNode n in nodes)
            {
                FindPlcs(n.Nodes, ref plcs);
                if (!n.Checked) continue;
                if (!(n.Tag is DeviceItem)) continue;
                SoftwareContainer sw_cont = ((DeviceItem)n.Tag).GetService<SoftwareContainer>();

                if (sw_cont != null)
                {
                    if (sw_cont.Software is PlcSoftware controller)
                    {
                        plcs.Add(controller);
                        
                    }

                }

            }
           
        }
        private bool FindHmi(TreeNodeCollection nodes, ref HmiTarget hmi)
        {
            foreach (TreeNode n in nodes)
            {
                if (!FindHmi(n.Nodes, ref hmi)) return false;
                if (!n.Checked) continue;
                if (!(n.Tag is DeviceItem)) continue;
                SoftwareContainer sw_cont = ((DeviceItem)n.Tag).GetService<SoftwareContainer>();

                if (sw_cont != null)
                {
                    if (sw_cont.Software is HmiTarget hmi_target)
                    {
                        if (hmi != null)
                        {
                            return false;
                        }
                        hmi = hmi_target;
                    }
                }
            }
            return true;
        }

     
        private void FindHmis(TreeNodeCollection nodes, ref List<HmiTarget> hmis)
        {
            foreach (TreeNode n in nodes)
            {
                FindHmis(n.Nodes, ref hmis);
                if (!n.Checked) continue;
                if (!(n.Tag is DeviceItem)) continue;
                SoftwareContainer sw_cont = ((DeviceItem)n.Tag).GetService<SoftwareContainer>();

                if (sw_cont != null)
                {
                    if (sw_cont.Software is HmiTarget hmi_target)
                    {
                        hmis.Add(hmi_target);
                    }
                }
            }
        }

        private void FindUnifiedHmis(TreeNodeCollection nodes, ref List<HmiSoftware> hmis)
        {
            foreach (TreeNode n in nodes)
            {
                FindUnifiedHmis(n.Nodes, ref hmis);
                if (!n.Checked) continue;
                if (!(n.Tag is DeviceItem)) continue;
                SoftwareContainer sw_cont = ((DeviceItem)n.Tag).GetService<SoftwareContainer>();
                if (sw_cont != null)
                {
                    if (sw_cont.Software is HmiSoftware hmi_sw)
                    {
                        hmis.Add(hmi_sw);
                    }
                }
            }
        }
        private void BtnPresetClick(object sender, EventArgs e)
        {
            PlcSoftware plc = null;
            if (!FindPlc(projectTreeView.Nodes, ref plc))
            {
                MessageBox.Show("More than one PLC is selected");
                return;
            }

            if (plc == null)
            {
                MessageBox.Show("No PLC is selected");
                return;
            }
            List<HmiTarget> hmis = new List<HmiTarget>();
            FindHmis(projectTreeView.Nodes, ref hmis);
            List<HmiSoftware> unified_hmis = new List<HmiSoftware>();
            FindUnifiedHmis(projectTreeView.Nodes, ref unified_hmis);
            presetGenerate = new PresetGenerate(tiaPortal, plc.BlockGroup, hmis, unified_hmis, culture);
            presetGenerate.ShowDialog();


        }

        private void BtnHmiTagsClick(object sender, EventArgs e)
        {
            PlcSoftware plc = null;
            if (!FindPlc(projectTreeView.Nodes, ref plc))
            {
                MessageBox.Show("More than one PLC is selected");
                return;
            }

            if (plc == null)
            {
                MessageBox.Show("No PLC is selected");
                return;
            }

            HmiTarget hmi = null;
            if (!FindHmi(projectTreeView.Nodes, ref hmi))
            {
                MessageBox.Show("More than one HMI device is selected");
                return;
            }

            if (hmi == null)
            {
                MessageBox.Show("No HMI device is selected");
                return;
            }
            ConstantLookup constants = new ConstantLookup();

            try
            {
                constants.Populate(tiaPortal, plc);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to build lookup table for user constants: " + ex.Message);
                return;
            }

            HMItagBuilder hmi_tags = new HMItagBuilder(tiaPortal, plc, hmi, constants);
            hmi_tags.ShowDialog();
        }

        private void BtnCopyClick(object sender, EventArgs e)
        {
            PlcSoftware plc = null;
            if (!FindPlc(projectTreeView.Nodes, ref plc))
            {
                MessageBox.Show("More than one PLC is selected");
                return;
            }

            HmiTarget hmi = null;
            if (!FindHmi(projectTreeView.Nodes, ref hmi))
            {
                MessageBox.Show("More than one HMI device is selected");
                return;
            }

            if (hmi == null && plc == null)
            {
                MessageBox.Show("No devices are selected");
                return;
            }
            CopySelection copy = new CopySelection(tiaPortal, plc, hmi);
            copy.ShowDialog();
        }
        protected override void OnClosed(EventArgs e)
        {
            tiaThread.Stop();
            base.OnClosed(e);
        }

        class TaskDebug : TIAAsyncWrapper.Task
        {
            public override object Run()
            {
                Console.WriteLine("TIA op started");
                Thread.Sleep(5000);
                Console.WriteLine("TIA op ended");
                //throw new NotImplementedException();
                return 5;
            }

            public override void CaughtException(Exception ex)
            {
                MessageBox.Show("Exception in operation: " + ex.Message);
            }

            public override void Done(object obj)
            {
                MessageBox.Show("Operation done" + (int)obj);
            }


        }
        private void StartTIAOpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TIAAsyncWrapper.Task task = new TaskDebug();
            tiaThread.RunAsync(task);
        }

        private void StartSyncTIAOpToolStripMenuItemClick(object sender, EventArgs e)
        {
            object res = tiaThread.RunSync((object arg) =>
            {
                Console.WriteLine("TIA sync op started");
                Thread.Sleep(5000);
                Console.WriteLine("TIA sync op ended");
                //throw new NotImplementedException();
                return 6;
            }, null);

            Console.WriteLine("Result: " + res);
        }

        void UpdatePresetValues(
            PlcSoftware plcSoftware, 
            Dictionary<string, PresetGroup> preset_groups)
        {
            ConstantLookup constants = new ConstantLookup();
            constants.Populate(tiaPortal, plcSoftware);



            PlcBlockGroup preset_group = plcSoftware.BlockGroup.Groups.Find("Preset");
            if (preset_group == null)
            {
                MessageBox.Show("No group named Preset found for PLC " + plcSoftware.Name);
                return;
            }

            foreach (string group_name in preset_groups.Keys)
            {
                string preset_db_name = "sDB_Preset_" + group_name;
                PlcBlock preset_db = preset_group.Blocks.Find(preset_db_name);
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
                  
                    var infos = preset_groups[group_name].presets;
                    foreach (PresetInfo info in infos)
                    {
                        PresetValueParser.SetPresetValue(static_elem, info.tag.readTagPath, constants, info.values);
                        PresetValueParser.SetPresetEnabled(static_elem, info.tag.readTagPath, constants, info.enabled);
                       
                    }
                    PresetValueParser.SetPresetNames(static_elem, constants, preset_groups[group_name].preset_names);
                    PresetValueParser.SetPresetColors(static_elem, constants, preset_groups[group_name].preset_colors);
                }
                else
                {
                    MessageBox.Show("No static section found for " + preset_db_name);
                    return;
                }
                try
                {
                    var group = (Siemens.Engineering.SW.Blocks.PlcBlockGroup)preset_db.Parent;
                    var name = group.Name;
                    TIAutils.ImportPlcBlockXML(doc, group);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to import block " + preset_db_name + ": " + ex.Message);
                    return;
                }

            }
        }
      

        private void BtnPresetImportClick(object sender, EventArgs e)
        {
            PlcSoftware plc = null;
            if (!FindPlc(projectTreeView.Nodes, ref plc))
            {
                MessageBox.Show("More than one PLC is selected");
                return;
            }

            if (plc == null)
            {
                MessageBox.Show("No PLC is selected");
                return;
            }
            if (loadPresetList.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Dictionary<string, PresetGroup> preset_groups;
                try
                {
                    PresetDocument.Load(loadPresetList.FileName, out preset_groups, culture);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load presets from file "+ loadPresetList.FileName+": " + ex.Message);
                    return;
                }
                try
                {
                    UpdatePresetValues(plc, preset_groups);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update presets loaded from file "+ loadPresetList.FileName+": " + ex.Message);
                    return;
                }
             
            }
        }

        private void BtnCompileDownloadClick(object sender, EventArgs e)
        {
            var plcs = new List<PlcSoftware>();
            FindPlcs(projectTreeView.Nodes, ref plcs);
            
            //var hmis = new List<HmiTarget>();
            
            
        }
    }
}
