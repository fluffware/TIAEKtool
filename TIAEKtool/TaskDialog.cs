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
    public partial class TaskDialog : Form
    {

        bool canceled = false;
        BindingList<SequentialTask> tasks = null;
        private IEnumerator<SequentialTask> current_task = null;
        public TaskDialog()
        {
            InitializeComponent();
            FormClosing += TaskDialog_FormClosing;
            btn_run.Enabled = true;
            btn_cancel.Enabled = false;
            btn_done.Enabled = true;

            ImageButtonCell log_btn = new ImageButtonCell();
            taskListView.Columns["Log"].CellTemplate = log_btn;
            taskListView.AutoGenerateColumns = false;
            tasks = new BindingList<SequentialTask>();
          
       
            taskListView.DataSource = tasks;
        }

        public void AddTask(SequentialTask task)
        {
            tasks.Add(task);
        }

        public void Clear()
        {
            CancelTasks();
            tasks.Clear();
        }

        private void TaskDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Only hide the window
            e.Cancel = true;
            Hide();
        }

        private void btn_run_Click(object sender, EventArgs e)
        {
            if (tasks == null)
            {
                Stopped();
                return;
            }
            current_task = tasks.GetEnumerator();
            btn_cancel.Enabled = true;
            btn_run.Enabled = false;
            btn_done.Enabled = false;
            RunNextTask();
        }

        protected void Stopped()
        {
            canceled = false;
            current_task = null;
            btn_cancel.Enabled = false;
            btn_done.Enabled = true;
            btn_run.Enabled = true;
        }

        protected void RunNextTask()
        {
            while (current_task.MoveNext())
            {
                SequentialTask t = current_task.Current;
                if (t.Selected)
                {
                    t.TaskCompleted += TaskCompleted;
                    if (t.Start()) return;
                }
            }
            Stopped();
            return;
        }

        protected void TaskCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SequentialTask t = current_task.Current;
            t.Selected = false;
            if (canceled)
            {
                Stopped();
                return;
            }
            RunNextTask();
        }

        public void CancelTasks()
        {
            if (current_task == null)
            {
                return;
            }
            canceled = true;
            current_task.Current.Cancel();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            CancelTasks();
        }

        private void taskListView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == taskListView.Columns["Log"].Index)
            {
                LogDialog log_viewer = new LogDialog(tasks[e.RowIndex].Log);
                log_viewer.ShowDialog();
            }
        }

        private void btn_done_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
