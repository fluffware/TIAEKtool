using System;
using System.Collections.Generic;

namespace TIAEKtool
{


    public class AlarmTag : IComparable
    {
        public enum Edge { 
            Rising,
            Falling
        }
        public PathComponent plcTag = null;  // Tag that represents the current state for this alarm.
        public string hmiTag = null;
        public int id;
        public MultilingualText eventText = null;
        public string alarmClass = null;
        public HashSet<String> sinks = null; // Where to handle the alarm (HMIs or tags)
        public int priority = 12;
        public int delay = 0; // Delay in ms
        public Edge edge = Edge.Rising; 
        public int CompareTo(object obj)
        {
            return plcTag.CompareTo(((AlarmTag)obj).plcTag);
        }

        public void InitDefaults()
        {
            if (eventText == null)
            {
                eventText = new MultilingualText();
            }
            if (alarmClass == null)
            {
                alarmClass = "Alarm";
            }
            if (sinks == null)
            {
                sinks = new HashSet<string>();
            }
           
        }
    }


}
