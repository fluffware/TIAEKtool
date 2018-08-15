using Siemens.Engineering;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TIAEKtool
{
    public class HMItagBuilder : TaskDialog
    {
        protected BackgroundWorker find_db_worker;
        protected TiaPortal portal;
        protected PlcSoftware plc;
        protected HmiTarget hmi;
        ConstantLookup constants;
        public HMItagBuilder(TiaPortal portal, PlcSoftware plc, HmiTarget hmi, ConstantLookup constants)
        {
            this.portal = portal;
            this.plc = plc;
            this.hmi = hmi;
            this.constants = constants;
            btn_run.Enabled = false;
            FindDB();
        }
         
        protected void FindDB()
        {
            find_db_worker = new BackgroundWorker();
            find_db_worker.DoWork += FindDB_DoWork;
            find_db_worker.RunWorkerCompleted += FindDB_RunWorkerCompleted;
            find_db_worker.ProgressChanged += FindDB_ProgressChanged;
            find_db_worker.WorkerReportsProgress = true;
            find_db_worker.RunWorkerAsync();
        }

        protected class HMItagInfo {
            public PlcBlock block;
            public TagFolder folder;
            public string function;
            public HMItagInfo(PlcBlock block, TagFolder folder,string function)
            {
                this.block = block;
                this.folder = folder;
                this.function = function;
            }
        }

        private void FindDB_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            HMItagInfo info = e.UserState as HMItagInfo;
            if (info != null)
            {
                AddTask(new CreateHmiTagsFromBlockTask(portal, info.block, info.folder, info.function, constants));
            }
        }

        protected void FindDB_RunWorkerCompleted(object s, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("FindDb complete");
            if (e.Error != null)
            {
                MessageBox.Show("Failed to find DBs for HMI tags: " + e.Error.Message);
                return;
            }
            btn_run.Enabled = true;
        }

        protected void BlockGroupIter(PlcBlockGroup group, string function, string HMI_group)
        {
            foreach (PlcBlock block in group.Blocks)
            {
                Console.WriteLine("Block " + block.Name);
                if (block.ProgrammingLanguage == ProgrammingLanguage.DB)
                {
                    if (block.HeaderFamily == "HMI" || block.HeaderFamily == "Settings")
                    {
                        find_db_worker.ReportProgress(50, new HMItagInfo(block, hmi.TagFolder, function));
                        
                    }
                }
            }

        }
        protected void FindDB_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (portal)
            {

                foreach (PlcBlockGroup function_group in plc.BlockGroup.Groups)
                {
                    string function = function_group.Name;
                    BlockGroupIter(function_group, function, null);
                    foreach (PlcBlockGroup hmi_group in function_group.Groups)
                    {
                        BlockGroupIter(function_group, function, hmi_group.Name);
                    }

                }

            }
        }
    }
}
