using PLC.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TIAEKtool
{
    class PresetValueParser
    {
       
        public MessageLog Log = null;

        
        protected static string ComponentToXPath(PathComponent path, ref List<int> indices)
        {
            string xpath = "";
            if (path.Parent != null)
            {
                xpath = ComponentToXPath(path.Parent, ref indices);
                if (xpath == null) return null;
            }
            if (path is MemberComponent member_path)
            {

                return xpath + "/if:Member[@Name='" + member_path.Name + "'] | /if:Sections/if:Section/if:Member[@Name='" + member_path.Name + "']";
             
            }
            else if (path is IndexComponent index_path)
            {
                indices.AddRange(index_path.Indices);
                return xpath;
            }
            else
            {
                return null;
            }
        }

        public class Limits
        {
            public int Low;
            public int High;
        }

        public static XmlElement GetPathElement(XmlElement tag_element, ref List<Limits> limits, PathComponent path, ConstantLookup constants)
        {

            XmlElement elem = tag_element;
            if (path.Parent != null)
            {
                elem = GetPathElement(tag_element,ref limits, path.Parent, constants);
                if (elem == null) return null;
            }
            if (path is MemberComponent member_path)
            {
                XmlElement child_elem = elem.SelectSingleNode("./if:Member[@Name='"+member_path.Name+ "'] | ./if:Sections/if:Section/if:Member[@Name='" + member_path.Name + "']", XMLUtil.nameSpaces) as XmlElement;
                if (child_elem == null)
                {
                    throw new Exception("Unable to find path " + member_path + " in element " + (elem?.GetAttribute("Name") ?? "<Unknown>"));
                }
                DataType type = DataTypeParser.Parse(child_elem.GetAttribute("Datatype"), out string _);
                if (type is ARRAY array)
                {
                    limits.AddRange(array.Limits.Select(x => new Limits() { Low = x.LowLimit.ResolveInt(constants), High = x.HighLimit.ResolveInt(constants) }));
                }
                return child_elem;
            }
            else if (path is IndexComponent index_path)
            {
                limits.RemoveRange(limits.Count - index_path.Indices.Count(), index_path.Indices.Count());
                limits.AddRange(index_path.Indices.Select(x => new Limits() { Low = x, High = x }));
                return elem;
            }
            else
            {
                return null;
            }

        }
        static void FillArrayDimension(Array a, Object value, ref int[] indices, int d)
        {
            if (d == a.Rank - 1)
            {
                for (int i = a.GetLowerBound(d); i <= a.GetUpperBound(d); i++)
                {
                    indices[d] = i;
                    a.SetValue(value, indices);
                }
            } else {
                for (int i = a.GetLowerBound(d); i <= a.GetUpperBound(d); i++)
                {
                    indices[d] = i;
                    FillArrayDimension(a, value, ref indices, d + 1);
                }
            }
        }

        public static void FillArray(Array a, Object value) {
            int[] indices = new int[a.Rank];
            FillArrayDimension(a, value, ref indices, 0);
        }

        public static void FlattenArrayDimension<T>(Array a, ref List<T> values, ref int[] indices, int d)
        {
            if (d == a.Rank - 1)
            {
                for (int i = a.GetLowerBound(d); i <= a.GetUpperBound(d); i++)
                {
                    indices[d] = i;
                    values.Add((T)a.GetValue(indices));
                }
            }
            else
            {
                for (int i = a.GetLowerBound(d); i <= a.GetUpperBound(d); i++)
                {
                    indices[d] = i;
                    FlattenArrayDimension(a, ref values, ref indices, d + 1);
                }
            }
        }

        public static T[] FlattenArray<T>(Array a)
        {
            List<T> flat = new List<T>();
            int[] indices = new int[a.Rank];
            FlattenArrayDimension(a, ref flat, ref indices, 0);
            return flat.ToArray();
        }

        private static readonly char[] hash = { '#' };
        public static Int32 ParseInteger(string str)
        {
            string[] parts = str.Split(hash);
            int parts_count = parts.Count();
            if (parts_count > 3) throw new FormatException("Too many '#' in integer");
            int radix = 10;
            int value;
            if (parts_count >= 2)
            {
                if (!Int32.TryParse(parts[parts_count - 2], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out radix))
                {
                    throw new FormatException("Invalid radix for number");
                }
            }
            if (radix == 10)
            {
                if (!Int32.TryParse(parts[parts_count - 1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out value))
                {
                    throw new FormatException("Invalid decimal number");
                }
            }
            else if (radix == 16)
            {
                if (!Int32.TryParse(parts[parts_count - 1], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out value))
                {
                    throw new FormatException("Invalid hexadecimal number");
                }
            }
            else
            {
                throw new FormatException("Unhandled radix " + radix + " for number");
            }
            return value;
        }

        static readonly char[] string_trim = { '\'' };
        public static Object ParseValue(string str, DataType type)
        {
            Object value;


            if (type is BitString bits)
            {
                value = ParseInteger(str);
            }
            else if (type is Integer)
            {
                value = ParseInteger(str);
            }
            else if (type is Float)
            {
                value = Double.Parse(str, System.Globalization.CultureInfo.InvariantCulture);

            }
            else if (type is BOOL)
            {
                string vstr = str.ToLower();
                if (vstr == "true")
                {
                    value = true;
                }
                else if (vstr == "false")
                {
                    value = false;
                }
                else
                {
                    throw new FormatException("Invalid bool value");
                }
            }
            else if (type is STRING)
            {
                value = str.Trim(string_trim);
            }
            else
            {
                throw new NotImplementedException("Unhandled value type " + type.ToString());
            }
            return value;
        }

        public static Array GetPathValues(XmlElement tag_element, PathComponent path, ConstantLookup constants)
        {
            List<Limits> limits = new List<Limits>();
            XmlElement elem = GetPathElement(tag_element, ref limits, path, constants);
            XmlNodeList start_values = elem.SelectNodes(".//if:Subelement/if:StartValue", XMLUtil.nameSpaces);
            int[] lengths = limits.Select(x => (x.High - x.Low + 1)).ToArray();
            int[] lower = limits.Select(x => x.Low).ToArray();

            DataType value_type = DataTypeParser.Parse(elem.GetAttribute("Datatype"), out string _);
            if (value_type is ARRAY value_array)
            {
                value_type = value_array.MemberType;
            }

            Type array_type;
            if (value_type is BitString bits)
            {
                array_type = typeof(int);
            }
            else if (value_type is Integer)
            {
                array_type = typeof(int);
            }
            else if (value_type is Float)
            {
                array_type = typeof(double);
            }
            else if (value_type is BOOL)
            {
                array_type = typeof(bool);
            }
            else if (value_type is STRING)
            {
                array_type = typeof(string);
            }
            else
            {
                throw new NotImplementedException("Unhandled value type " + value_type.ToString());
            }
            Array array = Array.CreateInstance(array_type, lengths, lower);
            
            foreach (XmlElement start_value in start_values)
            {
                string subpath = ((XmlElement)start_value.ParentNode).GetAttribute("Path");
                int[] indices = subpath.Split(new char[] { ',' }).Select(x=> int.Parse(x)).ToArray();
                string value_str = start_value.InnerText.Trim(new char[1] { '\'' });
                Object value = ParseValue(value_str, value_type);
                try
                {
                    array.SetValue(value, indices);
                }
                catch (IndexOutOfRangeException )
                {
                    // Ignore invalid indices
                }
            }
            return array;
        }

        public static string[] GetPresetNames(XmlElement tag_element, ConstantLookup constants)
        {
            ARRAY name_array = new ARRAY();
            MemberComponent name_tag = new MemberComponent("Names", name_array);
            Array array = GetPathValues(tag_element, name_tag, constants);
            if (array.Rank != 1)
            {
                throw new Exception("Names tag must be one dimensional");
            }


            return FlattenArray<string>(array);
        }

        public static object[] GetPresetValue(XmlElement tag_element, PathComponent path, ConstantLookup constants)
        {

            ARRAY preset_array = new ARRAY
            {
                MemberType = new STRUCT()
            };
            MemberComponent preset_tag = new MemberComponent("Preset", preset_array);
            PathComponent value_path = path.PrependPath(preset_tag);
            Array array = GetPathValues(tag_element, value_path, constants);
            return FlattenArray<object>(array); 
        }
        public static bool[] GetPresetEnabled(XmlElement tag_element, PathComponent path, ConstantLookup constants)
        {

            ARRAY enable_array = new ARRAY
            {
                MemberType = new STRUCT()
            };
            MemberComponent enable_tag = new MemberComponent("Enable", enable_array);
            PathComponent value_path = path.PrependPath(enable_tag);
            Array array = GetPathValues(tag_element, value_path, constants);
            return FlattenArray<bool>(array);
        }
    }
}
