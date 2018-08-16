using Siemens.Engineering;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public class CopyPlcBlockTask : SequentialTask
    {
        TiaPortal portal;
        PlcBlock fromBlock;
        PlcSoftware toPlc;
        bool overwrite;
        public CopyPlcBlockTask(TiaPortal portal, PlcBlock from, PlcSoftware to, bool overwrite)
        {
            this.portal = portal;
            fromBlock = from;
            toPlc = to;
            this.overwrite = overwrite;
            Selected = true;
            Description = "Copy block "+ from.Name + " to " + to.Name;
        }

        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {
                    Stack<string> group_names = new Stack<string>();
                    IEngineeringObject obj = fromBlock.Parent;
                    while (obj is PlcBlockUserGroup)
                    {
                        group_names.Push(((PlcBlockGroup)obj).Name);
                        obj = obj.Parent;
                    }
                    PlcBlockGroup group = toPlc.BlockGroup;
                    foreach (string group_name in group_names)
                    {
                        PlcBlockUserGroup child = group.Groups.Find(group_name);
                        if (child != null)
                        {
                            group = child;
                        }
                        else
                        {
                            group = group.Groups.Create(group_name);
                        }
                    }
                    FileInfo file = TempFile.File("export_block_", "xml");
                    fromBlock.Export(file, ExportOptions.None);
                    group.Blocks.Import(file, overwrite ? ImportOptions.Override : ImportOptions.None);
                 
                } catch(Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to copy from "+ fromBlock.Name +" to "+toPlc.Name+":\n" + ex.Message);
                    return;
                }
            }
        }
    }
}
