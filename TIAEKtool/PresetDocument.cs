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
            public bool?[] enabled;
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
            StringBuilder res = new StringBuilder(label.Substring(last, i));
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
                string[] headers = new string[N_PROPERTIES] { "Decription", "Order", "Tag", "Type", "Unit", "Min", "Max", "Precision" };
                int row_index = 1;


                var rangePresetTitle = ws.Range(row_index, N_PROPERTIES + 1, row_index, N_PROPERTIES + npresets);
                rangePresetTitle.FirstCell().Value = "Presets";
                rangePresetTitle.Merge();
                rangePresetTitle.Style.Font.Bold = true;
                row_index++;

                var rangePresetIndexHeader = ws.Range(row_index, 1, row_index, N_PROPERTIES);
                rangePresetIndexHeader.FirstCell().Value = "Preset index";
                rangePresetIndexHeader.Merge();
                rangePresetIndexHeader.Style.Font.Bold = true;
                rangePresetIndexHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                for (int i = 1; i <= preset_group.Value[0].values.Count(); i++)
                {
                    var cell = ws.Cell(row_index, i + N_PROPERTIES);
                    cell.Value = i;
                }
                row_index++;

                var rangePresetNameHeader = ws.Range(row_index, 1, row_index, N_PROPERTIES);
                rangePresetNameHeader.FirstCell().Value = "Preset name";
                rangePresetNameHeader.Merge();
                rangePresetNameHeader.Style.Font.Bold = true;
                rangePresetNameHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                var rangePresetName = ws.Cell(row_index, N_PROPERTIES + 1).InsertData(preset_names[preset_group.Key], true);
                row_index++;

                var rangeHeader = ws.Cell(row_index, 1).InsertData(headers, true);
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
                    for (int i = 0; i < info.values.Count(); i++)
                    {
                        var cell = ws.Cell(row_index, col_index);
                        cell.Value = info.values[i];
                        if (info.enabled[i] == true)
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



        static public void Load(string file,
            out Dictionary<string, List<PresetInfo>> preset_groups,
            out Dictionary<string, string[]> preset_names, string culture)
        {
            preset_groups = new Dictionary<string, List<PresetInfo>>();
            preset_names = new Dictionary<string, string[]>();
           
            XLWorkbook wb = new XLWorkbook(file);
            foreach (var ws in wb.Worksheets)
            {
                string group_name = ws.Name.Substring(6);
                preset_groups[group_name] = new List<PresetInfo>();
                List<int?> preset_indices = new List<int?>();
                var preset_index_cell = ws.Column(1).FirstCellUsed(XLCellsUsedOptions.Contents, c => c.Value as string == "Preset index");
                if (preset_index_cell == null)
                {
                    throw new Exception("Failed to find row starting with 'Preset index' in worksheet " + ws.Name);
                }
                int row_index = preset_index_cell.WorksheetRow().RowNumber();
                int first_value_column = preset_index_cell.MergedRange().LastCell().CellRight().WorksheetColumn().ColumnNumber();
                var index_range = ws.Range(ws.Cell(row_index, first_value_column), ws.Row(row_index).LastCellUsed(c => c.Value is double));
                int last_value_column = index_range.LastColumn().ColumnNumber();
                int max_preset_index = 0;
                foreach (var cell in index_range.Cells())
                {
                    if (cell.Value is double v && v >= 1)
                    {
                        preset_indices.Add((int)v);
                        if (max_preset_index < (int)v) max_preset_index = (int)v;
                    }
                    else
                    {
                        preset_indices.Add(null);
                    }
                }

                row_index++;
                var preset_name_cell = ws.Cell(row_index, 1);
                if (preset_name_cell.Value as string != "Preset name")
                {
                    throw new Exception("Failed to find row starting with 'Preset name' in worksheet " + ws.Name);
                }
                var name_range = ws.Range(row_index, first_value_column, row_index, last_value_column);
                string[] names = name_range.Cells().Select(x => x.Value as string).ToArray<string>();
                preset_names.Add(group_name, names);
                row_index+=2;


                while (ws.Cell(row_index, 3).Value is string path_str) {
                    if (path_str.Length == 0) break;
                    PresetInfo info = new PresetInfo();
                    info.tag = new PresetTag();
                    try
                    {
                        info.tag.tagPath = PathComponentUtils.ParsePath(path_str);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to parse tag '" + path_str + "' in " + ws.Name+": "+ex.Message);
                    }
                    if (!(ws.Cell(row_index, 4).Value is string type_str))
                    {
                        throw new Exception("Invalid type for tag " + path_str + " in " + ws.Name);
                    }
                    DataType value_type = DataTypeParser.Parse(type_str, out string left);
                    if (value_type == null)
                    {
                        throw new Exception("Failed to parse type "+type_str+" for tag " + path_str + " in " + ws.Name);
                    }
                    info.tag.tagPath.Type = value_type;
                    info.values = new object[max_preset_index];
                    info.enabled = new bool?[max_preset_index];
                    for (int i = 0; i <= last_value_column - first_value_column; i++)
                    {
                        if (!(preset_indices[i] is null))
                        {
                            int preset_index = preset_indices[i].Value - 1;
                            IXLCell cell = ws.Cell(row_index, first_value_column + i);
                            var bg = cell.Style.Fill.BackgroundColor.Color;
                            info.enabled[preset_index] = bg.A == 255 && bg.R * 11 < bg.G * 10 && bg.B * 11 < bg.G * 10;

                            object cell_value = cell.Value;
                            if (value_type is PLC.Types.Integer || value_type is PLC.Types.BitString)
                            {
                                if (cell_value is double v)
                                {
                                    info.values[preset_index] = (long)v;
                                }
                                else
                                {
                                    throw new Exception("Invalid integer in cell " + cell.Address.ToString() + " in " + ws.Name);
                                }
                            }
                            else if (value_type is PLC.Types.Float)
                            {
                                if (cell_value is double v)
                                {
                                    info.values[preset_index] = (double)v;
                                }
                                else
                                {
                                    throw new Exception("Invalid number in cell " + cell.Address.ToString() + " in " + ws.Name);
                                }
                            }
                            else if (value_type is PLC.Types.BOOL)
                            {
                                if (cell_value is bool v)
                                {
                                    info.values[preset_index] = v;
                                }
                                else
                                {
                                    throw new Exception("Invalid boolean in cell " + cell.Address.ToString() + " in " + ws.Name);
                                }

                            }
                            
                        }
                    }
                    row_index++;
                    preset_groups[group_name].Add(info);
                }
                
            }
                
        }
    }
}
