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
   
    public class AlarmSinkList : BindingList<AlarmSinkList.Row>
    {
        // Maps AlarmTag to a row with a single culture
        public class Row : IComparable<Row>
        {
            private readonly AlarmSink _sink;

            protected static string PathType(PathComponent path)
            {
                DataType type = path.Type;
                if (type == null) return "Unknown";
                else return type.ToDebug();
            }

            public int CompareTo(Row other)
            {
                return _sink.name.CompareTo(other._sink.name);
            }

            public string PlcTag { get => (_sink is AlarmSinkTag sink) ? sink.plcTag.ToString() : ""; }
            public string Name { get => _sink.name; set { _sink.name = value; } }
            public string Label { get => _sink.label; set { _sink.label = value; } }
            

            public AlarmSink AlarmSink { get => _sink; }
            public Row(AlarmSink sink)
            {
                _sink = sink;
            }
        }

        protected override  bool IsSortedCore { get => sortProperty != null; }
        protected PropertyDescriptor sortProperty = null;
        protected override PropertyDescriptor SortPropertyCore { get => sortProperty; }
        protected ListSortDirection sortDirection;
        protected override ListSortDirection SortDirectionCore { get => sortDirection; } 
     
      

        public AlarmSinkList() : base(new List<AlarmSinkList.Row>())
        {
        }
        
        public void AddSink(AlarmSink sink)
        {
            Add(new Row(sink));
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
