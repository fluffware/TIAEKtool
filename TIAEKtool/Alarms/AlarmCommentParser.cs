using System;

namespace TIAEKtool
{
    public static class AlarmCommentParser
    {
        class Ctxt
        {
            public AlarmTag alarm_tag = null;
            public AlarmSink alarm_sink = null;
            public string culture;

            AlarmTag get_tag()
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
                    get_tag();
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
                else if (type == "alarm_id")
                {
                    get_tag();
                    if (int.TryParse(data, out int value) && value >= 0)
                    {
                        alarm_tag.id = value;
                    }
                }
                else if (type == "alarm_sinks")
                {
                    get_tag();
                    char[] split_on = { ',', ' ' };
                    foreach (string sink in data.Split(split_on, StringSplitOptions.RemoveEmptyEntries))
                    {
                        alarm_tag.sinks.Add(sink);
                    }
                }
                else if (type == "alarm_priority")
                {
                    get_tag();
                    if (int.TryParse(data, out int value) && value >= 0)
                    {
                        alarm_tag.priority = value;
                    }
                }
                else if (type == "alarm_delay")
                {
                    get_tag();
                    if (int.TryParse(data, out int value) && value >= 0)
                    {
                        alarm_tag.delay = value;
                    }
                }
                else if (type == "alarm_edge")
                {
                    get_tag();
                    alarm_tag.edge = data.Trim().ToLower() == "falling" ? AlarmTag.Edge.Falling : AlarmTag.Edge.Rising;
                }
                else if (type == "alarm_sink")
                {
                    string name;
                    string label;
                    int p = data.IndexOf(':');
                    if (p < 0)
                    {
                        name = data.Trim();
                        label = name;
                    } else
                    {
                        name = data.Substring(0, p).Trim();
                        label = data.Substring(p + 1).Trim();
                    }

                    alarm_sink = new AlarmSinkTag(name, label);
                }
            }
        }

        static public void Parse(string comment, string culture, out AlarmTag alarm_tag, out AlarmSink alarm_sink)
        {
            Ctxt ctxt = new Ctxt { culture = culture };
            CommentParser.Parse(comment, ctxt.Handler);
            alarm_tag = ctxt.alarm_tag;
            alarm_sink = ctxt.alarm_sink;
        }
    }
}
