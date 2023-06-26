using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public class AlarmSinkTag: AlarmSink
    {
        public PathComponent plcTag = null;
        public AlarmSinkTag(string name, string label) : base(name,label)
        {
            
        }
    }
}
