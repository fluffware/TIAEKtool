using TIAEKtool.Plc;

namespace TIAEKtool
{
    public class AlarmTargetHmi: AlarmTarget
    {
        public string hmiName = null;
        public AlarmTargetHmi(string name, string label) : base(name,label)
        {
            
        }
    }
}
