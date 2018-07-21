using PLC.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TIAEKtool
{
   
    public class PresetTagList : BindingList<PresetTagList.Row>
    {
        // Maps PresetTag to a row with a single culture
        public class Row 
        {
            private StringBuilder _culture;  // Common for all rows
            private PresetTag _tag;

            protected static string PathType(PathComponent path)
            {
                DataType type = path.Type;
                if (type == null) return "Unknown";
                else return type.ToDebug();
            }

            public string Culture { get => _culture.ToString(); }
            public string Path { get => _tag.tagPath.ToString(); }
            public string Group { get => _tag.presetGroup; set => _tag.presetGroup = value; }
            public string Label { get => (_tag.labels != null) ? _tag.labels[_culture.ToString()] : ""; set { if (_tag.labels != null) _tag.labels[_culture.ToString()] = value; } }
            public string Type { get => PathType(_tag.tagPath); }
            public PresetTag Tag { get => _tag; }
            public Row(StringBuilder culture, PresetTag tag)
            {
                _culture = culture;
                _tag = tag;
            }
        }


        protected StringBuilder _culture = new StringBuilder();  // Common for all rows
        public string Culture { get => _culture.ToString(); set { _culture.Clear(); _culture.Append(value); } }

        public PresetTagList() : base()
        {
        }

        public void AddTag(PresetTag tag)
        {
            Add(new Row(_culture, tag));
        }
    }
}
