using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PLC.Types;
using System.Globalization;
using ClosedXML.Excel;

namespace TIAEKtool
{

    abstract class PresetDocument
    {
        public class PresetInfo
        {
            public PresetTag tag;
            public object[] values;
            public bool[] enabled;
        }


        protected static string PathType(PathComponent path)
        {
            DataType type = path.Type;
            if (type == null) return "Unknown";
            else return type.ToDebug();
        }

        // For now just use default values 
        static string ResolveLabelTags(string label)
        {
            
            
            
                int i = label.IndexOf("<hmitag");
            if (i == -1) return label;
            int pos = 0;
            int last = 0;
            StringBuilder res = new StringBuilder(label.Substring(last,i));
            while (true)
            {
                i = label.IndexOf(">", pos);
                if (i == -1) break;
                pos = i + 1;
                last = pos;
                i = label.IndexOf("<", pos);
                if (i == -1) break;
                pos = i + 7;
                res.Append(label, last, i - last);
            }
            res.Append(label, last, label.Length - last);

            return res.ToString();
        }

        const int N_PROPERTIES = 8;
        static public void Save(string file, Dictionary<string, List<PresetInfo>> preset_groups, Dictionary<string, string[]> preset_names, string culture)
        {

            int npresets = 20;
            var wb = new XLWorkbook();


            foreach (var preset_group in preset_groups)
            {
                var ws = wb.Worksheets.Add("Group " + preset_group.Key);
                string[] headers = new string[N_PROPERTIES]  { "Decription", "Order", "Tag",  "Type", "Unit", "Min", "Max", "Precision"};
                    int row_index = 1;


                var rangePresetTitle = ws.Range(row_index, N_PROPERTIES + 1, row_index, N_PROPERTIES + npresets);
                rangePresetTitle.FirstCell().Value = "Presets";
                rangePresetTitle.Merge();
                rangePresetTitle.Style.Font.Bold = true;

                row_index++;

                var rangePresetNameHeader = ws.Range(row_index, 1, row_index, N_PROPERTIES);
                rangePresetNameHeader.FirstCell().Value = "Preset name";
                rangePresetNameHeader.Merge();
                rangePresetNameHeader.Style.Font.Bold = true;
                rangePresetNameHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                var rangePresetName = ws.Cell(row_index, N_PROPERTIES+1).InsertData(preset_names[preset_group.Key], true);
                row_index++;

                var rangeHeader =  ws.Cell(row_index, 1).InsertData(headers,true);
                rangeHeader.Style.Font.Bold = true;

                row_index++;
                

                foreach (PresetInfo info in preset_group.Value)
                {
                    int col_index = 1;

                    PresetTag tag = info.tag;
                    // Label
                    ws.Cell(row_index, col_index).Value = ResolveLabelTags(tag.labels[culture].ToString());
                    col_index++;
                    // Order
                    ws.Cell(row_index, col_index).Value = tag.order;
                    col_index++;
                    // PLC tag
                    ws.Cell(row_index, col_index).Value = tag.tagPath.ToString();
                    col_index++;      
                    // Value type
                    ws.Cell(row_index, col_index).Value = PathType(tag.tagPath);
                    col_index++;
                    // Value unit
                    ws.Cell(row_index, col_index).Value = tag.unit;
                    col_index++;
                    // Minimum value
                    ws.Cell(row_index, col_index).Value = tag.min;
                    col_index++;
                    // Maximum value
                    ws.Cell(row_index, col_index).Value = tag.max;
                    col_index++;
                    // Decimal precision
                    ws.Cell(row_index, col_index).Value = tag.precision;

                    col_index++;
                    for(int i = 0; i < info.values.Count(); i++) {
                        var cell = ws.Cell(row_index, col_index);
                        cell.Value = info.values[i];
                        if (info.enabled[i])
                        {
                            cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
                            cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }
                        col_index++;
                    }
                   
                    row_index++;
                }
                wb.SaveAs(file);
            }
        }
       
    }
}
