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

        protected void langClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                culture = (string)item.Tag;
            }
        }

        protected void langDropDownOpened(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                foreach (ToolStripDropDownItem item in ((ToolStripDropDownItem)sender).DropDownItems)
                {
                    ToolStripMenuItem menu_item = item as ToolStripMenuItem;
                    if (menu_item != null)
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
        private void node_AfterCheck(object sender, TreeViewEventArgs e)
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
            projectTreeView.AfterCheck += node_AfterCheck;
            builder.StartBuild(projectTreeView.Nodes);

            if (tiaPortal.Projects.Count > 0)
            {
                languageToolStripMenuItem.DropDownItems.Clear();
                Project proj = tiaPortal.Projects[0];
                LanguageAssociation langs = proj.LanguageSettings.ActiveLanguages;
                culture = proj.LanguageSettings.EditingLanguage.Culture.Name;
                foreach (Language l in langs)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(l.Culture.Name);
                    item.Tag = l.Culture.Name;
                    item.Click += langClicked;

                    languageToolStripMenuItem.DropDownItems.Add(item);
                }
                languageToolStripMenuItem.DropDownOpened += langDropDownOpened;

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
        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
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
                        tiaPortal = (TiaPortal)tiaThread.RunSync((_) => { return proc.Attach(); }, null) ;
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

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
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
        private void browseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tiaPortal != null)
            {
                if (browse_dialog == null)
                {
                    browse_dialog = new InfoDialog(tiaThread, tiaPortal);

                    browse_dialog.AutoExpandMaxChildren = 1;
                    browse_dialog.Text = "Browse TIA portal";
                }
                if (browse_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                }
            }
        }

        PresetGenerate presetGenerate;

      
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void btn_connect_Click(object sender, EventArgs e)
        {
            connectToolStripMenuItem_Click(sender, e);
        }

        private void btn_disconnect_Click(object sender, EventArgs e)
        {
            disconnectToolStripMenuItem_Click(sender, e);
        }

       
        private bool find_plc(TreeNodeCollection nodes, ref PlcSoftware plc)
        {
            foreach (TreeNode n in nodes)
            {
                if (!find_plc(n.Nodes, ref plc)) return false;
                if (!n.Checked) continue;
                if (!(n.Tag is DeviceItem)) continue;
                SoftwareContainer sw_cont = ((DeviceItem)n.Tag).GetService<SoftwareContainer>();

                if (sw_cont != null)
                {
                    PlcSoftware controller = sw_cont.Software as PlcSoftware;
                    if (controller != null)
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

        private bool find_hmi(TreeNodeCollection nodes, ref HmiTarget hmi)
        {
            foreach (TreeNode n in nodes)
            {
                if (!find_hmi(n.Nodes, ref hmi)) return false;
                if (!n.Checked) continue;
                if (!(n.Tag is DeviceItem)) continue;
                SoftwareContainer sw_cont = ((DeviceItem)n.Tag).GetService<SoftwareContainer>();

                if (sw_cont != null)
                {
                    HmiTarget hmi_target = sw_cont.Software as HmiTarget;
                    if (hmi_target != null)
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

        private void btn_preset_Click(object sender, EventArgs e)
        {
            PlcSoftware plc = null;
            if (!find_plc(projectTreeView.Nodes, ref plc))
            {
                MessageBox.Show("More than one PLC is selected");
                return;
            }

            if (plc == null)
            {
                MessageBox.Show("No PLC is selected");
                return;
            }

            presetGenerate = new PresetGenerate(tiaPortal, plc.BlockGroup, culture);
            presetGenerate.ShowDialog();


        }

        private void btn_hmi_tags_Click(object sender, EventArgs e)
        {
            PlcSoftware plc = null;
            if (!find_plc(projectTreeView.Nodes, ref plc))
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
            if (!find_hmi(projectTreeView.Nodes, ref hmi))
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

        private void btn_copy_Click(object sender, EventArgs e)
        {
            PlcSoftware plc = null;
            if (!find_plc(projectTreeView.Nodes, ref plc))
            {
                MessageBox.Show("More than one PLC is selected");
                return;
            }

            HmiTarget hmi = null;
            if (!find_hmi(projectTreeView.Nodes, ref hmi))
            {
                MessageBox.Show("More than one HMI device is selected");
                return;
            }

            if (hmi == null && plc == null)
            {
                MessageBox.Show("No devices are selected");
                return;
            }
            CopySelection copy = new CopySelection(tiaPortal,plc, hmi);
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
                MessageBox.Show("Operation done"+(int)obj);
            }


        }
        private void startTIAOpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TIAAsyncWrapper.Task task = new TaskDebug();
            tiaThread.RunAsync(task);
        }

        private void startSyncTIAOpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            object res = tiaThread.RunSync((object arg) =>
            {
                Console.WriteLine("TIA sync op started");
                Thread.Sleep(5000);
                Console.WriteLine("TIA sync op ended");
                //throw new NotImplementedException();
                return 6;
            }, null);

            Console.WriteLine("Result: "+res);
        }
    }
}
