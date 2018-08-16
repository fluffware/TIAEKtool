using Siemens.Engineering;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public class CopyPlcTypeTask : SequentialTask
    {
        TiaPortal portal;
        PlcType fromType;
        PlcSoftware toPlc;
        bool overwrite;
        public CopyPlcTypeTask(TiaPortal portal, PlcType from, PlcSoftware to, bool overwrite)
        {
            
            this.portal = portal;
            fromType = from;
            toPlc = to;
            this.overwrite = overwrite;
            Selected = true;
            Description = "Copy type "+ from.Name + " to " + to.Name;
        }

        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {
                    Stack<string> group_names = new Stack<string>();
                    IEngineeringObject obj = fromType.Parent;
                    while (obj is PlcTypeUserGroup)
                    {
                        group_names.Push(((PlcTypeGroup)obj).Name);
                        obj = obj.Parent;
                    }
                    PlcTypeGroup group = toPlc.TypeGroup;
                    foreach (string group_name in group_names)
                    {
                        PlcTypeUserGroup child = group.Groups.Find(group_name);
                        if (child != null)
                        {
                            group = child;
                        }
                        else
                        {
                            group = group.Groups.Create(group_name);
                        }
                    }
                    FileInfo file = TempFile.File("copy_type_", "xml");
                    fromType.Export(file, ExportOptions.None);
                    group.Types.Import(file, overwrite ? ImportOptions.Override : ImportOptions.None);
                 
                } catch(Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to copy from "+ fromType.Name +" to "+toPlc.Name+":\n" + ex.Message);
                    return;
                }
            }
        }
    }
}
