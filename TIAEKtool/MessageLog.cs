using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public class MessageLog :IList<MessageLog.Entry>
    {
        public enum Severity
        {
            None = 0,
            Debug,
            Info,
            Warning,
            Error
        }

        public class Entry
        {
            public Severity Severity { get; protected set; }
            public string Message { get; protected set; }
            public Entry(Severity severity, string message)
            {
                Severity = severity;
                Message = message;
            }
        }
        protected List<Entry> log = new List<Entry>();

        public event PropertyChangedEventHandler PropertyChanged;
        public Severity HighestSeverity {get; protected set;}

        public int Count => log.Count();

        public bool IsReadOnly => true;

        public Entry this[int index] { get => log[index]; set => throw new NotSupportedException();}

        public MessageLog()
        {
            HighestSeverity = Severity.None;
        }

        public void Clear()
        {
            HighestSeverity = Severity.None;
            log.Clear();
        }

        public void LogMessage(Severity severity, string message)
        {
            log.Add(new Entry(severity, message));
            if (severity > HighestSeverity)
            {
                HighestSeverity = severity;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighestSeverity)));
        }

        public int IndexOf(Entry item)
        {
            return IndexOf(item);
        }

        public void Insert(int index, Entry item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void Add(Entry item)
        {
            throw new NotSupportedException();
        }

        public bool Contains(Entry item)
        {
            return log.Contains(item);
        }

        public void CopyTo(Entry[] array, int arrayIndex)
        {
            log.CopyTo(array, arrayIndex);
        }

        public bool Remove(Entry item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            return log.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return log.GetEnumerator();
        }

        public IList<Entry> List { get =>log; }
    }
}
