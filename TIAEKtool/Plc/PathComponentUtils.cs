using TIAEktool.Plc.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TIAEKtool.Plc
{
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

            if (path is IndexComponent index)
            {
               

                int[] indices = new int[index.Indices.Count()];
                for (int i = 0; i < indices.Count(); i++)
                {
                    indices[i] = ((IntegerLiteral)((ARRAY)parent.Type).Limits[i].LowLimit).Value;
                }

                path = new IndexComponent(indices, parent);
            }
            else
            {
                MemberComponent member = (MemberComponent)path;
                DataType type = member.Type;
                
                if (path.Type is ARRAY array_type)
                {
                    ARRAY new_array_type = new ARRAY()
                    {
                        MemberType = array_type.MemberType,
                        Limits = new ArrayLimits[array_type.Limits.Count]
                    };
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
            if (path is IndexComponent index)
            {

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
                if  (path != null && path.Type == null)
                {
                    path.Type = new STRUCT(); // A MemberComponent always has a struct as parent
                }
                path = new MemberComponent(name, null, path);
                if (pos == str.Length) break;
                if (str[pos] == '[') {
                    if (pos + 1 >= str.Length) throw new ParseException("Path ends with '['");
                    int end = str.IndexOf(']', pos + 1);
                    if (end == -1) throw new ParseException("No terminating ']'");
                    string indices = str.Substring(pos + 1, end - pos - 1);
                    string[] index_str = indices.Split(',');
                    if (path.Type == null)
                    {
                        path.Type = new ARRAY();// A IndexComponent always has an array as parent
                    }
                    path = new IndexComponent(index_str.Select(s => int.Parse(s)).ToArray<int>(), path);
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

        // Substitute all indices in path with the low limit of the corresponding array
        public static PathComponent SubstituteIndicesLow(PathComponent path)
        {
            PathComponent parent_copy;
            if (path.Parent != null)
            {
                parent_copy = SubstituteIndicesLow(path.Parent);
            }
            else
            {
                parent_copy = null;
            }

            if (path is IndexComponent ic)
            {
                int[] indices = new int[ic.Indices.Length];
                if (!(ic.Parent is MemberComponent && ic.Parent.Type is ARRAY)) throw new Exception("Parent of index component is not an array");
                ARRAY array_type = (ARRAY)ic.Parent.Type;
                for (int l = 0; l < array_type.Limits.Count; l++)
                {
                    Constant low = array_type.Limits[l].LowLimit;
                    if (!(low is IntegerLiteral)) throw new Exception("Low limit of array is not an integer constant.");
                    int low_limit = ((IntegerLiteral)low).Value;
                    indices[l] = low_limit;

                }

                return new IndexComponent(indices, parent_copy);
            }
            else
            {
                MemberComponent member = (MemberComponent)path;
                return new MemberComponent(member.Name, member.Type, parent_copy);
            }
        }


        /// <summary>
        /// Makes a copy of the path with the indices substituted
        /// </summary>
        /// <param name="path">Original path</param>
        /// <param name="substituted">Copy of path with new indices</param>
        /// <param name="indices">Indices to substitute</param>
        /// <returns>Number of indices in path</returns>
        public static int SubstituteIndices(PathComponent path, out PathComponent substituted, IEnumerator<int> indices)
        {
            PathComponent parent_copy;
            int subs_count;
            if (path.Parent != null)
            {
                subs_count = SubstituteIndices(path.Parent, out parent_copy, indices);

            }
            else
            {
                parent_copy = null;
                subs_count = 0;
            }

            if (path is IndexComponent ic)
            {
                IndexComponent copy = new IndexComponent(new int[ic.Indices.Length], parent_copy);
                for (int i = 0; i < ic.Indices.Length; i++)
                {
                    if (!indices.MoveNext()) break;
                    copy.Indices[i] = indices.Current;

                }
                subs_count += ic.Indices.Length;
                substituted = copy;
                return subs_count;
            }
            else
            {
                MemberComponent member = (MemberComponent)path;
                substituted = new MemberComponent(member.Name, member.Type, parent_copy);
                return subs_count;
            }
        }


    }

}
