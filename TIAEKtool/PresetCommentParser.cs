using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public static class PresetCommentParser
    {
        class Ctxt
        {
            public PresetTag preset;
            public string culture;
            public void Handler(string type, string data)
            {
                if (type == "preset")
                {
                    int p = data.IndexOf(':');
                    if (p < 0) return;
                    string group = data.Substring(0, p).Trim();
                    if (group == "") group = "main";
                    string label = data.Substring(p + 1).Trim();
                    preset.presetGroup = group;
                    if (preset.labels == null)
                    {
                        preset.labels = new MultilingualText(culture, label);
                    }
                    else
                    {
                        preset.labels.AddText(culture, label);
                    }
                }
                else if (type == "preset_default")
                {
                    preset.defaultValue = data.Trim();
                }
                else if (type == "preset_nostore")
                {
                    preset.noStore = true;
                }
                else if (type == "preset_unit")
                {
                    preset.unit = data.Trim();
                }
                else if (type == "preset_precision")
                {
                    int value;
                    if (int.TryParse(data, out value) && value >= 0)
                    {
                        preset.precision = value;
                    }
                }
                else if (type == "preset_state")
                {
                    int p = data.IndexOf(':');
                    if (p < 0) return;
                    string value_str = data.Substring(0, p).Trim();
                    int value;
                    if (int.TryParse(value_str, out value))
                    {
                        string label_str = data.Substring(p + 1).Trim();
                        if (preset.state_labels == null)
                        {
                            preset.state_labels = new Dictionary<int, MultilingualText>();
                        }
                        MultilingualText label;
                        if (preset.state_labels.TryGetValue(value, out label))
                        {
                            label.AddText(culture, label_str);

                        }
                        else
                        {
                            label = new MultilingualText(culture, label_str);
                            preset.state_labels.Add(value, label);
                        }
                    }
                }
            }
        }

        static public void Parse(string comment, string culture, PresetTag preset)
        {
            Ctxt ctxt = new Ctxt { preset = preset, culture = culture };
            CommentParser.Parse(comment, ctxt.Handler);
        }
    }
}
