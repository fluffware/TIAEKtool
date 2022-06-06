using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PLC.Types;
using System.Globalization;
using ClosedXML.Excel;
using System.Drawing;

namespace TIAEKtool
{

    public abstract class PresetDocument
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
            public List<PresetInfo> presets;

            public PresetGroup()
            {
            }

            public PresetGroup(int preset_count)
            {
                preset_names = new string[preset_count];
                preset_colors = new uint[preset_count];
                presets = new List<PresetInfo>();
            }
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

        static bool TagValueAsInt(object value, out int i)
        {
            if (value is int v)
            {
                i = v;
                return true;
            }
            if (value is bool b)
            {
                i = b ? 1 : 0;
                return true;
            }
            i = 0;
            return false;
        }

        const int N_PROPERTIES = 8; // Number of properties for for each tag not including the state values
        static public void Save(string file, Dictionary<string, PresetGroup> preset_groups, string culture)
        {

        

            var wb = new XLWorkbook();
            var scratch_ws = wb.Worksheets.Add("Scratch");
            scratch_ws.Hide();
            var scratch_next_col = 1;


            foreach (var preset_group in preset_groups)
            {
                int npresets = preset_group.Value.presets[0].values.Count();
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
                for (int i = 1; i <= npresets; i++)
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

                var rangePresetName = ws.Cell(row_index, N_PROPERTIES + 1).InsertData(preset_group.Value.preset_names, true);
                row_index++;

                var rangePresetColorHeader = ws.Range(row_index, 1, row_index, N_PROPERTIES);
                rangePresetColorHeader.FirstCell().Value = "Color";
                rangePresetColorHeader.Merge();
                rangePresetColorHeader.Style.Font.Bold = true;
                rangePresetColorHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
               
               //var rangePresetColors = ws.Cell(row_index, N_PROPERTIES + 1).InsertData(hexcolors, true);
                for (int i = 0; i < npresets; i++)
                {
                    int a = (int)preset_group.Value.preset_colors[i] >> 24 & 0xff;
                    int r = (int)preset_group.Value.preset_colors[i] >> 16 & 0xff;
                    int g = (int)preset_group.Value.preset_colors[i] >> 8 & 0xff;
                    int b = (int)preset_group.Value.preset_colors[i] & 0xff;
                    int grey = r *30 + g * 59 + b * 11;
                    IXLCell cell = ws.Cell(row_index, i + N_PROPERTIES + 1);
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(a, r, g, b);
                    cell.Style.Font.FontColor = grey > 128 * 100 ? XLColor.Black : XLColor.White;
                    cell.Value = "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
                }
                row_index++;

                var rangeHeader = ws.Cell(row_index, 1).InsertData(headers, true);
                rangeHeader.Style.Font.Bold = true;

                row_index++;

                Dictionary<string, IXLRange> state_lists = new Dictionary<string, IXLRange>();
                foreach (PresetInfo info in preset_group.Value.presets)
                {
                    PresetTag tag = info.tag;
              
                    if (tag.state_labels != null && tag.state_labels.Count > 0)
                    {
                        List<string> list = new List<string>();
                        int list_row = 1;
                        foreach (KeyValuePair<int, MultilingualText> state in tag.state_labels)
                        {
                            if (state.Value.TryGetText(culture, out string text))
                            {
                                var list_cell = scratch_ws.Cell(list_row, scratch_next_col);
                                list_cell.Value = state.Key.ToString() + ":" + ResolveLabelTags(text);
                                list_row++;
                            }
                        }
                        var range = scratch_ws.Range(1, scratch_next_col, list_row - 1, scratch_next_col);
                        state_lists.Add(tag.readTagPath.ToString(), range);
                        scratch_next_col++;
                    }


                }
                foreach (PresetInfo info in preset_group.Value.presets)
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
                    ws.Cell(row_index, col_index).Value = tag.readTagPath.ToString();
                    col_index++;
                    // Value type
                    ws.Cell(row_index, col_index).Value = PathType(tag.readTagPath);
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
                      
                         
                        if (state_lists.TryGetValue(tag.readTagPath.ToString(), out IXLRange range))
                        {
                            cell.DataValidation.List(range);
                        }
                        if (TagValueAsInt(info.values[i], out int value))
                        {
                            if (tag.state_labels != null && tag.state_labels.TryGetValue(value, out MultilingualText state_text))
                            {
                                if (state_text.TryGetText(culture, out string text))
                                {
                                    cell.Value = value.ToString() + ":" + ResolveLabelTags(text);
                                }
                            }
                        }
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

        static public void Load(string file, out Dictionary<string, PresetGroup> preset_groups, string culture)
        {
            preset_groups = new Dictionary<string, PresetGroup>();
          
            XLWorkbook wb = new XLWorkbook(file);
            foreach (var ws in wb.Worksheets)
            {
                if (!ws.Name.StartsWith("Group ")) continue;
                string group_name = ws.Name.Substring(6);
               

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
                var group = new PresetGroup
                {
                    preset_names = names
                };

                row_index += 1;

                var preset_color_cell = ws.Cell(row_index, 1);
                if (preset_color_cell.Value as string != "Color")
                {
                    throw new Exception("Failed to find row starting with 'Color' in worksheet " + ws.Name);
                }
                var color_range = ws.Range(row_index, first_value_column, row_index, last_value_column);
                List<uint> colors = new List<uint>();
                foreach (IXLCell cell in color_range.Cells()) {
                    string color_str = cell.GetValue<string>().Trim();
                    uint color = (uint)0xff << 24; //Opaque
                    if (color_str.Length > 0)
                    {
                        if (color_str[0] != '#') throw new Exception("Color must start with '#' in cell " + cell.Address.ToString() + " in " + ws.Name);
                        color |= Convert.ToUInt32(color_str.Substring(1, 2), 16) << 16;
                        color |= Convert.ToUInt32(color_str.Substring(3, 2), 16) << 8;
                        color |= Convert.ToUInt32(color_str.Substring(5, 2), 16);
                        
                       
                    } else
                    {
                        Color cell_color = Color.Black;
                        switch (cell.Style.Fill.BackgroundColor.ColorType)
                        {
                            case XLColorType.Theme:
                                cell_color = wb.Theme.ResolveThemeColor(cell.Style.Fill.BackgroundColor.ThemeColor).Color;
                                break;
                            case XLColorType.Color:
                                cell_color = cell.Style.Fill.BackgroundColor.Color;
                                break;
                            case XLColorType.Indexed:
                                cell_color = XLColor.FromIndex(cell.Style.Fill.BackgroundColor.Indexed).Color;
                                break;
                        }
                        color |= ((uint)cell_color.R << 16) | ((uint)cell_color.G << 8) | cell_color.B;
                    }
                    colors.Add(color);
                }

                group.preset_colors = colors.ToArray();

                row_index += 2;

                group.presets = new List<PresetInfo>();
                while (ws.Cell(row_index, 3).Value is string path_str) {
                    if (path_str.Length == 0) break;
                    PresetInfo info = new PresetInfo
                    {
                        tag = new PresetTag()
                    };
                    try
                    {
                        info.tag.readTagPath = PathComponentUtils.ParsePath(path_str);
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
                    info.tag.readTagPath.Type = value_type ?? throw new Exception("Failed to parse type "+type_str+" for tag " + path_str + " in " + ws.Name);
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


                            if (value_type is PLC.Types.TIME)
                            {
                                if (cell_value is string ts)
                                {
                                    info.values[preset_index] = ts;
                                }
                                else
                                {
                                    throw new Exception("Invalid time in cell " + cell.Address.ToString() + " in " + ws.Name);
                                }

                            }
                            else
                            if (value_type is PLC.Types.Integer || value_type is PLC.Types.BitString)
                            {
                                if (cell_value is double v)
                                {
                                    info.values[preset_index] = (long)v;
                                }
                                else if (cell_value is string str)
                                {
                                    var parts = str.Split(new char[] { ':' });
                                    info.values[preset_index] = long.Parse(parts[0]);
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
                                else if (cell_value is string str)
                                {
                                    var parts = str.Split(new char[] { ':' });
                                    info.values[preset_index] = double.Parse(parts[0]);
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
                                else if (cell_value is string str)
                                {
                                    var parts = str.Split(new char[] { ':' });
                                    info.values[preset_index] = long.Parse(parts[0]) != 0;
                                }
                                else
                                {
                                    throw new Exception("Invalid boolean in cell " + cell.Address.ToString() + " in " + ws.Name);
                                }

                            }
                            else if (value_type is PLC.Types.STRING)
                            {
                                info.values[preset_index] = cell_value.ToString();
                            }

                        }

                    }
                    group.presets.Add(info);
                    row_index++;
                   
                }
                preset_groups[group_name] = group;
            }
                
        }
    }
}
