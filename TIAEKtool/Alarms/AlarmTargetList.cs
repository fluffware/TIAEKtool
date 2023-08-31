using System;
using System.Collections.Generic;
using System.ComponentModel;
using TIAEktool.Plc.Types;
using TIAEKtool.Plc;

namespace TIAEKtool
{

    public class AlarmTargetList : BindingList<AlarmTargetList.Row>
    {
        // Maps AlarmTag to a row with a single culture
        public class Row : IComparable<Row>
        {
            private readonly AlarmTarget _target;

            protected static string PathType(PathComponent path)
            {
                DataType type = path.Type;
                if (type == null) return "Unknown";
                else return type.ToDebug();
            }

            public int CompareTo(Row other)
            {
                return _target.name.CompareTo(other._target.name);
            }

            public string PlcTag { get => (_target is AlarmTargetTag sink) ? sink.plcTag.ToString() : ""; }
            public string Name { get => _target.name; set { _target.name = value; } }
            public string Label { get => _target.label; set { _target.label = value; } }
            

            public AlarmTarget AlarmSink { get => _target; }
            public Row(AlarmTarget sink)
            {
                _target = sink;
            }
        }

        protected override  bool IsSortedCore { get => sortProperty != null; }
        protected PropertyDescriptor sortProperty = null;
        protected override PropertyDescriptor SortPropertyCore { get => sortProperty; }
        protected ListSortDirection sortDirection;
        protected override ListSortDirection SortDirectionCore { get => sortDirection; } 
     
      

        public AlarmTargetList() : base(new List<AlarmTargetList.Row>())
        {
        }
        
        public void AddSink(AlarmTarget sink)
        {
            Add(new Row(sink));
        }

        
        protected override bool SupportsSortingCore => true;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            RaiseListChangedEvents = false;
            List<AlarmTargetList.Row> itemsList = (List<AlarmTargetList.Row>)this.Items;
            if (prop.PropertyType.GetInterface("IComparable") != null)
            {
                itemsList.Sort(new Comparison<AlarmTargetList.Row>(delegate (Row x, Row y)
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
