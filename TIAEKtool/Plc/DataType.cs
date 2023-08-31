using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TIAEKtool;

namespace TIAEktool.Plc.Types
{

    public static class DataTypeParser
    {

        static readonly string[] keywords = {
            BYTE.TypeName, WORD.TypeName, DWORD.TypeName, LWORD.TypeName,
            SINT.TypeName, INT.TypeName, DINT.TypeName, LINT.TypeName,
            USINT.TypeName, UINT.TypeName, UDINT.TypeName, ULINT.TypeName,
            BOOL.TypeName,
            "Array", "Struct","String",
            REAL.TypeName, LREAL.TypeName,
            TIME.TypeName, LTIME.TypeName
        };
        static readonly DataType[] types = {
            BYTE.Type, WORD.Type, DWORD.Type, LWORD.Type,
            SINT.Type, INT.Type, DINT.Type, LINT.Type,
            USINT.Type, UINT.Type, UDINT.Type, ULINT.Type,
            BOOL.Type,
            new ARRAY(), new STRUCT(), new STRING(),
            REAL.Type, LREAL.Type,
            TIME.Type, LTIME.Type
        };


        static Dictionary<string, DataType> build_keyword_dictionary()
        {
            Dictionary<string, DataType> dict = new Dictionary<string, DataType>();
            for (int i = 0; i < keywords.Length; i++)
            {
                dict.Add(keywords[i].ToLowerInvariant(), types[i]);
            }
            return dict;
        }
        static Dictionary<string, DataType> keyword_lookup = build_keyword_dictionary();

        static Constant ParseConstant(string str, out string left)
        {
            left = str;
            str = str.TrimStart();
            if (str.Length == 0) return null;
            if (str[0] == '"')
            {
                int end = str.IndexOf('"', 1);
                if (end < 0)
                {
                    return null;
                }
                left = str.Substring(end + 1);
                return new GlobalConstant(str.Substring(1, end - 1));
            }
            else if (str[0] == '#')
            {
                if (str.Length < 2) return null;
                int i = 1;
                while (i < str.Length)
                {
                    char c = str[i];
                    if (!(Char.IsLetterOrDigit(c) || c == '_'))
                    {
                        break;
                    }
                    i++;
                }
                left = str.Substring(i);
                return new LocalConstant(str.Substring(1, i - 1));
            }
            else if (str[0] == '-' || Char.IsDigit(str[0]))
            {

                int i = 1;
                while (i < str.Length && Char.IsDigit(str[i])) i++;
                try
                {
                    int value = int.Parse(str.Substring(0, i));
                    left = str.Substring(i);
                    return new IntegerLiteral(value);
                }
                catch (Exception)
                {
                    return null;
                }

            }
            else return null;
        }

        public static bool MatchSymbol(string sym, string str, out string left)
        {
            left = str;
            str = str.TrimStart();
            if (!str.StartsWith(sym)) return false;
            left = str.Substring(sym.Length);
            return true;
        }

        public static DataType Parse(string str, out string left)
        {
            char[] ws = { ' ', '\t' };
            left = str;
            str = str.TrimStart();
            if (str.Length == 0) return null;
            int i;
            for (i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                {
                    break;
                }
            }
            if (i == 0) return UNKNOWN.Type;
            DataType type;
            if (!keyword_lookup.TryGetValue(str.Substring(0, i).ToLowerInvariant(), out type))
            {
                return UNKNOWN.Type;
            }
            left = str.Substring(i);
            str = left;
            if (type is ARRAY)
            {
                ARRAY array_type = new ARRAY();
                if (!MatchSymbol("[", str, out str)) return null;
                while (true)
                {
                    Constant low = ParseConstant(str, out str);
                    if (low == null) return null;
                    if (!MatchSymbol("..", str, out str)) return null;
                    Constant high = ParseConstant(str, out str);
                    if (low == null) return null;
                    array_type.Limits.Add(new ArrayLimits(low, high));
                    if (!MatchSymbol(",", str, out str)) break;
                }
                if (!MatchSymbol("]", str, out str)) return null;
                if (!MatchSymbol("of", str, out str)) return null;
                DataType item_type = Parse(str, out str);
                if (item_type == null) return null;
                left = str;
                array_type.MemberType = item_type;
                return array_type;
            }
            else if (type is STRING)
            {
                STRING string_type = new STRING();
                if (!MatchSymbol("[", str, out str))
                {
                    string_type.Capacity = new IntegerLiteral(254);
                }
                else
                {
                    Constant capacity = ParseConstant(str, out str);
                    if (capacity == null) return null;
                    if (!MatchSymbol("]", str, out str)) return null;
                    left = str;
                    string_type.Capacity = capacity;
                }
                return string_type;
            }
            else if (type is STRUCT)
            {
                STRUCT struct_type = new STRUCT();

                return struct_type;
            }

            return type;
        }
    }

    public abstract class DataType
    {

        public virtual string ToDebug()
        {
            return ToString();
        }

    }

    public class UNKNOWN : DataType
    {
        private UNKNOWN()
        {
        }
        public static readonly UNKNOWN Type = new UNKNOWN();
        public static readonly string TypeName = "Unknown";
        public override string ToString() => TypeName;
    }

    public abstract class BitString : DataType
    {
        public virtual int ByteLength { get => 0; }
    }

    public class BYTE : BitString
    {
        private BYTE()
        {
        }
        public static readonly BYTE Type = new BYTE();
        public static readonly string TypeName = "Byte";
        public override int ByteLength { get => 1; }
        public override string ToString() => TypeName;
    }
    public class WORD : BitString
    {
        private WORD()
        {
        }
        public static readonly WORD Type = new WORD();
        public static readonly string TypeName = "Word";
        public override int ByteLength { get => 2; }
        public override string ToString() => TypeName;
    }

    public class DWORD : BitString
    {
        private DWORD()
        {
        }
        public static readonly DWORD Type = new DWORD();
        public static readonly string TypeName = "DWord";
        public override int ByteLength { get => 4; }
        public override string ToString() => TypeName;
    }

    public class LWORD : BitString
    {
        private LWORD()
        {
        }
        public static readonly LWORD Type = new LWORD();
        public static readonly string TypeName = "LWord";
        public override int ByteLength { get => 8; }
        public override string ToString() => TypeName;
    }

    public abstract class Integer : DataType
    {
        public virtual int ByteLength { get => 0; }
    }

    public abstract class SignedInteger : Integer
    {
    }

    public class SINT : SignedInteger
    {
        private SINT()
        {
        }
        public static readonly SINT Type = new SINT();
        public static readonly string TypeName = "SInt";
        public override int ByteLength { get => 1; }
        public override string ToString() => TypeName;
    }

    public class INT : SignedInteger
    {
        private INT()
        {
        }
        public static readonly INT Type = new INT();
        public static readonly string TypeName = "Int";
        public override int ByteLength { get => 2; }
        public override string ToString() => TypeName;
    }

    public class DINT : SignedInteger
    {
        private DINT()
        {
        }
        public static readonly DINT Type = new DINT();
        public static readonly string TypeName = "DInt";
        public override int ByteLength { get => 4; }
        public override string ToString() => TypeName;
    }

    public class LINT : SignedInteger
    {
        private LINT()
        {
        }
        public static readonly LINT Type = new LINT();
        public static readonly string TypeName = "LInt";
        public override int ByteLength { get => 8; }
        public override string ToString() => TypeName;
    }

    public abstract class UnsignedInteger : Integer
    {
    }

    public class USINT : UnsignedInteger
    {
        private USINT()
        {
        }
        public static readonly USINT Type = new USINT();
        public static readonly string TypeName = "USInt";
        public override int ByteLength { get => 1; }
        public override string ToString() => TypeName;
    }
    public class UINT : UnsignedInteger
    {
        private UINT()
        {
        }
        public static readonly UINT Type = new UINT();
        public static readonly string TypeName = "UInt";
        public override int ByteLength { get => 2; }
        public override string ToString() => TypeName;
    }

    public class UDINT : UnsignedInteger
    {
        private UDINT()
        {
        }
        public static readonly UDINT Type = new UDINT();
        public static readonly string TypeName = "UDInt";
        public override int ByteLength { get => 4; }
        public override string ToString() => TypeName;
    }

    public class ULINT : UnsignedInteger
    {
        private ULINT()
        {
        }
        public static readonly ULINT Type = new ULINT();
        public static readonly string TypeName = "ULInt";
        public override int ByteLength { get => 8; }

        public override string ToString() => TypeName;
    }

    public class BOOL : DataType
    {
        private BOOL()
        {
        }
        public static readonly BOOL Type = new BOOL();
        public static readonly string TypeName = "Bool";
        public override string ToString() => TypeName;
    }

    public abstract class Float : DataType
    {
    }

    public class REAL : Float
    {
        private REAL()
        {
        }
        public static readonly REAL Type = new REAL();
        public static readonly string TypeName = "Real";
        public override string ToString() => TypeName;
    }

    public class LREAL : Float
    {
        private LREAL()
        {
        }
        public static readonly LREAL Type = new LREAL();
        public static readonly string TypeName = "LReal";
        public override string ToString() => TypeName;
    }

    public class STRING : DataType
    {
        public Constant Capacity { get; set; }
        public override bool Equals(object obj)
        {
            return obj != null && obj is STRING && ((STRING)obj).Capacity.Equals(Capacity);
        }

        public override int GetHashCode()
        {
            return Capacity.GetHashCode();
        }

        public override string ToString()
        {
            return "String[" + Capacity + "]";
        }
    }

    public abstract class Constant
    {
        public int ResolveInt(ConstantLookup lookup)
        {
            if (this is IntegerLiteral i) return i.Value;
            if (this is NamedConstant c)
            {
                return lookup.IntegerLookup(c.Name);
            }
            else
            {
                throw new Exception("Can only resolve named constants");
            }

        }
    }

    public class IntegerLiteral : Constant
    {
        public int Value { get; set; }
        public IntegerLiteral(int v)
        {
            Value = v;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is IntegerLiteral && ((IntegerLiteral)obj).Value.Equals(Value);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public abstract class NamedConstant : Constant
    {
        public string Name;

       
    }

    public class LocalConstant : NamedConstant
    {
        public LocalConstant(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is LocalConstant && ((LocalConstant)obj).Name.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return "#" + Name;
        }
    }

    public class GlobalConstant : NamedConstant
    {
        public GlobalConstant(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is GlobalConstant && ((GlobalConstant)obj).Name.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return "\"" + Name + "\"";
        }
    }

    public class ArrayLimits
    {
        public Constant LowLimit;
        public Constant HighLimit;
        public ArrayLimits(Constant low, Constant high)
        {
            LowLimit = low;
            HighLimit = high;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ArrayLimits)) return false;
            ArrayLimits limits = (ArrayLimits)obj;
            if (LowLimit == null || HighLimit == null) return false;
            return LowLimit.Equals(limits.LowLimit) && HighLimit.Equals(limits.HighLimit);
        }

        public override int GetHashCode()
        {
            return LowLimit.GetHashCode() ^ HighLimit.GetHashCode();
        }
    }

    public class ARRAY : DataType
    {
        public IList<ArrayLimits> Limits = new List<ArrayLimits>();
        public DataType MemberType;

        public override string ToString()
        {
            StringBuilder str = new StringBuilder("Array[");
            for (int l = 0; l < Limits.Count; l++)
            {
                str.Append(Limits[l].LowLimit);
                str.Append("..");
                str.Append(Limits[l].HighLimit);
                if (l + 1 != Limits.Count()) str.Append(",");
            }
            str.Append("] of ");
            str.Append(MemberType);
            return str.ToString();
        }

        public override string ToDebug()
        {
            StringBuilder str = new StringBuilder("Array[");
            for (int l = 0; l < Limits.Count; l++)
            {
                str.Append(Limits[l].LowLimit);
                str.Append("..");
                str.Append(Limits[l].HighLimit);
                if (l + 1 != Limits.Count()) str.Append(",");
            }
            str.Append("] of ");
            str.Append(MemberType.ToDebug());
            return str.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ARRAY)) return false;
            ARRAY array = (ARRAY)obj;

            if (Limits.Count != array.Limits.Count) return false;
            for (int l = 0; l < Limits.Count; l++)
            {
                if (!Limits[l].Equals(array.Limits[l])) return false;
            }
            return MemberType.Equals(array.MemberType);
        }
        public override int GetHashCode()
        {
            int hash = 0;
            foreach (ArrayLimits l in Limits)
            {
                hash ^= l.GetHashCode();
            }
            return MemberType.GetHashCode() ^ hash;
        }
    }

    public class StructMember
    {
        public String Name;
        public DataType MemberType;
    }

    public class STRUCT : DataType
    {
        public IList<StructMember> Members = new List<StructMember>();

        public override string ToString()
        {
            return "Struct";
        }

        public override string ToDebug()
        {
            StringBuilder str = new StringBuilder("Struct {");
            foreach (StructMember m in Members)
            {
                str.Append(m.Name);
                str.Append(": ");
                str.Append(m.MemberType.ToDebug());
                str.Append("; ");
            }
            str.Append("}");
            return str.ToString();
        }
    }

    public class TIME : SignedInteger
    {
        private TIME()
        {
        }
        public static readonly TIME Type = new TIME();
        public static readonly string TypeName = "Time";
        public override string ToString() => TypeName;
        public override int ByteLength { get => 4; }
      
    }

    public class LTIME : SignedInteger
    {
        private LTIME()
        {
        }
        public static readonly LTIME Type = new LTIME();
        public static readonly string TypeName = "LTime";
        public override string ToString() => TypeName;
        public override int ByteLength { get => 8; }
       
    }
}
