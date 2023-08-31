using Siemens.Engineering;
using Siemens.Engineering.Hmi.Tag;
using System;
using System.Collections.Generic;

namespace TIAEKtool
{
    public class CreateHmiPresetConstantTagsTask : SequentialTask
    {
        readonly TiaPortal portal;
        readonly TagFolder folder;
        readonly ConstantLookup constants;
        const string PRESET_CONSTANT_TABLE_NAME = "PresetConstants";
        public CreateHmiPresetConstantTagsTask(TiaPortal portal, TagFolder folder, ConstantLookup constants)
        {
            this.portal = portal;
            this.folder = folder;
            this.constants = constants;
            Description = "Update HMI preset constant table " + PRESET_CONSTANT_TABLE_NAME;
        }

        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {


                    HMIConstantTable table = new HMIConstantTable(PRESET_CONSTANT_TABLE_NAME);
                    foreach (KeyValuePair<string, ConstantLookup.Entry> entry in constants)
                    {
                        if (entry.Key.StartsWith("PresetNumber"))
                        {
                            if (int.TryParse(entry.Value.value, out int value))
                            {
                                table.AddIntegerConstant(entry.Key, value);
                            }
                        }
                    }
                    TIAutils.ImportHMITagTableXML(table.Document, folder);

                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to update preset constant table " + PRESET_CONSTANT_TABLE_NAME + "\n" + ex.Message);
                    return;
                }

            }

        }

    }
}