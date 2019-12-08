using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    class MessageTask :SequentialTask
    {

        public MessageTask(string description, MessageLog.Severity severity,  string message)
        {
            Description = description;
            LogMessage(severity, message);
        }

        protected override void DoWork()
        {
        }
    }
}
