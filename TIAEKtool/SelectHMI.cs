using System;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HW.Utilities;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.TechnologicalObjects;
using Siemens.Engineering.SW.TechnologicalObjects.Motion; 
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Cycle;
using Siemens.Engineering.Hmi.Communication;
using Siemens.Engineering.Hmi.Globalization;
using Siemens.Engineering.Hmi.TextGraphicList;
using Siemens.Engineering.Hmi.RuntimeScripting;
using System.Collections.Generic; 
using Siemens.Engineering.Online;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.Library;
using Siemens.Engineering.Library.Types;
using Siemens.Engineering.Library.MasterCopies;
using Siemens.Engineering.Compare;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace TIAtool
{
    public partial class SelectHMI : Form
    {
        TiaPortal tiaPortal = null;
        public SelectHMI(TiaPortal portal)
        {
            tiaPortal = portal;
            InitializeComponent();
            VisibleChanged += updateList;
            itemTree.MouseDoubleClick += treeDoubleClick;
        }

        protected void updateList(object sender, EventArgs e)
        {
            if (Visible)
            {
                itemTree.Nodes.Clear();
                buildPortalTree(itemTree.Nodes, tiaPortal.Projects);
                selectBtn.Enabled = false;
            }
        }

        class HmiTargetNode : TreeNode
        {
            public HmiTarget hmi;
            public HmiTargetNode(string name, HmiTarget hmi)
                : base(name)
            {
                this.hmi = hmi;
            }

        }
     
        private void buildDeviceItemTree(TreeNodeCollection nodes, DeviceItemComposition items)
        {
            foreach (DeviceItem item in items) {
            
             
                SoftwareContainer cont = item.GetService<SoftwareContainer>();
                HmiTarget hmi = cont.Software as HmiTarget;
                if (hmi != null)
                {
                    TreeNode itemNode = new HmiTargetNode(item.Name, hmi);
                    nodes.Add(itemNode);
                }
                else
                {
                    buildDeviceItemTree(nodes, item.DeviceItems);
                }
            }
        }
        private void buildDeviceTree(TreeNodeCollection nodes, DeviceComposition devices)
        {
            foreach (Device d in devices)
            {
                TreeNode deviceNode = new TreeNode(d.Name);
                nodes.Add(deviceNode);
            
                buildDeviceItemTree(deviceNode.Nodes, d.DeviceItems);
            }

        }
        private void buildDeviceFolderTree(TreeNodeCollection nodes, DeviceUserGroupComposition folders)
        {
            foreach (DeviceUserGroup f in folders)
            {
                TreeNode folderNode = new TreeNode(f.Name);
                nodes.Add(folderNode);
                buildDeviceFolderTree(folderNode.Nodes, f.Groups);
                buildDeviceTree(folderNode.Nodes, f.Devices);
            }
        }
        private void buildProjectTree(TreeNodeCollection nodes, Project proj)
        {
            TreeNode projectNode = new TreeNode(proj.Path.Name);
            nodes.Add(projectNode);

            TreeNode deviceNode = new TreeNode("Devices");
            projectNode.Nodes.Add(deviceNode);
            buildDeviceFolderTree(deviceNode.Nodes, proj.DeviceGroups);
            buildDeviceTree(deviceNode.Nodes, proj.Devices);

        }

        private void buildPortalTree(TreeNodeCollection nodes, ProjectComposition projs)
        {
            foreach (Project p in projs)
            {
                buildProjectTree(nodes, p);
            }
        }

        private void itemTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = itemTree.SelectedNode;
            if (node is HmiTargetNode)
            {
                selectBtn.Enabled = true;
                SelectedHMI = (node as HmiTargetNode).hmi;
            }
            else
            {
                selectBtn.Enabled = false;
            }
        }

        public HmiTarget SelectedHMI {get; private set; }

        private void treeDoubleClick(object sender, EventArgs e)
        {
            selectBtn.PerformClick();
        }
    }
}
