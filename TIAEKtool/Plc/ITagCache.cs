using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool.Plc
{
    interface ITagCache
    {
        void Add(Tag tag);
        Tag Find(PathComponent path);

        IEnumerator<Tag> GetEnumerator(PathComponent prefix);
    }
}
