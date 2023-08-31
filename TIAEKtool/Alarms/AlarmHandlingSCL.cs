using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TIAEKtool.Plc;
using TIAEKtool.Properties;

namespace TIAEKtool.Alarms 
{
    public class AlarmHandlingSCL : BuildSCL
    {
    public AlarmHandlingSCL(string block_name, XmlDocument doc) : base(block_name)
    {
        if (doc == null)
        {
            doc = new XmlDocument();
            doc.LoadXml(Resources.InitialAlarmHandlingSCL);
        }
        SetDocument(doc);

         


    }
        public void AddTargetTag(AlarmTargetTag target, IList<AlarmTag> alarm_tags)
        {

            builder.Push(structured_text);
            string target_id = target.name;
            List<AlarmTag> selected_tags = new List<AlarmTag>();
            foreach(AlarmTag tag in alarm_tags)
            {
                if (tag.targets.Contains(target_id))
                {
                    selected_tags.Add(tag);
                }
            }
            if (selected_tags.Count > 0)
            {
                
                // Target path
                builder.GlobalVariable();
                builder.Down();
                builder.Symbol();
                builder.Down();
                builder.SymbolAddComponents(target.plcTag);
                builder.Pop();
                builder.Pop();

                // :=
                builder.Blank();
                builder.Token(":=");
                builder.Blank();

                for (int i = 0; i < selected_tags.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.NewLine();
                        builder.Blank(4);
                        builder.Token("OR");
                        builder.Blank();
                    }
                    if (selected_tags[i].edge == AlarmTag.Edge.Falling)
                    {
                        builder.Token("NOT");
                        builder.Blank();
                    }
                    // Target path
                    builder.GlobalVariable();
                    builder.Down();
                    builder.Symbol();
                    builder.Down();
                    builder.SymbolAddComponents(selected_tags[i].plcTag);
                    builder.Pop();
                    builder.Pop();
                }
                builder.Token(";");
                builder.NewLine();
            }
        }
}
}
