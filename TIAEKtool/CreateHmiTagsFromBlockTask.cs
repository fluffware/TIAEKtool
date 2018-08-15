using PLC.Types;
using Siemens.Engineering;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.Xml;

namespace TIAEKtool
{
    public class CreateHmiTagsFromBlockTask : SequentialTask
    {
        TiaPortal portal;
        PlcBlock block;
        TagFolder folder;
        string function;
        ConstantLookup constants;
        public CreateHmiTagsFromBlockTask(TiaPortal portal, PlcBlock block, TagFolder folder, string function, ConstantLookup constants)
        {
            this.portal = portal;
            this.folder = folder;
            this.block = block;
            this.function = function;
            this.constants = constants;
            Description = "Update HMI tag table " + function + " for block " + block.Name;
        }

        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {
                   

                    TagTable table = folder.TagTables.Find(function);

                    if (table != null)
                    {
                        
                        XmlDocument table_doc = TIAutils.ExportHMITagTableXML(table);
                        TagParser parser = new TagParser(portal);
                        parser.HandleTag += Parser_HandleTag;
                        parser.Parse(block, log, TagParser.Options.AllowNoComment | TagParser.Options.NoSubelement);
                       
                        TIAutils.ImportHMITagTableXML(table_doc, folder);
                    } else
                    {
                        LogMessage(MessageLog.Severity.Warning, "No tag table named " + function + " for tags from " + block.Name);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to update tag table from block:\n" + ex.Message);
                    return;
                }

            }

        }

        private void Parser_HandleTag(object sender, TagParser.HandleTagEventArgs e)
        {
            DataType type = e.Path.Type;
            if (type is Integer || type is BOOL || type is Float || type is STRING)
            {
                PathComponent path = PathComponentUtils.InitializeArrayPath(e.Path, constants);
                do
                {
                    LogMessage(MessageLog.Severity.Debug, "tag " + path.ToString());
                } while (PathComponentUtils.NextArrayPath(path));
            }
        }
    }
}