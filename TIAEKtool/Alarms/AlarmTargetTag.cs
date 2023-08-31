using TIAEKtool.Plc;

namespace TIAEKtool
{
    public class AlarmTargetTag: AlarmTarget
    {
        public PathComponent plcTag = null;
        public AlarmTargetTag(string name, string label) : base(name,label)
        {
            
        }
    }
}
