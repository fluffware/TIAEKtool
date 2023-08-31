using Siemens.Engineering;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.Xml;

namespace TIAEKtool.Plc
{
    public class CreateTagTask : SequentialTask
    {
        readonly TiaPortal portal;
        readonly PlcBlockGroup resultGroup;
      

        public CreateTagTask(TiaPortal portal, PlcBlockGroup blockGroup)
        {
            this.portal = portal;
         
            this.resultGroup = blockGroup;
            
            Description = "Test PLC-tags";
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
                PlcBlock block = group.Blocks.Find(name);
                return TIAutils.ExportPlcBlockXML(block);
            }

            public override void WriteBlock(string name, XmlDocument doc)
            {
                TIAutils.ImportPlcBlockXML(doc, group);
            }
        }
        protected override void DoWork()
        {
            lock (portal)
            {


                try
                {


                    PlcBlock block = resultGroup.Blocks.Find("sDB_TagTest");

                    XmlDocument doc = TIAutils.ExportPlcBlockXML(block);
                    TagCacheTree tag_cache = new TagCacheTree();
                    TagsXML.ParseTags(doc, tag_cache);
                    Log.LogMessage(MessageLog.Severity.Debug, "Cache: \n" + tag_cache);
                    TagsXML.WriteTags(tag_cache, new ReadWriteBlocks(resultGroup), null);
                   
                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to test tags:\n" + ex.Message);
                    return;
                }

            }
        }
    }
}