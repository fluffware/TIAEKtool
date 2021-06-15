using PLC.Types;
using System;
using System.Collections.Generic;

namespace TIAEKtool
{
    

    public class PresetTag : IComparable
    {
        public PathComponent tagPath;
        public String[] presetGroups;
        public MultilingualText labels;
        public string defaultValue;
        public bool noStore;
        public string unit;
        public int precision = 0; // Number of digits after decimal point
        public float? min = null;  // Min numeric value, inclusive
        public float? max = null;   // Max numeric value, inclusive
        public int order = 10000;   // Lower values are presented to the user before higher values
        public IDictionary<int,MultilingualText> state_labels;

        public int CompareTo(object obj)
        {
            return order.CompareTo(((PresetTag)obj).order);
        }
    }


}
