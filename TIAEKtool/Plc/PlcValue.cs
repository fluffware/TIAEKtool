using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TIAEktool.Plc.Types;

namespace TIAEKtool.Plc
{
    public static class PlcValue
    {
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

        public static TimeSpan ParseTimeValue(string str)
        {
            Match m = Regex.Match(str, @"^(T|TIME)#((?<value>\d+)(?<unit>ms|d|h|m|s)_?)+$");
            if (!m.Success) throw new Exception("Illegal time vale: " + str);


            Int32 days = 0;
            Int32 hours = 0;
            Int32 minutes = 0;
            Int32 seconds = 0;
            Int32 milliseconds = 0;
            var values = m.Groups["value"].Captures;
            var units = m.Groups["unit"].Captures;
            for (int i = 0; i < values.Count; i++)
            {

                int v = Int32.Parse(values[i].Value);
                switch (units[i].Value)
                {
                    case "d":
                        days = v;
                        break;
                    case "h":
                        hours = v;
                        break;
                    case "m":
                        minutes = v;
                        break;
                    case "s":
                        seconds = v;
                        break;
                    case "ms":
                        milliseconds = v;
                        break;
                }
            }

            return new TimeSpan(days, hours, minutes, seconds, milliseconds);
        }
        static readonly char[] string_trim = { '\'' };
        public static Object ParseValue(string str, DataType type)
        {
            Object value;


            if (type is BitString)
            {
                value = ParseInteger(str);
            }
            else if (type is TIME)
            {
                value = str;
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

        public static string EscapeStringValue(string unescaped)
        {
            return Regex.Replace(unescaped, "['$\\n\\r\\t\\f]", match => {
                switch (match.Captures[0].Value) {
                    case "'":

                        return "$'";
                    case "$":
                        return "$$";
                    case "\n":
                        return "$L";
                    case "\r":
                        return "$R";
                    case "\t":
                        return "$T";
                    case "\f":
                        return "$P";
                    default:
                        return "$?";
                }
            });
        }
        public static string ValueToString(Object value)
        {
            
            if (value is Int32 integer)
            {
                return integer.ToString();
            }
            else if (value is Double d)
            {
                return d.ToString(CultureInfo.InvariantCulture);
            }
            else if (value is string str)
            {
                return "'" + EscapeStringValue(str) + "'";
            }
            else if (value is TimeSpan time)
            {
                StringBuilder time_str = new StringBuilder("T#");
                if (time.Days != 0)
                {
                    time_str.Append(time.Days.ToString() + "d");
                }
                if (time.Hours != 0) { 
                    time_str.Append(time.Hours.ToString() + "h"); 
                }
                if (time.Minutes != 0)
                {
                    time_str.Append(time.Minutes.ToString() + "m");
                }
                if (time.Seconds != 0)
                {
                    time_str.Append(time.Seconds.ToString() + "s");
                }
                if (time.Milliseconds != 0)
                {
                    time_str.Append(time.Milliseconds.ToString() + "ms");
                }
                return time_str.ToString();
            }
            else
            {
                throw new NotImplementedException("Unhandled value type " + value.GetType().ToString());
            }
        }
    }
}
