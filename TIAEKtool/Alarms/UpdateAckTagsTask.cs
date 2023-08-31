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
    public class UpdateAckTagsTask : SequentialTask
    {
        readonly TiaPortal portal;
        readonly PlcBlockGroup blocks;
        readonly List<AlarmTag> tags;
        readonly ConstantLookup constants;
        readonly string culture;

        public UpdateAckTagsTask(TiaPortal portal, PlcBlockGroup blocks, List<AlarmTag> tags, string culture, ConstantLookup constants)
        {
            this.portal = portal;
            this.blocks = blocks;
            this.constants = constants;
            this.culture = culture;
            this.tags = tags;
            Description = "Add missing _ack tags for PLC alarm tags";
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

                        if (alarm_tag.plcTag is MemberComponent alarm_member)
                        {
                            MemberComponent ack_path = new MemberComponent(alarm_member.Name + "_ack", TIAEktool.Plc.Types.BYTE.Type, alarm_member.Parent);

                            Tag ack_tag = plc_tags.Find(ack_path);
                            if (ack_tag == null)
                            {
                                ack_tag = new Tag(ack_path);
                                if (ack_tag.Comment == null)
                                {
                                    ack_tag.Comment = new MultilingualText();
                                }
                                ack_tag.Comment.AddText(culture, "Bit 0 - ACK status, bit 1 - ACK control");
                                plc_tags.Add(ack_tag);
                            }


                        }
                    }
                    block_cache.WriteAll(false);

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
