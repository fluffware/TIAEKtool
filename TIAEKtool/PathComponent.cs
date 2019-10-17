using PLC.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public abstract class PathComponent

    {
        public PathComponent Parent { get; protected set; }

        public DataType Type { get; set; }

        public PathComponent(DataType type, PathComponent parent = null)
        {

            Type = type;
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
        public override PathComponent CloneComponent()
        {
            return new MemberComponent(Name, Type, Parent);
        }
    }

    public class IndexComponent : PathComponent
    {
        public int[] Indices { get; protected set; }

        public IndexComponent(int[] indices, DataType type, PathComponent parent = null) : base(type, parent)
        {
            Indices = (int[])indices.Clone();
        }


        public override string ToString()
        {
            StringBuilder str = new StringBuilder(Parent.ToString());
            str.Append("[");
            if (Indices.Length >= 1)
            {
                str.Append(Indices[0]);
                for (int i = 1; i < Indices.Length; i++)
                {
                    str.Append(",");
                    str.Append(Indices[i]);
                }
            }
            str.Append("]");
            return str.ToString();
        }

        public override PathComponent CloneComponent()
        {
            return new IndexComponent(Indices, Type, Parent);
        }
    }
    public class PathComponentUtils
    {

       

        public static PathComponent InitializeArrayPath(PathComponent path, ConstantLookup constants)
        {
            PathComponent parent;
            if (path.Parent != null)
            {
                parent = InitializeArrayPath(path.Parent, constants);
            }
            else
            {
                parent = null;
            }

            if (path is IndexComponent)
            {
                IndexComponent index = (IndexComponent)path;

                int[] indices = new int[index.Indices.Count()];
                for (int i = 0; i < indices.Count(); i++)
                {
                    indices[i] = ((IntegerLiteral)((ARRAY)parent.Type).Limits[i].LowLimit).Value;
                }

                path = new IndexComponent(indices, index.Type, parent);
            }
            else
            {
                MemberComponent member = (MemberComponent)path;
                DataType type = member.Type;
                ARRAY array_type = path.Type as ARRAY;
                if (array_type != null)
                {
                    ARRAY new_array_type = new ARRAY();
                    new_array_type.MemberType = array_type.MemberType;
                    new_array_type.Limits = new ArrayLimits[array_type.Limits.Count];
                    for (int i = 0; i < new_array_type.Limits.Count(); i++)
                    {
                        new_array_type.Limits[i] = new ArrayLimits(
                            new IntegerLiteral(array_type.Limits[i].LowLimit.ResolveInt(constants)),
                            new IntegerLiteral(array_type.Limits[i].HighLimit.ResolveInt(constants)));
                    }
                    type = new_array_type;
                }
                path = new MemberComponent(member.Name, type, parent);
            }


            return path;
        }

        public static bool NextArrayPath(PathComponent path)
        {
            if (path is IndexComponent)
            {
                IndexComponent index = (IndexComponent)path;
                IList<ArrayLimits> limits = ((ARRAY)path.Parent.Type).Limits;
                for (int i = 0; i < index.Indices.Count(); i++)
                {
                    if (index.Indices[i] < ((IntegerLiteral)limits[i].HighLimit).Value)
                    {
                        index.Indices[i]++;
                        return true;
                    }
                    else
                    {
                        index.Indices[i] = ((IntegerLiteral)limits[i].LowLimit).Value;
                    }
                }
            }
            if (path.Parent == null) return false;
            return NextArrayPath(path.Parent);
        }

        static readonly char[] special_chars = { '.', '[', '"' };
        public class ParseException : Exception
        {
            public ParseException(string msg) : base(msg)
            {
                
            }

        }

        public static PathComponent ParsePath(string str)
        {
            int pos = 0;
            PathComponent path = null;
            string name;
            while (pos < str.Length) {
                if (str[pos] == '"')
                {
                    if (pos + 1 >= str.Length) throw new ParseException("Path ends with '\"'");

                    int end = str.IndexOf('"', pos + 1);
                    if (end == -1) throw new ParseException("No terminating '\"'");
                    name = str.Substring(pos + 1, end - pos - 1);
                    pos = end + 1;
                }
                else
                {
                    int start = pos;
                    pos = str.IndexOfAny(MemberComponent.ESCAPED_CHARS, pos);
                    if (pos == -1)
                    {
                        pos = str.Length;
                    }
                    name = str.Substring(start, pos - start);
                }
                path = new MemberComponent(name, null, path);
                if (pos == str.Length) break;
                if (str[pos] == '[') {
                    if (pos + 1 >= str.Length) throw new ParseException("Path ends with '['");
                    int end = str.IndexOf(']', pos + 1);
                    if (end == -1) throw new ParseException("No terminating ']'");
                    string indices = str.Substring(pos + 1, end - pos - 1);
                    string[] index_str = indices.Split(',');
                    path = new IndexComponent(index_str.Select(s => int.Parse(s)).ToArray<int>(), null, path);
                    pos = end + 1;
                }
                if (pos == str.Length) break;
                if (str[pos] != '.') {
                    throw new ParseException("Expected '.'");
                }
                pos++;
            }
            return path;
        }

    }

}
