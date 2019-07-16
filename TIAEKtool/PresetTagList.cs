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
        public class Row: IComparable<Row>
        {
            private StringBuilder _culture;  // Common for all rows
            private PresetTag _tag;

            protected static string PathType(PathComponent path)
            {
                DataType type = path.Type;
                if (type == null) return "Unknown";
                else return type.ToDebug();
            }

            public int CompareTo(Row other)
            {
                return _tag.order.CompareTo(other._tag.order);
            }

            public string Culture { get => _culture.ToString(); }
            public string Path { get => _tag.tagPath.ToString(); }
            public string Group { get => _tag.presetGroup; set => _tag.presetGroup = value; }
            public string Label { get => (_tag.labels != null) ? _tag.labels[_culture.ToString()] : ""; set { if (_tag.labels != null) _tag.labels[_culture.ToString()] = value; } }
            public string Type { get => PathType(_tag.tagPath); }
            public string DefaultValue { get => _tag.defaultValue; }
            public bool NoStore { get => _tag.noStore; }
            public string Unit { get => _tag.unit; }
            public int Precision { get => _tag.precision; }
            public float Min { get => _tag.min; }
            public float Max { get => _tag.max; }
            public int Order { get => _tag.order; }
            public PresetTag Tag { get => _tag; }
            public Row(StringBuilder culture, PresetTag tag)
            {
                _culture = culture;
                _tag = tag;
            }
        }

        protected override  bool IsSortedCore { get => sortProperty != null; }
        protected PropertyDescriptor sortProperty = null;
        protected override PropertyDescriptor SortPropertyCore { get => sortProperty; }
        protected ListSortDirection sortDirection;
        protected override ListSortDirection SortDirectionCore { get => sortDirection; } 
        protected StringBuilder _culture = new StringBuilder();  // Common for all rows
        public string Culture { get => _culture.ToString(); set { _culture.Clear(); _culture.Append(value); ResetBindings(); } }

        public PresetTagList() : base(new List<PresetTagList.Row>())
        {
        }
        
        public void AddTag(PresetTag tag)
        {
            Add(new Row(_culture, tag));
        }

        
        protected override bool SupportsSortingCore => true;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            RaiseListChangedEvents = false;
            List<PresetTagList.Row> itemsList = (List<PresetTagList.Row>)this.Items;
            if (prop.PropertyType.GetInterface("IComparable") != null)
            {
                itemsList.Sort(new Comparison<PresetTagList.Row>(delegate (PresetTagList.Row x, PresetTagList.Row y)
                {
                    // Compare x to y if x is not null. If x is, but y isn't, we compare y
                    // to x and reverse the result. If both are null, they're equal.
                    if (prop.GetValue(x) != null)
                        return ((IComparable)prop.GetValue(x)).CompareTo(prop.GetValue(y)) * (direction == ListSortDirection.Descending ? -1 : 1);
                    else if (prop.GetValue(y) != null)
                        return ((IComparable)prop.GetValue(y)).CompareTo(prop.GetValue(x)) * (direction == ListSortDirection.Descending ? 1 : -1);
                    else
                        return 0;
                }));
            }
            sortProperty = prop;
            sortDirection = direction;
            RaiseListChangedEvents = true;
            ResetBindings();
        
        }

        protected override void RemoveSortCore()
        {
            sortProperty = null;
        }

       
    }
}
