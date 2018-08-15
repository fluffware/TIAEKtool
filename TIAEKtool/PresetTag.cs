using PLC.Types;
using System;
using System.Collections.Generic;

namespace TIAEKtool
{
    

    public class PresetTag
    {
        public PathComponent tagPath;
        public String presetGroup;
        public MultilingualText labels;
        public string defaultValue;
        public bool noStore;
        public string unit;
        public int precision = 0; // Number of digits after decimal point
        public IDictionary<int,MultilingualText> state_labels;
    }
}
