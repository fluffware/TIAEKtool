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
   
    public class AlarmTagList : BindingList<AlarmTagList.Row>
    {
        // Maps AlarmTag to a row with a single culture
        public class Row: IComparable<Row>
        {
            private readonly StringBuilder _culture;  // Common for all rows
            private readonly AlarmTag _tag;

            protected static string PathType(PathComponent path)
            {
                DataType type = path.Type;
                if (type == null) return "Unknown";
                else return type.ToDebug();
            }

            public int CompareTo(Row other)
            {
                return _tag.priority.CompareTo(other._tag.priority);
            }

            public string Culture { get => _culture.ToString(); }
            public string PlcTag { get => _tag.plcTag.ToString(); }
            public string Id { get => _tag.id.ToString(); set { _tag.id = int.Parse(value); } }
            public string Sinks { get => string.Join(",", _tag.sinks); 
                set {foreach (string sink in value.Split(','))
                    { _tag.sinks.Add(sink); } 
                } }
            public string AlarmClass { get => _tag.alarmClass; set { _tag.alarmClass = value; } }
            public string Priority { get => _tag.priority.ToString(); set { _tag.priority = int.Parse(value); } }
            public string Delay { get => _tag.delay.ToString(); set { _tag.delay = int.Parse(value); } }
            public string Edge { get => _tag.edge.ToString(); set { _tag.edge = (value.ToLower() == "falling") ? AlarmTag.Edge.Falling : AlarmTag.Edge.Rising; } }
            public string AlarmText { get => _tag.eventText[_culture.ToString()]; set { _tag.eventText[_culture.ToString()] = value; } }

            public AlarmTag AlarmTag { get => _tag; }
            public Row(StringBuilder culture, AlarmTag tag)
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

        public AlarmTagList() : base(new List<AlarmTagList.Row>())
        {
        }
        
        public void AddTag(AlarmTag tag)
        {
            tag.InitDefaults();
            Add(new Row(_culture, tag));
        }

        
        protected override bool SupportsSortingCore => true;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            RaiseListChangedEvents = false;
            List<AlarmTagList.Row> itemsList = (List<AlarmTagList.Row>)this.Items;
            if (prop.PropertyType.GetInterface("IComparable") != null)
            {
                itemsList.Sort(new Comparison<AlarmTagList.Row>(delegate (AlarmTagList.Row x, AlarmTagList.Row y)
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
