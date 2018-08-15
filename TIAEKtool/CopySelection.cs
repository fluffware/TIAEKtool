using Siemens.Engineering;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace TIAEKtool
{
    public partial class CopySelection : Form
    {
        TiaPortal portal;
        PlcSoftware plc;
        HmiTarget hmi;
        BackgroundWorker worker;
        public CopySelection(TiaPortal portal, PlcSoftware plc, HmiTarget hmi)
        {
            this.portal = portal;
            this.plc = plc;
            this.hmi = hmi;
            InitializeComponent();
            fromTreeView.AfterCheck += node_AfterCheck;
            toTreeView.AfterCheck += node_AfterCheck;
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            
            worker.RunWorkerAsync();
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


        public enum NodeDest
        {
            From,
            To
        }
        class NodeEvent
        {
            public NodeDest Dest;
            public NodeEvent(NodeDest dest)
            {
                Dest = dest;
            }
        }

            class NodeEnter: NodeEvent
        {
            public string Name;
            public NodeEnter(NodeDest dest, string name) : base(dest)
            {
                Name = name;
            }
        }
        class NodeExit : NodeEvent
        {
            public NodeExit(NodeDest dest) : base(dest) { }
        }

        class NodeData : NodeEvent
        {
            
            public string Name;
            public object Object;
            public NodeData(NodeDest dest, string name, object obj) : base(dest)
            {
                Name = name;
                Object = obj;
            }
        }

        private void AddBlocks(PlcBlockGroup group) {
            foreach (PlcBlock block in group.Blocks)
            {
                worker.ReportProgress(50, new NodeData(NodeDest.From,block.Name, block));
            }
            foreach (PlcBlockGroup child in group.Groups) {
                worker.ReportProgress(50, new NodeEnter(NodeDest.From, child.Name));
                AddBlocks(child);
                worker.ReportProgress(50, new NodeExit(NodeDest.From));
            }
        }

        private void AddTypes(PlcTypeGroup group)
        {
            foreach (PlcType type in group.Types)
            {
                worker.ReportProgress(50, new NodeData(NodeDest.From, type.Name, type));
            }
            foreach (PlcTypeGroup child in group.Groups)
            {
                worker.ReportProgress(50, new NodeEnter(NodeDest.From, child.Name));
                AddTypes(child);
                worker.ReportProgress(50, new NodeExit(NodeDest.From));
            }
        }

        private void AddScreens(ScreenFolder group)
        {
            foreach (Siemens.Engineering.Hmi.Screen.Screen screen in group.Screens)
            {
                worker.ReportProgress(50, new NodeData(NodeDest.From, screen.Name, screen));
            }
            foreach (ScreenFolder child in group.Folders)
            {
                worker.ReportProgress(50, new NodeEnter(NodeDest.From, child.Name));
                AddScreens(child);
                worker.ReportProgress(50, new NodeExit(NodeDest.From));
            }
        }

        private void AddTagTables(TagFolder group)
        {
            foreach (TagTable tags in group.TagTables)
            {
                worker.ReportProgress(50, new NodeData(NodeDest.From, tags.Name, tags));
            }
            foreach (TagFolder child in group.Folders)
            {
                worker.ReportProgress(50, new NodeEnter(NodeDest.From, child.Name));
                AddTagTables(child);
                worker.ReportProgress(50, new NodeExit(NodeDest.From));
            }
        }

        private void FindDeviceItems(DeviceItemAssociation items)
        {
            foreach (DeviceItem item in items)
            {

                SoftwareContainer sw_cont = item.GetService<SoftwareContainer>();
                if (sw_cont != null)
                {
                    Software sw = sw_cont.Software;

                    if (sw.Name != plc?.Name && sw.Name != hmi?.Name)
                    {
                        if (sw is PlcSoftware || sw is HmiTarget)
                        {
                            worker.ReportProgress(50, new NodeData(NodeDest.To, item.Name, sw));
                        }
                    }

                }
                FindDeviceItems(item.Items);
            }
        }

        private void FindDevices(DeviceUserGroup devices)
        {
            worker.ReportProgress(50, new NodeEnter(NodeDest.To, devices.Name));
            foreach (Device device in devices.Devices)
            {
                FindDeviceItems(device.Items); 
            }
            foreach (DeviceUserGroup group in devices.Groups)
            {
                FindDevices(group);
            }
            worker.ReportProgress(50, new NodeExit(NodeDest.To));
        }


        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (portal)
            {
                if (worker.CancellationPending) return;
                IEngineeringObject engineering_obj = null;
                if (plc != null)
                {
                    worker.ReportProgress(50,new NodeEnter(NodeDest.From, plc.Name));

                    worker.ReportProgress(50, new NodeEnter(NodeDest.From, "Blocks"));
                    AddBlocks(plc.BlockGroup);
                    worker.ReportProgress(50, new NodeExit(NodeDest.From));

                    if (worker.CancellationPending) return;

                    worker.ReportProgress(50, new NodeEnter(NodeDest.From, "Types"));
                    AddTypes(plc.TypeGroup);
                    worker.ReportProgress(50, new NodeExit(NodeDest.From));
                    worker.ReportProgress(50, new NodeExit(NodeDest.From));
                    if (worker.CancellationPending) return;

                    engineering_obj = plc;
                }

                if (hmi != null)
                {
                    worker.ReportProgress(50, new NodeEnter(NodeDest.From, hmi.Name));
                  
                    worker.ReportProgress(50, new NodeEnter(NodeDest.From, "Screens"));
                    AddScreens(hmi.ScreenFolder);
                    worker.ReportProgress(50, new NodeExit(NodeDest.From));

                    if (worker.CancellationPending) return;
                    worker.ReportProgress(50, new NodeEnter(NodeDest.From, "Tagtables"));
                    AddTagTables(hmi.TagFolder);
                    worker.ReportProgress(50, new NodeExit(NodeDest.From));
                    worker.ReportProgress(50, new NodeExit(NodeDest.From));

                    engineering_obj = hmi;
                }

                while (engineering_obj != null && !(engineering_obj is Project)) engineering_obj = engineering_obj.Parent;
                Project proj = engineering_obj as Project;
                if (proj != null)
                {
                    foreach (DeviceUserGroup group in proj.DeviceGroups)
                    {
                        FindDevices(group);
                    }
                    foreach (Device device in proj.Devices)
                    {
                        FindDeviceItems(device.Items);
                    }


                }
            }
        }

        private TreeNode CurrentParent = null;
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!(e.UserState is NodeEvent)) return;

            TreeNodeCollection nodes = null;

            if (CurrentParent != null)
            {
                nodes = CurrentParent.Nodes;
            }
            else
            {
                switch (((NodeEvent)e.UserState).Dest)
                {
                    case NodeDest.From:
                        nodes = fromTreeView.Nodes;
                        break;
                    case NodeDest.To:
                        nodes = toTreeView.Nodes;
                        break;
                }
            }
            NodeEnter enter = e.UserState as NodeEnter;
            if (enter != null)
            {
                TreeNode child = new TreeNode(enter.Name);
                nodes.Add(child);
                CurrentParent = child;
            }
            NodeExit exit = e.UserState as NodeExit;
            if (exit != null)
            {
                CurrentParent = CurrentParent.Parent;
            }
            NodeData node_data = e.UserState as NodeData;
            if (node_data != null)
            {
                TreeNode child = new TreeNode(node_data.Name);
                child.Tag = node_data.Object;
                nodes.Add(child);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
           
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
