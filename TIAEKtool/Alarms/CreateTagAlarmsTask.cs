using Siemens.Engineering;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using TIAEKtool.Plc;

namespace TIAEKtool.Alarms
{
    public class CreateTagAlarmsTask : SequentialTask
    {
        readonly TiaPortal portal;
        readonly PlcSoftware plc_software;
        readonly IList<AlarmTag> alarm_tags;
        IList<AlarmTarget> targets;

        public CreateTagAlarmsTask(TiaPortal portal, PlcSoftware plc, IList<AlarmTarget> targets, IList<AlarmTag> alarm_tags)
        {
            this.portal = portal;
            plc_software = plc;
            this.targets = targets;
            this.alarm_tags = alarm_tags;

            Description = plc.Name + ": Create tag alarms";
        }

        const string ALARM_HANDLING_BLOCK_NAME = "AlarmHandling";
        protected override void DoWork()
        {
            lock (portal)
            {
                string blockName = ALARM_HANDLING_BLOCK_NAME;
                

                PlcBlockGroup resultGroup = (Plc.PlcUtils.FindPlcBlockName(blockName, plc_software.BlockGroup)?.Parent as PlcBlockGroup) ?? plc_software.BlockGroup;
               
                try
                {
                    AlarmHandlingSCL scl = new AlarmHandlingSCL(blockName, null);
                    foreach (var target in targets)
                    {
                        if (target is AlarmTargetTag tag_target)
                        scl.AddTargetTag(tag_target, alarm_tags);
                    }
                    TIAutils.ImportPlcBlockXML(scl.Document, resultGroup);
                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to create tag alarms:\n" + ex.Message);
                    return;
                }

            }
        }

    }
}
