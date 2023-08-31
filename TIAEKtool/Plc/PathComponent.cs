using TIAEktool.Plc.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TIAEKtool.Plc
{
    public abstract class PathComponent :IComparable<PathComponent>, IEquatable<PathComponent>

    {
        public PathComponent Parent { get; protected set; }

        private DataType _type;
        public DataType Type { get { return _type; } set { Debug.Assert(_type == null);  _type = value; } }

        public PathComponent(DataType type, PathComponent parent = null)
        {

            _type = type;
            Parent = parent;
        }

        public abstract PathComponent CloneComponent();

        public PathComponent ClonePath()
        {
            PathComponent clone = CloneComponent();
            if (Parent != null)
            {
                clone.Parent = Parent.ClonePath();
            }
            return clone;
        }
        public PathComponent PrependPath(PathComponent prefix)
        {

            PathComponent clone = CloneComponent();
            if (Parent != null)
            {
                clone.Parent = Parent.PrependPath(prefix);
            }
            else
            {
                clone.Parent = prefix;
            }
            return clone;
        }

        public static PathComponent operator + (PathComponent a, PathComponent b) {
            return b.CloneComponent().PrependPath(a.CloneComponent());
        }

        public int PathLength()
        {
            int length = 0;
            PathComponent p = this;
            while (p != null)
            {
                p = p.Parent;
                length++;
            }
            return length;
        }
      
        public abstract int CompareComponent(PathComponent other);

        // The paths need to be of same length
        private static int ComparePaths(PathComponent a, PathComponent b)
        {
            Debug.Assert(a != null && b != null);
            int cmp;
            if (a.Parent != null) cmp = ComparePaths(a.Parent, b.Parent);
            else
            {
                Debug.Assert(b.Parent == null);
                cmp = 0;
            }
            if (cmp != 0) return cmp;
            return a.CompareComponent(b);

        }
        /// <summary>
        /// Make both paths have the same length by shortening the longest one
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Length of a subtracted by length of b</returns>
        public static int MakeEqualLength(ref PathComponent a, ref PathComponent b)
        {
            int al = a.PathLength();
            int bl = b.PathLength();
            if (al > bl)
            {
                int l = al;
                do { a = a.Parent; l--; } while (l > bl);
            }
            else if (al < bl)
            {
                int l = bl;
                do { b = b.Parent; l--; } while (l > al);
            }
            return al - bl;
        }
        public int CompareTo(PathComponent b)
        {
            PathComponent a = this;
            int diff = MakeEqualLength(ref a, ref b);
            int cmp = ComparePaths(a, b);
            if (cmp == 0) return diff;
            else return cmp;
        }
        public abstract string ToHmiTagName();

        public bool Equals(PathComponent other)
        {
            return CompareTo(other) == 0;
        }

        private static int PrefixLength(PathComponent a, PathComponent b)
        {

            int length = 1;
            int shortest = 0;
            while (a.Parent != null)  {
                if (a.CompareComponent(b) != 0)
                {
                    shortest = length;
                }
                a = a.Parent;
                b = b.Parent;
                length++;
            }
            return shortest;

        }
        /// <summary>
        /// Returns how many components needs to be removed from the end of the path to match the start of other.
        /// </summary>
        /// <param name="other">Path to match against</param>
        /// <returns>Path components that needs to be remove to match the start of the other path.</returns>
        public int MatchPrefix(PathComponent other)
        {
            var a = this;
            var b = other;
            int diff = MakeEqualLength(ref a, ref b);

            int length = PrefixLength(a,b);
            return length + ((diff > 0) ? diff : 0);
        }
    }


    public class MemberComponent : PathComponent
    {
        public string Name { get; protected set; }


        public MemberComponent(string name, DataType type, PathComponent parent = null) : base(type, parent)
        {
            Name = name;
        }

        static public readonly char[] ESCAPED_CHARS = { '.', '[', ']', '"', ' ' };
        static private string EscapeName(string str)
        {
            if (str.IndexOfAny(ESCAPED_CHARS) >= 0)
            {
                return '"' + str + '"';
            }
            else
            {
                return str;
            }
        }
        public override string ToString()
        {
            return ((Parent != null) ? Parent.ToString() + "." : "") + EscapeName(Name);
        }
        public override string ToHmiTagName()
        {
            return ((Parent != null) ? Parent.ToString() + "_" : "") + Name;
        }

        public override PathComponent CloneComponent()
        {
            return new MemberComponent(Name, Type, Parent);
        }

        public override int CompareComponent(PathComponent other)
        {
            if (other is MemberComponent other_member)
            {
                return Name.CompareTo(other_member.Name);
            }
            return 1; // Members are greater than indices
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + ((Parent != null) ? Parent.GetHashCode() : 0);
        }
    }

    public class IndexComponent : PathComponent
    {
        private int[] _indices;
        public int[] Indices { get
            {
                if (_indices != null) return _indices;
                var limits = (Parent.Type as ARRAY).Limits;
                int[] indices = new int[limits.Count];
                for (int i = 0; i < indices.Length; i++)
                {
                    Constant low = limits[i].LowLimit;
                    if (low is IntegerLiteral literal)
                    {
                        int low_limit = literal.Value;
                        indices[i] = low_limit;
                    }
                    else
                    {
                        throw new Exception("Low limit of array is not an integer constant.");
                    }
                }
                return indices;
            }
            protected set
            {
                _indices = value;
            }
        }

        public IndexComponent(int[] indices, PathComponent parent) : base((parent.Type as ARRAY).MemberType, parent)
        {
            _indices = (int[])indices?.Clone();
        }
        protected IndexComponent(int[] indices, DataType type, PathComponent parent) : base(type, parent)
        {
            _indices = (int[])indices?.Clone();
        }
        public IndexComponent(PathComponent parent) : base((parent.Type as ARRAY).MemberType, parent)
        {
            _indices = null;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder(Parent.ToString());
            str.Append("[");
            if (_indices == null)
            {
                if (Parent.Type is ARRAY array)
                {
                    int l = array.Limits.Count;
                    if (l >= 1)
                    {
                        str.Append("*");
                        for (int i = 1; i < l; i++)
                        {
                            str.Append(",*");
                        }
                    }
                }
            }
            else
            {
                if (_indices.Length >= 1)
                {
                    str.Append(Indices[0]);
                    for (int i = 1; i < Indices.Length; i++)
                    {
                        str.Append(",");
                        str.Append(Indices[i]);
                    }
                }
            }
            str.Append("]");
            return str.ToString();
        }
        public override string ToHmiTagName()
        {
            StringBuilder str = new StringBuilder(Parent.ToString());
            str.Append("{");
            if (_indices != null)
            {
                if (Indices.Length >= 1)
                {
                    str.Append(Indices[0]);
                    for (int i = 1; i < Indices.Length; i++)
                    {
                        str.Append(",");
                        str.Append(Indices[i]);
                    }
                }
            }
            str.Append("}");
            return str.ToString();
        }
        public override PathComponent CloneComponent()
        {
            return new IndexComponent(Indices, Type, Parent);
        }

        public override int CompareComponent(PathComponent other)
        {
            if (other is IndexComponent other_index)
            {
                foreach (var c in Enumerable.Zip(Indices, other_index.Indices, (a, b) => a.CompareTo(b)))
                {
                    if (c != 0)
                    {
                        return c;
                    }
                }
                return Indices.Count() - other_index.Indices.Count();
            }
            return -1; // Members are greater than indices
        }

        public override int GetHashCode()
        {
            int hash = 0;
            if (_indices != null) hash += Indices.Aggregate(0, (int a, int b) => a + b);
            if (Parent != null) hash += Parent.GetHashCode();
            return hash;
        }
    }

}
