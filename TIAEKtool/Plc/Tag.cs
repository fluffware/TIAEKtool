using Siemens.Engineering.SW.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool.Plc
{
    public class Tag
    {
        public PathComponent Path { get; }
        public MultilingualText Comment = null;
        public object StartValue = null;

        public Tag(PathComponent path)
        {
            Path = path;
        }

       
    }

   
}
