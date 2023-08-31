using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool.Plc
{
    public static class PlcUtils
    {
        public static  PlcBlock FindPlcBlockName(string name, PlcBlockGroup blocks)
        {
            PlcBlock block = blocks.Blocks.Find(name);
            if (block == null)
            {
                foreach (PlcBlockGroup group in blocks.Groups)
                {
                    block = FindPlcBlockName(name, group);
                    if (block != null) break;
                }
            }
            return block;
        }
        public static  PlcBlock FindPlcBlock(PathComponent path, PlcBlockGroup blocks)
        {
            while (path.Parent != null)
            {
                path = path.Parent;
            }
            if (!(path is MemberComponent)) return null;
            string name = ((MemberComponent)path).Name;
            Console.WriteLine("Name " + name);
            return FindPlcBlockName(name, blocks);
        }
    }
}
