using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public abstract class AlarmSink
    {
        public string name;
        public string label;

        public AlarmSink(string name, string label)
        {
            this.name = name;
            this.label = label;
        }
    }
}
