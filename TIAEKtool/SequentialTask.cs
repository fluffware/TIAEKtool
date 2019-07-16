using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public abstract class SequentialTask:INotifyPropertyChanged, IDisposable
    {
      
        #region Events




        public event EventHandler<RunWorkerCompletedEventArgs> TaskCompleted;
        public event ProgressChangedEventHandler ProgressChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        protected BackgroundWorker worker;
        protected Object workerRunning= new object(); // Locked while the worker is running

        protected MessageLog log;
   
        #region Properties
        public IList<SequentialTask> DependantTasks { get; protected set; } // Must be run after this task
        public SequentialTask RequisiteTask { get; protected set; } // Must be run before this task
        public string Description { get; set; }
        bool selected;
        public bool Selected { get => selected; set { selected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected))); } }
        public MessageLog Log { get => log; }
        public MessageLog.Severity LogSeverity { get => log.HighestSeverity; }
        public bool IsRunning { get => worker != null; }
        public bool IsCompleted { get; protected set; }
        #endregion

        public SequentialTask()
        {
            DependantTasks = new List<SequentialTask>();
            Description = "<none>";
            Selected = true;
            IsCompleted = false;
            log = new MessageLog();
            log.PropertyChanged += Log_PropertyChanged;  
        }

        private void Log_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           if (e.PropertyName == nameof(MessageLog.HighestSeverity))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogSeverity)));
            }
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
            lock (workerRunning)
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

        public void LogMessage(MessageLog.Severity severity, string message)
        {
            log.LogMessage(severity, message);
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
                lock(workerRunning) // Wait for task to end 
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Cancel();
                    worker.Dispose();
                    worker = null;
                }

                

                disposedValue = true;
            }
        }


        ~SequentialTask() {
           // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
          Dispose(false);
         }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
