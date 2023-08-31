using System;

namespace TIAEKtool
{
    public static class AlarmCommentParser
    {
        class Ctxt
        {
            public AlarmTag alarm_tag = null;
            public AlarmTarget alarm_sink = null;
            public string culture;

            AlarmTag GetTag()
            {
                if (alarm_tag == null)
                {
                    alarm_tag = new AlarmTag();
                    alarm_tag.InitDefaults();

                }
                return alarm_tag;
            }



            public void Handler(string type, string data)
            {
                if (type == "alarm")
                {
                    GetTag();
                    int p = data.IndexOf(':');
                    if (p < 0) return;
                    string alarm_class = data.Substring(0, p).Trim();
                    if (alarm_class == "") alarm_class = "Alarm";
                    string label = data.Substring(p + 1).Trim();

                    if (alarm_tag.eventText == null)
                    {
                        alarm_tag.eventText = new MultilingualText(culture, label);
                    }
                    else
                    {
                        alarm_tag.eventText.AddText(culture, label);
                    }
                    alarm_tag.alarmClass = alarm_class;
                }
                else if (type == "alarm_text_1")
                {
                    alarm_tag.additionalText[0] = new MultilingualText(culture, data.Trim());
                }
                else if (type == "alarm_text_2")
                {
                    alarm_tag.additionalText[1] = new MultilingualText(culture, data.Trim());
                }
                else if (type == "alarm_id")
                {
                    GetTag();
                    if (int.TryParse(data, out int value) && value >= 0)
                    {
                        alarm_tag.id = value;
                    }
                }
                else if (type == "alarm_targets")
                {
                    GetTag();
                    char[] split_on = { ',', ' ' };
                    foreach (string sink in data.Split(split_on, StringSplitOptions.RemoveEmptyEntries))
                    {
                        alarm_tag.targets.Add(sink);
                    }
                }
                else if (type == "alarm_priority")
                {
                    GetTag();
                    if (int.TryParse(data, out int value) && value >= 0)
                    {
                        alarm_tag.priority = value;
                    }
                }
                else if (type == "alarm_delay")
                {
                    GetTag();
                    if (int.TryParse(data, out int value) && value >= 0)
                    {
                        alarm_tag.delay = value;
                    }
                }
                else if (type == "alarm_edge")
                {
                    GetTag();
                    alarm_tag.edge = data.Trim().ToLower() == "falling" ? AlarmTag.Edge.Falling : AlarmTag.Edge.Rising;
                }
                else if (type == "alarm_target")
                {
                    string name;
                    string label;
                    int p = data.IndexOf(':');
                    if (p < 0)
                    {
                        name = data.Trim();
                        label = name;
                    }
                    else
                    {
                        name = data.Substring(0, p).Trim();
                        label = data.Substring(p + 1).Trim();
                    }

                    alarm_sink = new AlarmTargetTag(name, label);
                }
            }
        }

        static public void Parse(string comment, string culture, out AlarmTag alarm_tag, out AlarmTarget alarm_sink)
        {
            Ctxt ctxt = new Ctxt { culture = culture };
            CommentParser.Parse(comment, ctxt.Handler);
            alarm_tag = ctxt.alarm_tag;
            alarm_sink = ctxt.alarm_sink;
        }
    }
}
