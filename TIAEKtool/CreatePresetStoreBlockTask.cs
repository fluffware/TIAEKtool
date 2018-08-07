using Siemens.Engineering;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.Xml;

namespace TIAEKtool
{
    public class CreatePresetStoreBlockTask : SequentialTask
    {
        TiaPortal portal;
        PlcBlockGroup resultGroup;
        string valueTypeName;
        string enableTypeName;
        string blockName;
        IList<PresetTag> tags;
        public CreatePresetStoreBlockTask(TiaPortal portal, IList<PresetTag> tags, PlcBlockGroup blockGroup, string blockName, string value_type_name, string enable_type_name)
        {
            this.portal = portal;
            this.tags = tags;
            this.resultGroup = blockGroup;
            this.valueTypeName = value_type_name;
            this.enableTypeName = enable_type_name;
            this.blockName = blockName;
            Description = "Create store SCL block " + blockName;
        }
        protected override void DoWork()
        {
            lock (portal)
            {


                try
                {

                    PresetStoreSCL scl = new PresetStoreSCL(blockName, valueTypeName, null);
                    foreach (var tag in tags)
                    {
                        if (!tag.noStore)
                        {
                            scl.AddStore(tag.tagPath);
                        }
                    }
                    TIAutils.ImportPlcBlockXML(scl.Document, resultGroup);
                }
                catch (Exception ex)
                {
                    LogMessage(Severity.Error, "Failed to update preset store SCL block:\n" + ex.Message);
                    return;
                }

            }
        }
    }
}