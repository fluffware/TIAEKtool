using System;
using System.Collections.Generic;
using TIAEKtool.Plc;

namespace TIAEKtool
{


    public class PresetTag : IComparable
    {
        public PathComponent readTagPath;  // Tag that represents the current value for this preset.
        public PathComponent writeTagPath;  // Tag to write when setting this preset. Usually the same as readTagPath.
        public String[] presetGroups; // Groups that this preset belongs to
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
