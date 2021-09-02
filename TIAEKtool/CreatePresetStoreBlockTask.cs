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
        string blockEnabledName;
        IList<PresetTag> tags;

        public CreatePresetStoreBlockTask(TiaPortal portal, IList<PresetTag> tags, PlcBlockGroup blockGroup, string blockName, string blockEnabledName, string value_type_name, string enable_type_name)
        {
            this.portal = portal;
            this.tags = tags;
            this.resultGroup = blockGroup;
            this.valueTypeName = value_type_name;
            this.enableTypeName = enable_type_name;
            this.blockName = blockName;
            this.blockEnabledName = blockEnabledName;
            Description = "Create store SCL block " + blockName + " and " + blockEnabledName;
        }

        protected override void DoWork()
        {
            lock (portal)
            {


                try
                {

                    PresetStoreSCL scl = new PresetStoreSCL(blockName, valueTypeName, null);
                    PresetStoreEnabledSCL enabled_scl = new PresetStoreEnabledSCL(blockEnabledName, valueTypeName, enableTypeName, null);
                    foreach (var tag in tags)
                    {
                        if (!tag.noStore)
                        {
                            scl.AddStore(tag.readTagPath);
                            enabled_scl.AddStore(tag.readTagPath);
                        }
                    }
                    TIAutils.ImportPlcBlockXML(scl.Document, resultGroup);
                    TIAutils.ImportPlcBlockXML(enabled_scl.Document, resultGroup);
                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to create preset store SCL block:\n" + ex.Message);
                    return;
                }

            }
        }
    }
}