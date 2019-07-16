using Siemens.Engineering;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Tags;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public class ConstantLookup: IEnumerable<System.Collections.Generic.KeyValuePair<string, ConstantLookup.Entry>>
    {

        public class Entry
        {
            public string type;
            public string value;
            public Entry(string type, string value)
            {
                this.type = type;
                this.value = value;
            }
        }
        protected Dictionary<string, Entry> lookup = new Dictionary<string, Entry>();
        public ConstantLookup()
        {

        }

        protected void Populate(PlcTagTableGroup group)
        {
            foreach (PlcTagTableGroup g in group.Groups) {
                Populate(g);
            }
            foreach (PlcTagTable t in group.TagTables)
            {
                foreach (PlcUserConstant uc in t.UserConstants)
                {
                    lookup.Add(uc.Name, new Entry(uc.DataTypeName, uc.Value));
                }
            }
        }
        public void Populate(TiaPortal portal, PlcSoftware plc)
        {
            lock (portal)
            {
                Populate(plc.TagTableGroup);
            }
        }

        public Entry Lookup(string name)
        {
            Entry entry;
            if (lookup.TryGetValue(name, out entry))
            {
                return entry;
            }
            else
            {
                return null;
            }
        }

        public int IntegerLookup(string name)
        {
            ConstantLookup.Entry entry = Lookup(name);
            if (entry == null) new KeyNotFoundException("Failed to lookup constant " + name);
            int value = int.Parse(entry.value);
            return value;
        }



        IEnumerator<KeyValuePair<string, Entry>> IEnumerable<KeyValuePair<string, Entry>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected IEnumerator<KeyValuePair<string, Entry>> GetEnumerator()
        {
            return lookup.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
    }
}
