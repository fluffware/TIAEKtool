using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.SW;
using System.Threading;
using TIAEKtool;
using Siemens.Engineering.HmiUnified.HmiTags;

namespace TIAtool
{
    public partial class InfoDialog : Form
    {
        TIATree.TreeNodeBuilder builder;
        TiaPortal portal;
        public InfoDialog(TIAAsyncWrapper thread, TiaPortal portal)
        {
            InitializeComponent();
            AutoExpandMaxChildren = -1;
            this.portal = portal;
            builder = new TIATree.TreeNodeBuilder(thread, portal);
            builder.BuildDone += TreeDone;
            VisibleChanged += updateList;
            FormClosing += FormClosingEventHandler;
            blockTree.MouseDoubleClick += treeDoubleClick;
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

        protected void updateList(object sender, EventArgs e)
        {
            if (Visible)
            {
                blockTree.Nodes.Clear();
                builder.StartBuild(blockTree.Nodes);

            }
        }

        protected void TreeDone(object sender, TIATree.BuildDoneEventArgs e)
        {
            if (AutoExpandMaxChildren > 0)
            {
                TIATree.TreeNodeBuilder.Expand(blockTree.Nodes, AutoExpandMaxChildren);
            }

        }

        protected void FormClosingEventHandler(object sender, FormClosingEventArgs e)
        {
            builder.CancelBuild();
        }

        private void treeDoubleClick(object sender, EventArgs e)
        {

        }

        public Object SelectedObject { get; protected set; }

        private void blockTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            bool locked = false;
            Monitor.TryEnter(portal, 100, ref locked);
            if (!locked) return;
            try
            {
                TreeNode node = blockTree.SelectedNode;
               
                attrList.Items.Clear();
                if (node.Tag is HmiTag hmi_tag)
                {
                    attrList.Items.Add(new ListViewItem(new String[] { "DataType", hmi_tag.DataType }));
                    attrList.Items.Add(new ListViewItem(new String[] { "Connection", hmi_tag.Connection }));
                    attrList.Items.Add(new ListViewItem(new String[] { "HmiDataType", hmi_tag.HmiDataType }));
                    attrList.Items.Add(new ListViewItem(new String[] { "PlcName", hmi_tag.PlcName }));
                    attrList.Items.Add(new ListViewItem(new String[] { "PlcTag", hmi_tag.PlcTag }));
                }
                else if (node.Tag is IEngineeringObject ie)
                {
                    try
                    {
                        foreach (EngineeringAttributeInfo ai in ie.GetAttributeInfos())
                        {
                            Object value = ie.GetAttribute(ai.Name);
                            String valStr;
                            if (value != null)
                            {
                                valStr = value.ToString();
                            }
                            else
                            {
                                valStr = "(null)";
                            }
                            attrList.Items.Add(new ListViewItem(new String[] { ai.Name, valStr }));

                        }
                        attrList.Items.Add(new ListViewItem(new String[] {"Parent", ie.Parent.ToString() }));
                    }
                    catch (Exception ex)
                    {
                        attrList.Items.Add(new ListViewItem(new String[] { "Exception", ex.ToString() }));
                    }
                }
                if (node.Tag is Siemens.Engineering.SW.Blocks.PlcBlock
                    || node.Tag is Siemens.Engineering.Hmi.Screen.Screen
                    || node.Tag is Siemens.Engineering.Hmi.Screen.ScreenTemplate
                    || node.Tag is Siemens.Engineering.Hmi.Screen.ScreenPopup)
                {
                    ExportBtn.Enabled = true;

                }
                else
                {
                    ExportBtn.Enabled = false;
                }

                if (node.Tag is Siemens.Engineering.SW.Blocks.PlcBlockGroup
                    || node.Tag is Siemens.Engineering.Hmi.Screen.ScreenFolder
                    || node.Tag is Siemens.Engineering.Hmi.Screen.ScreenTemplateFolder
                    || node.Tag is Siemens.Engineering.Hmi.Screen.ScreenPopupFolder)
                {
                    ImportBtn.Enabled = true;

                }
                else
                {
                    ImportBtn.Enabled = false;
                }
            }
            finally
            {
                Monitor.Exit(portal);
            }

        }

        private void AttrBtn_Click(object sender, EventArgs e)
        {

        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            bool locked = false;
            Monitor.TryEnter(portal, 100, ref locked);
            if (!locked) return;
            try
            {
                TreeNode node = blockTree.SelectedNode;
                if (node.Tag is Siemens.Engineering.SW.Blocks.PlcBlock)
                {
                    Siemens.Engineering.SW.Blocks.PlcBlock block = node.Tag as Siemens.Engineering.SW.Blocks.PlcBlock;
                    String block_name = block.GetAttribute("Name") as String;
                    if (block_name != null)
                    {
                        exportFileDialog.FileName = block_name + ".xml";
                    }
                    if (exportFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            block.Export(new FileInfo(exportFileDialog.FileName), ExportOptions.WithDefaults);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to export block: " + ex.Message);
                        }

                    }
                }
                else if (node.Tag is Siemens.Engineering.Hmi.Screen.Screen)
                {
                    Siemens.Engineering.Hmi.Screen.Screen screen = node.Tag as Siemens.Engineering.Hmi.Screen.Screen;
                    String screen_name = screen.Name;
                    if (screen_name != null)
                    {
                        exportFileDialog.FileName = screen_name + ".xml";
                    }
                    if (exportFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            screen.Export(new FileInfo(exportFileDialog.FileName), ExportOptions.WithDefaults);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to export screen: " + ex.Message);
                        }

                    }
                }
                else if (node.Tag is Siemens.Engineering.Hmi.Screen.ScreenTemplate)
                {
                    Siemens.Engineering.Hmi.Screen.ScreenTemplate template = node.Tag as Siemens.Engineering.Hmi.Screen.ScreenTemplate;
                    String name = template.Name;
                    if (name != null)
                    {
                        exportFileDialog.FileName = name + ".xml";
                    }
                    if (exportFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            template.Export(new FileInfo(exportFileDialog.FileName), ExportOptions.WithDefaults);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to export screen template: " + ex.Message);
                        }

                    }
                }
                else if (node.Tag is Siemens.Engineering.Hmi.Screen.ScreenPopup)
                {
                    Siemens.Engineering.Hmi.Screen.ScreenPopup popup = node.Tag as Siemens.Engineering.Hmi.Screen.ScreenPopup;
                    String name = popup.Name;
                    if (name != null)
                    {
                        exportFileDialog.FileName = name + ".xml";
                    }
                    if (exportFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            popup.Export(new FileInfo(exportFileDialog.FileName), ExportOptions.WithDefaults);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to export screen popup: " + ex.Message);
                        }

                    }
                }
            }
            finally
            {
                Monitor.Exit(portal);
            }
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            bool locked = false;
            Monitor.TryEnter(portal, 100, ref locked);
            if (!locked) return;
            try
            {
                TreeNode node = blockTree.SelectedNode;
                if (node.Tag is Siemens.Engineering.SW.Blocks.PlcBlockGroup)
                {
                    Siemens.Engineering.SW.Blocks.PlcBlockGroup group = node.Tag as Siemens.Engineering.SW.Blocks.PlcBlockGroup;
                    String block_name = "NewBlock";
                    if (block_name != null)
                    {
                        importFileDialog.FileName = block_name + ".xml";
                    }
                    if (importFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            group.Blocks.Import(new FileInfo(importFileDialog.FileName), ImportOptions.Override);
                            // block.Import(new FileInfo(exportFileDialog.FileName), ExportOptions.WithDefaults);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to import block: " + ex.Message);
                        }

                    }
                }
                else if (node.Tag is Siemens.Engineering.Hmi.Screen.ScreenFolder)
                {
                    Siemens.Engineering.Hmi.Screen.ScreenFolder screen_folder = node.Tag as Siemens.Engineering.Hmi.Screen.ScreenFolder;
                    String screen_name = "NewScreen";
                    if (screen_name != null)
                    {
                        importFileDialog.FileName = screen_name + ".xml";
                    }
                    if (importFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            screen_folder.Screens.Import(new FileInfo(importFileDialog.FileName), ImportOptions.Override);
                            // block.Import(new FileInfo(exportFileDialog.FileName), ExportOptions.WithDefaults);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to import screen: " + ex.Message);
                        }

                    }
                }
                else if (node.Tag is Siemens.Engineering.Hmi.Screen.ScreenTemplateFolder)
                {
                    Siemens.Engineering.Hmi.Screen.ScreenTemplateFolder folder = node.Tag as Siemens.Engineering.Hmi.Screen.ScreenTemplateFolder;
                    String name = "NewScreenTemplate";
                    if (name != null)
                    {
                        importFileDialog.FileName = name + ".xml";
                    }
                    if (importFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            folder.ScreenTemplates.Import(new FileInfo(importFileDialog.FileName), ImportOptions.Override);
                          
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to import screen template: " + ex.Message);
                        }

                    }
                }
                else if (node.Tag is Siemens.Engineering.Hmi.Screen.ScreenPopupFolder)
                {
                    Siemens.Engineering.Hmi.Screen.ScreenPopupFolder folder = node.Tag as Siemens.Engineering.Hmi.Screen.ScreenPopupFolder;
                    String name = "NewScreenPopup";
                    if (name != null)
                    {
                        importFileDialog.FileName = name + ".xml";
                    }
                    if (importFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            folder.ScreenPopups.Import(new FileInfo(importFileDialog.FileName), ImportOptions.Override);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to import screen popup: " + ex.Message);
                        }

                    }
                }

            }
            finally
            {
                Monitor.Exit(portal);
            }

        }
    }
}
