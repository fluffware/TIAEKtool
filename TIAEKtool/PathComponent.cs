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

        public DataType Type { get; protected set; }

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
    }

    public class MemberComponent : PathComponent
    {
        public string Name { get; protected set; }


        public MemberComponent(string name, DataType type, PathComponent parent = null) : base(type, parent)
        {
            Name = name;
        }

        public override string ToString()
        {
            return ((Parent != null) ? Parent.ToString() + "." : "") + Name;
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
            Indices = indices;
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

        private static int IntegerConstantLookup(ConstantLookup constants, string name)
        {
            ConstantLookup.Entry entry = constants.Lookup(name);
            if (entry == null) new Exception("Failed to lookup constant " + name);
            int value;
            if (!int.TryParse(entry.value, out value)) new Exception("Constant " + name + " doas not have an integer value");
            return value;
        }
        private static IntegerLiteral ResolveIntConstant(ConstantLookup lookup, Constant constant)
        {
            if (constant is IntegerLiteral) return (IntegerLiteral)constant;
            if (!(constant is GlobalConstant)) throw new Exception("Can only resolve global constants");
            return new IntegerLiteral(IntegerConstantLookup(lookup, ((GlobalConstant)constant).Name));
        }

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
                            ResolveIntConstant(constants, array_type.Limits[i].LowLimit),
                            ResolveIntConstant(constants, array_type.Limits[i].HighLimit));
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

    }

}
