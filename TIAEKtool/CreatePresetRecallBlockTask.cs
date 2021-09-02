using Siemens.Engineering;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.Xml;

namespace TIAEKtool
{
    public class CreatePresetRecallBlockTask : SequentialTask
    {
        TiaPortal portal;
        PlcBlockGroup resultGroup;
        string valueTypeName;
        string enableTypeName;
        string blockName;
        IList<PresetTag> tags;
        public CreatePresetRecallBlockTask(TiaPortal portal, IList<PresetTag> tags, PlcBlockGroup blockGroup, string blockName, string value_type_name, string enable_type_name)
        {
            this.portal = portal;
            this.tags = tags;
            this.resultGroup = blockGroup;
            this.valueTypeName = value_type_name;
            this.enableTypeName = enable_type_name;
            this.blockName = blockName;
            Description = "Create recall SCL block " + blockName;
        }
        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {
                  
                    PresetRecallSCL scl = new PresetRecallSCL(blockName, valueTypeName, enableTypeName, null);
                    foreach (var tag in tags)
                    {
                        scl.AddRecall(tag.readTagPath, tag.writeTagPath);
                    }
                    TIAutils.ImportPlcBlockXML(scl.Document, resultGroup);
                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to update preset recall SCL block:\n" + ex.Message);
                    return;
                }

              
            }
        }
    }
}