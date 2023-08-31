using Siemens.Engineering;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TIAEKtool.Plc;

namespace TIAEKtool.Alarms
{
    public class UpdateAlarmTagsTask : SequentialTask
    {
        readonly TiaPortal portal;
        readonly PlcBlockGroup blocks;
        readonly List<AlarmTag> tags;
        readonly string culture;
        readonly ConstantLookup constants;
        public UpdateAlarmTagsTask(TiaPortal portal, PlcBlockGroup blocks, List<AlarmTag> tags,
            string culture, ConstantLookup constants)
        {
            this.portal = portal;
            this.blocks = blocks;
            this.tags = tags;
            this.culture = culture;
            this.constants = constants;
            Description = "Update alarm definitions for PLC tags";
        }

        protected static PlcBlock PlcBlockRecursiveFind(PlcBlockGroup group, string name)
        {
            PlcBlock block = group.Blocks.Find(name);
            if (block != null) return block;
            foreach (PlcBlockGroup subgroup in group.Groups)
            {
                block = PlcBlockRecursiveFind(subgroup, name);
                if (block != null) return block;
            }
            return null;
        }
        class ReadWriteBlocks : TagsXML.ReadWriteBlock
        {
            private readonly PlcBlockGroup group;

            public ReadWriteBlocks(PlcBlockGroup group)
            {
                this.group = group;
            }

            public override XmlDocument ReadBlock(string name)
            {
                PlcBlock block = PlcBlockRecursiveFind(group,name);
                if (block == null)
                {
                    throw new Exception("PLC block " + name + " not found");
                }
                return TIAutils.ExportPlcBlockXML(block);
            }

            public override void WriteBlock(string name, XmlDocument doc)
            {
                TIAutils.ImportPlcBlockXML(doc, group);
            }
        }

        protected static readonly string[] ALARM_ANNOTATIONS = new string[] {
            "alarm", 
            "alarm_id", 
            "alarm_targets", 
            "alarm_priority", 
            "alarm_delay",
            "alarm_text_1",
            "alarm_text_2",
            "alarm_edge" };

        protected bool EditAlarm(string name, ref string data, AlarmTag alarm_tag, string culture)
        {
            switch(name)
            {
                case "alarm":
                    data = " " + alarm_tag.alarmClass + ":" + alarm_tag.eventText[culture];
                    break;
                case "alarm_text_1":
                    if (alarm_tag.additionalText != null && alarm_tag.additionalText[0] != null)
                    {
                        data = " " + alarm_tag.additionalText[0][culture];
                    }
                    break;
                case "alarm_text_2":
                    if (alarm_tag.additionalText != null && alarm_tag.additionalText[1] != null)
                    {
                        data = " " + alarm_tag.additionalText[1][culture];
                    }
                    break;
                case "alarm_id":
                    data = " " + alarm_tag.id;
                    break;
                case "alarm_priority":
                    data = " " + alarm_tag.priority;
                    break;
                case "alarm_targets":
                    data = " " + string.Join(",",alarm_tag.targets);
                    break;
                case "alarm_delay":
                    data = " " + alarm_tag.delay;
                    break;
                case "alarm_edge":
                    data = " " + alarm_tag.edge;
                    break;
                default:
                    return false;
            }
            return true;
        }
        protected override void DoWork()
        {
            lock (portal)
            {


                try
                {
                    BlockXmlCache block_cache = new BlockXmlCache(blocks);
                    TagCacheXML plc_tags = new TagCacheXML(block_cache, constants); 
                    foreach (AlarmTag alarm_tag in tags)
                    {
                        Tag plc_tag = plc_tags.Find(alarm_tag.plcTag);
                        if (plc_tag == null) throw new Exception("Alarm tag "+alarm_tag.plcTag);
                        StringBuilder comment = new StringBuilder(plc_tag.Comment[culture] ?? "");
                        CommentEditor.Edit(comment, new List<string>(ALARM_ANNOTATIONS), 
                            (string name, ref string data) => EditAlarm(name, ref data, alarm_tag,culture));
                        if (plc_tag.Comment == null)
                        {
                            plc_tag.Comment = new MultilingualText();
                        }
                        plc_tag.Comment.AddText(culture, comment.ToString());
                        plc_tags.Add(plc_tag);
                    }
                    block_cache.WriteAll();

                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to update alarm tags:\n" + ex.Message);
                    return;
                }

            }
        }
    }
}
