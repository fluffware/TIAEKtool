using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TIAEKtool.Plc;
using System.Globalization;
using ClosedXML.Excel;
using System.Drawing;
using TIAEktool.Plc.Types;

namespace TIAEKtool
{
    abstract class AlarmProperty
    {
        public string Name { get; protected set; }
        public string Label { get; protected set; }
        public abstract void ReadCell(IXLCell cell, AlarmTag tag);
        public abstract void WriteCell(IXLCell cell, AlarmTag tag);

        virtual public void PrepareCell(IXLCell cell)
        {

        }
    }

    abstract class AlarmStringProperty: AlarmProperty
    {
        protected abstract string GetString(AlarmTag tag);
        protected abstract void SetString(AlarmTag tag, string value);

        override
        public void ReadCell(IXLCell cell, AlarmTag tag)
        {
            string value = cell.GetValue<string>().Trim();
            SetString(tag, value);
        }
        override
        public void WriteCell(IXLCell cell, AlarmTag tag)
        {
            cell.SetValue<string>(GetString(tag));
        }
    }

    abstract class AlarmBoolProperty : AlarmProperty
    {
        protected abstract bool GetBool(AlarmTag tag);
        protected abstract void SetBool(AlarmTag tag, bool value);

        override
        public void ReadCell(IXLCell cell, AlarmTag tag)
        {
            bool value = cell.GetValue<bool>();
            SetBool(tag, value);
        }
        override
        public void WriteCell(IXLCell cell, AlarmTag tag)
        {
            cell.SetValue<bool>(GetBool(tag));
        }

        override public void PrepareCell(IXLCell cell)
        {
            // Use a locale independent representation of TRUE
            cell.AddConditionalFormat().WhenEquals("=1=1") .Fill.SetBackgroundColor(XLColor.Green);
            cell.SetDataValidation().List(cell.Worksheet.Workbook.Worksheet("Scratch").Range("A1:A2"));
        }
    }

    class PlcTagProperty: AlarmStringProperty
    {
        public PlcTagProperty()
        {
            Label = "Plc Tag";
            Name = "plc_tag";
        }
        override protected string GetString(AlarmTag tag)
        {
            return tag.plcTag.ToString();
        }
        override protected void SetString(AlarmTag tag, string value)
        {
            tag.plcTag = PathComponentUtils.ParsePath(value);
        }
    }

    class AlarmClassProperty : AlarmStringProperty
    {
        public AlarmClassProperty()
        {
            Label = "Alarm Class";
            Name = "alarm_class";
        }
        override protected string GetString(AlarmTag tag)
        {
            return tag.alarmClass;
        }
        override protected void SetString(AlarmTag tag, string value)
        {
            tag.alarmClass = value;
        }
    }

    class SinkProperty : AlarmBoolProperty
    {
        readonly string sink;
        readonly string label;
        public SinkProperty(string sink, string label)
        {
            Label = "Alarm Target: "+label;
            Name = "alarm_target_" + sink;
            this.sink = sink;
        }
        override protected bool GetBool(AlarmTag tag)
        {

            return tag.targets.Contains(sink);
        }
        override protected void SetBool(AlarmTag tag, bool value)
        {
            if (value)
            {
                tag.targets.Add(sink);
            }
            else
            {
                tag.targets.Remove(sink);
            }

        }
    }

    abstract class AlarmIntProperty : AlarmProperty
    {
        protected abstract int GetInt(AlarmTag tag);
        protected abstract void SetInt(AlarmTag tag, int value);

        override
        public void ReadCell(IXLCell cell, AlarmTag tag)
        {
            int value = cell.GetValue<int>();
            SetInt(tag, value);
        }
        override
        public void WriteCell(IXLCell cell, AlarmTag tag)
        {
            cell.SetValue<int>(GetInt(tag));
        }
    }

    class IdProperty : AlarmIntProperty
    {
        public IdProperty()
        {
            Label = "ID";
            Name = "id";
        }
        override protected int GetInt(AlarmTag tag)
        {
            return tag.id;
        }
        override protected void SetInt(AlarmTag tag, int value)
        {

            tag.id = value;
        }
    }
    class PriorityProperty : AlarmIntProperty
    {
        public PriorityProperty()
        {
            Label = "Priority";
            Name = "priority";
        }
        override protected int GetInt(AlarmTag tag)
        {
            return tag.priority;
        }
        override protected void SetInt(AlarmTag tag, int value)
        {

            tag.priority = value;
        }
    }

    class DelayProperty : AlarmIntProperty
    {
        public DelayProperty()
        {
            Label = "Delay (ms)";
            Name = "delay";
        }
        override protected int GetInt(AlarmTag tag)
        {
            return tag.delay;
        }
        override protected void SetInt(AlarmTag tag, int value)
        {

            tag.delay = value;
        }
    }

    class AlarmEdgeProperty : AlarmStringProperty
    {
        public AlarmEdgeProperty()
        {
            Label = "Edge";
            Name = "edge";
           
        }
        override protected string GetString(AlarmTag tag)
        {
            return tag.edge.ToString();
           
        }
        override protected void SetString(AlarmTag tag, string value)
        {
            switch(value.Trim().ToLower())
            {
                case "rising":
                    tag.edge = AlarmTag.Edge.Rising;
                    break;
                case "falling":
                    tag.edge = AlarmTag.Edge.Falling;
                    break;
            }
        }

        override public void PrepareCell(IXLCell cell)
        {
            cell.SetDataValidation().List(cell.Worksheet.Workbook.Worksheet("Scratch").Range("B1:B2"));
        }
    }

    class AlarmTextProperty : AlarmStringProperty
    {
        readonly string culture;
        public AlarmTextProperty(string culture)
        {
            Label = "Alarm Text";
            Name = "alarm_text";
            this.culture = culture;
        }
        override protected string GetString(AlarmTag tag)
        {
            if (tag.eventText != null && tag.eventText.TryGetText(culture, out string text))
                return text;
            else
                return "";
        }
        override protected void SetString(AlarmTag tag, string value)
        {
            if (tag.eventText == null) tag.eventText = new MultilingualText();
            tag.eventText.AddText(culture, value);
        }
    }
    class AdditionalTextProperty : AlarmStringProperty
    {
        readonly string culture;
        readonly int index;
        public AdditionalTextProperty(string culture, int index)
        {
            Label = "Additional Text "+index;
            Name = "additional_text_"+index;
            this.culture = culture;
            this.index = index;
        }

        override protected string GetString(AlarmTag tag)
        {
            if (tag.additionalText != null && tag.additionalText[index].TryGetText(culture, out string text))
                return text;
            else
                return "";
        }
        override protected void SetString(AlarmTag tag, string value)
        {
            if (tag.additionalText != null)
            {
                tag.additionalText[index].AddText(culture, value);
            }
        }
    }
    public abstract class AlarmDocument
    {
        public class PresetInfo
        {
            public PresetTag tag;
            public object[] values;
            public bool?[] enabled;
        }

        public class PresetGroup
        {
            public string[] preset_names;
            public uint[] preset_colors;
            public int[] preset_symbols;
            public List<PresetInfo> presets;

            public PresetGroup()
            {
            }

            public PresetGroup(int preset_count)
            {
                preset_names = new string[preset_count];
                preset_colors = new uint[preset_count];
                preset_symbols = new int[preset_count];
                presets = new List<PresetInfo>();
            }
        }

        protected static string PathType(PathComponent path)
        {
            DataType type = path.Type;
            if (type == null) return "Unknown";
            else return type.ToDebug();
        }

   
        static private List<AlarmProperty> FixedColumns(string culture)
        {
            return new List<AlarmProperty> {
                new IdProperty(),
                new PlcTagProperty(),
                new AlarmClassProperty(),
                new PriorityProperty(),
                new AlarmTextProperty(culture),
                new AdditionalTextProperty(culture,0),
                new AdditionalTextProperty(culture,1),
                new DelayProperty(),
                new AlarmEdgeProperty()
            };
        }

        const string COLUMN_PREFIX = "column_";
      
        static public void Save(string file, List<AlarmTag> tags, List<AlarmTarget> targets, string culture)
        {



            var wb = new XLWorkbook();
            var scratch_ws = wb.Worksheets.Add("Scratch");
            scratch_ws.Hide();
            var scratch_next_col = 1;

            // FALSE/TRUE for drop down lists
            scratch_ws.Cell(1, scratch_next_col).SetValue(false);
            scratch_ws.Cell(2, scratch_next_col).SetValue(true);
            scratch_next_col++;

            // Rising/falling for edge
            scratch_ws.Cell(1, scratch_next_col).SetValue("Rising");
            scratch_ws.Cell(2, scratch_next_col).SetValue("Falling");
            scratch_next_col++;

            var ws = wb.Worksheets.Add("Alarms");

            List<AlarmProperty> columns = FixedColumns(culture);

            HashSet<string> sinks = new HashSet<string>();

            foreach (AlarmTag alarm_tag in tags)
            {
                sinks.UnionWith(alarm_tag.targets);
            }
            List<string> sorted_sinks = sinks.ToList();
            sorted_sinks.Sort();
            foreach(string sink in sorted_sinks)
            {
                string label = targets.Find(x => sink == x.name)?.label ?? sink;
                columns.Add(new SinkProperty(sink, label));
            }

            int row_index = 1;

            row_index++;


            var col_index = 1;
            foreach (AlarmProperty prop in columns)
            {
                var label_cell = ws.Cell(row_index, col_index);
                label_cell.Value = prop.Label;
                var name_cell = ws.Cell(row_index+1, col_index);
                name_cell.Value = COLUMN_PREFIX+prop.Name;
                col_index++;
            }
            ws.Row(row_index+1).Hide();
            row_index+=2;
            foreach (AlarmTag alarm_tag in tags)
            {
                col_index = 1;
                foreach (AlarmProperty prop in columns)
                {
                    var cell = ws.Cell(row_index, col_index);
                    prop.PrepareCell(cell);
                    prop.WriteCell(cell, alarm_tag);
                    col_index++;
                }
                row_index++;
            }
            wb.SaveAs(file);
        }

    

        static public string TimespanToPLCValue(TimeSpan t)
        {
            string str = "T#";
            if (t.TotalMilliseconds == 0) return str + "0s";
            if (t.Days > 0) str += t.Days + "d";
            if (t.Hours > 0) str += t.Hours + "h";
            if (t.Minutes > 0) str += t.Minutes + "m";
            if (t.Seconds > 0) str += t.Seconds + "s";
            if (t.Milliseconds > 0) str += t.Milliseconds + "ms";
            return str;
        }

        public static bool StripPrefix(string text, string prefix, out string stripped)
        {
            if (text.StartsWith(prefix))
            {
                stripped = text.Substring(prefix.Length);
                return true;
            } else
            {
                stripped = null;
                return false;
            }
        }
        static public void Load(string file, out List<AlarmTag> tags, string culture)
        {
            tags = new List<AlarmTag>();
           
          
            XLWorkbook wb = new XLWorkbook(file);
            IXLWorksheet ws = wb.Worksheet("Alarms");
            if (ws == null)
            {
                throw new Exception("No worksheet named 'Alarms' found");
            }
            int row_index = 1;

            row_index+=2;
            int col_index = 1;
            List<AlarmProperty> available_columns = FixedColumns(culture);
            var names = ws.Range(ws.Cell(row_index, col_index), ws.Row(row_index).LastCellUsed());
           
            List<AlarmProperty> columns = new List<AlarmProperty>();
            foreach (IXLCell cell in names.Cells())
            {
                string cell_value = cell.GetValue<string>();
                string label_value = cell.CellAbove().GetValue<String>();
                int split_pos = label_value.IndexOf(':');
                string label = (split_pos >= 0) ? label_value.Substring(split_pos + 1) : label_value;
                AlarmProperty prop;
                if (StripPrefix(cell_value, COLUMN_PREFIX, out string name))
                {
                    if (StripPrefix(name, "alarm_target_", out string target))
                    {
                        prop = new SinkProperty(target, label);
                    }
                    else
                    {
                        prop = available_columns.Find(x => x.Name == name);
                    }
                } else
                {
                    prop = null;
                }

                columns.Add(prop);
            }

            row_index++;

            var first_cell = ws.Cell(row_index, col_index);
            while (true)
            {
                
                if (first_cell.IsEmpty()) break;
                var cell = first_cell;
                var tag = new AlarmTag();
                tag.InitDefaults();
                foreach(AlarmProperty prop in columns)
                {
                    if (prop != null)
                    {
                        prop.ReadCell(cell, tag);
                    }
                    cell = cell.CellRight();
                }
                tags.Add(tag);
                first_cell = first_cell.CellBelow();
            }
    
        }
    }
}
