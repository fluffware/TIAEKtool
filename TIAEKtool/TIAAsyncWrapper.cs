using Siemens.Engineering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TIAEKtool
{

    /* Execute all TIA functions from a singel thread */
    public class TIAAsyncWrapper: IDisposable
    {



        public abstract class Task: IDisposable
        {
            public SynchronizationContext caller_context;
            private void ResultMarshal(object obj)
            {
                Result(obj);
            }

            public void CaughtExceptionMarshal(object obj)
            {
                CaughtException((Exception)obj);
            }

            public void DoneMarshal(object obj)
            {
                Done(obj);
            }

            // Task.Run should return as soon as possible when this is set
            public bool cancelled;

            // Called in dedicated thread
            public abstract object Run();

            // Called in the SynchronizationContext that called TIAAsyncWrapper.Run
            public virtual void Done(object result)
            {
            }

            public virtual void CaughtException(Exception ex)
            {
            }

            // Called in the SynchronizationContext that called TIAAsyncWrapper.Run as a result of calling SendResult
            public virtual void Result(object result)
            {
            }

            // Called from Task.Run
            public void SendResult(object result)
            {
                caller_context.Post(ResultMarshal, result);
            }

            #region IDisposable Support
          
            protected virtual void Dispose(bool disposing)
            {
            }

            ~Task()
            {
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

        Task task = null;

        protected bool exiting = false;
        AutoResetEvent next_operation = new AutoResetEvent(true);
        AutoResetEvent start_operation = new AutoResetEvent(false);

        Thread thread;
        public TIAAsyncWrapper()
        {
            thread = new Thread(new ThreadStart(TIAMain));
            thread.Start();
        }

        ~TIAAsyncWrapper()
        {
            //Console.WriteLine("~TIAAsyncWrapper");
            Dispose(false);
        }


        public void cancel()
        {
            if (task != null)
            {
                task.cancelled = true;
            }
        }

        public void Stop()
        {
            if (!exiting)
            {
                cancel();
                exiting = true;
                start_operation.Set();
                next_operation.Set();
                thread.Join();
                thread = null;
            }
        }


        public void TIAMain()
        {

            while (true)
            {
                start_operation.WaitOne();
                if (exiting) return;
                try
                {
                    object res = task.Run();
                    task.caller_context.Post(task.DoneMarshal, res);
                }
                catch (Exception ex)
                {
                    task.caller_context.Post(task.CaughtExceptionMarshal, ex);
                }
                next_operation.Set();
                task = null;
            }
        }

        [Serializable]
        public class ThreadExitingException : Exception
        {
            public ThreadExitingException() : base("Trying to run a task on a thread that is shuttingdown")
            {

            }
        }

        public void RunAsync(Task task)
        {

            task.cancelled = false;
            task.caller_context = SynchronizationContext.Current;
            next_operation.WaitOne();
            this.task = task;
            if (exiting)
            {
                throw new ThreadExitingException();
            }

            start_operation.Set();
        }

        public delegate object SyncOp(object state);
        class SyncTask : Task, IDisposable
        {
            public SyncOp cb;
            public object state;
            public object result = null;
            public Exception ex = null;
            public AutoResetEvent done = new AutoResetEvent(false);
            public override object Run()
            {
                return cb(state);
            }

            public override void Done(object res)
            {
                result = res;
                done.Set();
            }

            public override void CaughtException(Exception ex)
            {
                this.ex = ex;
                done.Set();
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            override protected void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        done.Dispose();
                    }
                    disposedValue = true;
                }
                base.Dispose(disposing);
            }


            ~SyncTask()
            {
                Dispose(false);
            }


            #endregion
        }

        public object RunSync(SyncOp cb, object state)
        {
            SyncTask task = new SyncTask();
            task.cb = cb;
            task.state = state;
            task.caller_context = new SynchronizationContext();

            next_operation.WaitOne();
            this.task = task;
            if (exiting)
            {
                throw new ThreadExitingException();
            }

            start_operation.Set();

            task.done.WaitOne();
            if (task.ex != null)
            {
                throw task.ex;
            }
            return task.result;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                    task.Dispose();
                    next_operation.Dispose();
                    start_operation.Dispose();
                }

                disposedValue = true;
            }
        }

       
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
