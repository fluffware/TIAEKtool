using PLC.Types;
using Siemens.Engineering;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.Hmi.TextGraphicList;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TIAEKtool.Properties;

namespace TIAEKtool
{
    public class DeleteHmiTextListTask : SequentialTask
    {
        TiaPortal portal;
        TextListComposition text_lists;
        List<string> list_names;
        String prefix;
       
        public DeleteHmiTextListTask(TiaPortal portal, string prefix, TextListComposition text_lists, List<String> list_names)
        {
            this.portal = portal;
            this.text_lists = text_lists;
            this.list_names = list_names;
            this.prefix = prefix;

            

            Description = "Delete old HMI text lists starting with "+prefix;
        }


        protected override void DoWork()
        {
            lock (portal)
            {

               
                    foreach (var name in list_names)
                    {
                        TextList list = text_lists.Find(name);
                        if (list != null)
                        {
                            try
                            {
                                list.Delete();
                            }
                            catch (Exception ex)
                            {
                                LogMessage(MessageLog.Severity.Error, "Failed to delete text list " + name + ":" + ex.Message);
                                return;
                            }
                            LogMessage(MessageLog.Severity.Info, "Deleted text list " + name);
                        }
                   
                }
            }

        }

    }
}