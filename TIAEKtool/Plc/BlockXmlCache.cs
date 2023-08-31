using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TIAEKtool.Plc
{


    public class BlockXmlCache
    {
        class Entry
        {
            public PlcBlockGroup parent;
            public XmlDocument xml;
            public bool changed = false;
        }
        readonly PlcBlockGroup top_group;
        readonly Dictionary<string, Entry> cached;

        public BlockXmlCache(PlcBlockGroup top_group)
        {
            this.top_group = top_group;
            cached = new Dictionary<string, Entry>();
        }



        protected static PlcBlock PlcBlockRecursiveFind(PlcBlockGroup group, string name)
        {
            PlcBlock block = group.Blocks.Find(name);
            if (block != null) return block;
            foreach (PlcBlockGroup subgroup in group.Groups)
            {
                block = PlcBlockRecursiveFind(subgroup, name);
                if (block != null) return block;
            }
            return null;
        }
        public XmlDocument GetXML(string block_name)
        {
            if (cached.TryGetValue(block_name, out Entry found))
            {
                return found.xml;
            }
            PlcBlock block = PlcBlockRecursiveFind(top_group, block_name);
            XmlDocument xml = TIAutils.ExportPlcBlockXML(block);
            cached[block_name] = new Entry()
            {
                parent = block.Parent as PlcBlockGroup,
                xml = xml
            };
            return xml;
        }

        public void SetChanged(string block_name)
        {
            if (cached.TryGetValue(block_name, out Entry found))
            {
                found.changed = true;
            }
          
        }

        public bool HasChanged(string block_name)
        {
            if (cached.TryGetValue(block_name, out Entry found))
            {
                return found.changed;
            }
            return false;

        }
        public void WriteXML(string block_name, bool force = false)
        {
            if (cached.TryGetValue(block_name, out Entry entry))
            {
                if (force || entry.changed)
                {
                    TIAutils.ImportPlcBlockXML(entry.xml, entry.parent);
                }
            }
        }

        public void WriteAll(bool force = false)
        {
            foreach (Entry entry in cached.Values)
            {
                if (force || entry.changed)
                {
                    TIAutils.ImportPlcBlockXML(entry.xml, entry.parent);
                }
            }
        }
    }
}
