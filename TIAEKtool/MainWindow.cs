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

namespace TIAtool
{
    public partial class MainForm : Form
    {
       
        public MainForm()
        {
            InitializeComponent();
            alarmList.CellFormatting +=
            new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.cellFormatter);
            disconnectToolStripMenuItem.Enabled = false;

        }

        private void saveAlarmDefinitionsToolStripMenuItem_Click(object sender, EventArgs ev)
        {
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save alarm definitions: " + ex.Message);
                }

            }
        }

        private void cellFormatter(object sender,
        System.Windows.Forms.DataGridViewCellFormattingEventArgs e)
        {
            string colName = alarmList.Columns[e.ColumnIndex].Name;
            if (colName.Equals("ColumnID"))
            {
                if (e.Value is Int32)
                {
                    int id = (int)e.Value;
                    if (id < 0)
                    {
                        e.Value = "-";
                    }


                }
            }
            else if (colName.Equals("ColumnSilent"))
            {

               
            }
            else if (colName.Equals("ColumnAutoAck"))
            {

               
            }
        }
        private void loadAlarmDefinitionsToolStripMenuItem_Click(object sender, EventArgs ev)
        {
            if (loadFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    alarmList.DataSource = null;
                 
                    alarmList.AutoGenerateColumns = false;
                  
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load alarm definitions: " + ex.Message);
                }

            }
        }

        TiaPortal tiaPortal = null;
        PortalSelect select_dialog = null;
        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {



            if (select_dialog == null)
            {
                select_dialog = new PortalSelect();
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
                        tiaPortal = proc.Attach();
                        connectToolStripMenuItem.Enabled = false;
                        disconnectToolStripMenuItem.Enabled = true;
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
                tiaPortal.Dispose();
                tiaPortal = null;
                folder_dialog = null;
                preset_block_group_dialog = null;
                browse_dialog = null;
                connectToolStripMenuItem.Enabled = true;
                disconnectToolStripMenuItem.Enabled = false;
            }

        }

        // Generate alarm definitions in project
        BrowseDialog folder_dialog;
        private void alarmDefsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (tiaPortal != null)
            {
                if (folder_dialog == null)
                {
                    folder_dialog = new BrowseDialog(tiaPortal);
                    folder_dialog.Descend = TIATree.ControllerOnly;
                    folder_dialog.Leaf = (o => o is PlcBlockGroup);
                    folder_dialog.AutoExpandMaxChildren = 1;
                    folder_dialog.AcceptText = "Generate";
                    folder_dialog.Text = "Select where to generate block";
                }
                if (folder_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (folder_dialog.SelectedObject is PlcBlockGroup)
                    {
                        PlcBlockGroup folder = (PlcBlockGroup)folder_dialog.SelectedObject;
                    
                   

                        // Move up the tree until we find a ControllerTarget
                        IEngineeringObject obj = folder.Parent;
                        while (!(obj is PlcSoftware))
                        {
                            obj = obj.Parent;
                            // Shouldn't happen, but just in case
                            if (obj == null)
                            {
                                MessageBox.Show(this, "No controller found as parent");
                                return;
                            }
                        }
                        PlcSoftware controller = (PlcSoftware)obj;
                        List<ConstTable.Constant> consts = new List<ConstTable.Constant>();
                 
                    }
                }

            }
        }

        private void alarmList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        SelectHMI hmi_dialog;
        private void HMITagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tiaPortal != null)
            {
                if (hmi_dialog == null)
                {
                    hmi_dialog = new SelectHMI(tiaPortal);

                }
                if (hmi_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    
                  
                    HmiTarget hmi = hmi_dialog.SelectedHMI;
                    Console.WriteLine("HMI name: " + hmi.Name);
                    
                    foreach (Connection conn in hmi.Connections) {
                        Console.WriteLine("Connection: " + conn.Name);
                    }
                    if (hmi.Connections.Count != 1) {
                          MessageBox.Show(this, "Can only handle exacltly one HMI connection. This device has "+hmi.Connections.Count);
                       
                            return;
                    }
                    Connection c = hmi.Connections.First();
                   
                }
            }


        }
          BrowseDialog preset_block_group_dialog;
          private void extractAlarmDefsToolStripMenuItem_Click(object sender, EventArgs e)
          {
              if (tiaPortal != null)
              {
                  if (preset_block_group_dialog == null)
                  {
                      preset_block_group_dialog = new BrowseDialog(tiaPortal);
                      preset_block_group_dialog.Descend = TIATree.ControllerOnly;
                      preset_block_group_dialog.Leaf = TIATree.SharedDBOnly;
                      preset_block_group_dialog.AutoExpandMaxChildren = 1;
                      preset_block_group_dialog.AcceptText = "Extract";
                      preset_block_group_dialog.Text = "Select alarm data block";
                  }
                  if (preset_block_group_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                  {
                      if (preset_block_group_dialog.SelectedObject is DataBlock)
                      {
                          DataBlock block = (DataBlock)preset_block_group_dialog.SelectedObject;
                          try
                          {

                              // Extract from data base
                              FileInfo file = TempFile.File("AlarmDB", "xml");
                           
                              block.Export(file, ExportOptions.WithDefaults | ExportOptions.WithReadOnly);
                             

                              // Extract from constant tags 
                              // Move up the tree until we find a ControllerTarget
                              IEngineeringObject obj = (block as IEngineeringObject).Parent;
                              while (!(obj is PlcSoftware))
                              {
                                  obj = obj.Parent;
                                  // Shouldn't happen, but just in case
                                  if (obj == null)
                                  {
                                      MessageBox.Show(this, "No controller found as parent");
                                      return;
                                  }
                              }
                              PlcSoftware controller = (PlcSoftware)obj;
                              List<ConstTable.Constant> consts = new List<ConstTable.Constant>();

                              PlcTagTable table = controller.TagTableGroup.TagTables.Find("Alarms");
                              if (table == null)
                              {
                                  MessageBox.Show(this, "No tag table named Alarms was found");
                              }
                              else
                              {
                                  file = TempFile.File("ConstantTags", "xml");
                                  Console.WriteLine("Wrote to " + file.Name);
                                  table.Export(file, ExportOptions.WithDefaults | ExportOptions.WithReadOnly);
                                  List<ConstTable.Constant> constants = ConstTable.getConstants(file);
                                  foreach (ConstTable.Constant c in constants)
                                  {
                                      if (c.Name.StartsWith("Alarm") && c.Value is int)
                                      {
                                         
                                      }
                                  }
                              }
                          

                              alarmList.AutoGenerateColumns = false;
                             
                          }
                          catch (Exception ex)
                          {
                              MessageBox.Show("Failed to extract alarm definitions: " + ex.Message);
                          }


                      }
                  }
              }
          }

          InfoDialog browse_dialog;
          private void browseToolStripMenuItem_Click(object sender, EventArgs e)
          {
              if (tiaPortal != null)
              {
                  if (browse_dialog == null)
                  {
                      browse_dialog = new InfoDialog(tiaPortal);

                      browse_dialog.AutoExpandMaxChildren = 1;
                      browse_dialog.Text = "Browse TIA portal";
                  }
                  if (browse_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                  {
                  }
              }
          }

        PresetGenerate presetGenerate;

        private void presetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (tiaPortal != null)
            {
                if (preset_block_group_dialog == null)
                {
                    preset_block_group_dialog = new BrowseDialog(tiaPortal);
                    preset_block_group_dialog.Descend = TIATree.BlockGroupOrParent;
                    preset_block_group_dialog.Leaf = TIATree.BlockOrBlockGroup;
                    preset_block_group_dialog.AutoExpandMaxChildren = 1;
                    preset_block_group_dialog.AcceptText = "Search";
                    preset_block_group_dialog.Text = "Select where to search for preset tags";
                }
                if (preset_block_group_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IEngineeringCompositionOrObject selected = (IEngineeringCompositionOrObject)preset_block_group_dialog.SelectedObject;
                    if (selected != null)
                    {
                        presetGenerate = new PresetGenerate(tiaPortal, (IEngineeringCompositionOrObject)selected);
                        presetGenerate.ShowDialog();
                    }
                }
            }
        }
    }
}
