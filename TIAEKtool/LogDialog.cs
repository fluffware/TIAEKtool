using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TIAEKtool
{
    public partial class LogDialog : Form
    {
        
        public LogDialog(IList<SequentialTask.LogEntry> log)
        {
            InitializeComponent();
            ImageButtonCell status_btn = new ImageButtonCell();
            status_btn.FlatStyle = FlatStyle.Flat;
            logListView.Columns["severity"].CellTemplate = status_btn;

         
            logListView.DataSource = log;
        }


    }
}
