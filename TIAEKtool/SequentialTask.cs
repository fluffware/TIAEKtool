using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public abstract class SequentialTask:INotifyPropertyChanged
    {
        public enum Severity
        {
            None = 0,
            Debug,
            Info,
            Warning,
            Error
        }

        #region Events




        public event EventHandler<RunWorkerCompletedEventArgs> TaskCompleted;
        public event ProgressChangedEventHandler ProgressChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        protected BackgroundWorker worker;

        public class LogEntry
        {
            public Severity Severity { get; protected set; }
            public string Message { get; protected set; }
            public LogEntry(Severity severity, string message)
            {
                Severity = severity;
                Message = message;
            }
        }
        protected List<LogEntry> log = new List<LogEntry>();
   
        #region Properties
        public IList<SequentialTask> DependantTasks { get; protected set; } // Must be run after this task
        public SequentialTask RequisiteTask { get; protected set; } // Must be run before this task
        public string Description { get; set; }
        bool selected;
        public bool Selected { get => selected; set { selected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected))); } }
        public IList<LogEntry> Log { get => log; }
        public Severity LogSeverity { get; protected set; }
        public bool IsRunning { get => worker != null; }
        public bool IsCompleted { get; protected set; }
        #endregion

        public SequentialTask()
        {
            DependantTasks = new List<SequentialTask>();
            Description = "<none>";
            Selected = true;
            IsCompleted = false;
            LogSeverity = Severity.None;
        }


     

       

        #region worker events
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressChanged(this, e);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        { 
            TaskCompleted(this, e);
        }

        protected void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (worker)
            {
                DoWork();
                IsCompleted = true;
                worker = null;
            }
        }
        #endregion

        #region Public methods

        public void AddDependantTask(SequentialTask task)
        {
            DependantTasks.Add(task);
            task.RequisiteTask = this;
        }

        public void LogMessage(Severity severity, string message)
        {
            log.Add(new LogEntry(severity, message));
            if (severity > LogSeverity)
            {
                LogSeverity = severity;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogSeverity)));
        }

        public virtual bool Start()
        {
            if (IsCompleted || IsRunning) return false;
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted; ;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerAsync();
            return true;
        }

        public virtual void Cancel()
        {
            BackgroundWorker w = worker;
            if (w != null)
            {
                w.CancelAsync();
                lock(worker) // Wait for task to end 
                {
                }
            }
        }

        /// <summary>
        /// Reset the task so it can be started again
        /// </summary>
        public virtual void Reset()
        {
            Cancel();
        }
        #endregion

        /// <summary>
        /// The actual work is done here
        /// </summary>
        protected abstract void DoWork();

    }
}
