using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Siemens.Engineering;
using TIAEKtool;

namespace TIAtool
{
    public partial class PortalSelect : Form
    {
        TIAAsyncWrapper tiaThread;
        public PortalSelect(TIAAsyncWrapper thread)
        {
            tiaThread = thread;
            InitializeComponent();
            listBox1.MouseDoubleClick += listDoubleClick;
        }


        // Item that has the processes path as string representation
        private class ProcItem
        {
            public ProcItem(TiaPortalProcess proc, string path)
            {
                this.proc = proc;
                this.path = path;
            }

            public TiaPortalProcess proc;
            public string path;


            public override string ToString()
            {
                
                return path;
            }
        }
        private void PortalSelect_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            List<ProcItem> items = new List<ProcItem>();
            tiaThread.RunSync((_) =>
            {
               
                foreach (TiaPortalProcess proc in TiaPortal.GetProcesses())
                {
                    FileInfo proj = proc.ProjectPath;
                    string path = null;
                    if (proj != null)
                    {
                        path = proj.Name;
                    }
                    if (path == null)
                    {
                        path = "No project loaded";
                    }
                    ProcItem item = new ProcItem(proc, path);
                    items.Add(item);
                }
                return null;
            }, null);
            foreach (ProcItem item in items)
            {
                listBox1.Items.Add(item);
                listBox1.SetSelected(0, true);
            }
            connectBtn.Enabled = (listBox1.SelectedItem != null);
        }

        public TiaPortalProcess selectedProcess()
        {
            if (listBox1.SelectedItem == null) return null;
            return ((ProcItem)listBox1.SelectedItem).proc;
        }

        private void listDoubleClick(object sender, EventArgs e)
        {
            connectBtn.PerformClick();
        }
        
    }
}
