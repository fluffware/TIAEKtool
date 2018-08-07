using Siemens.Engineering;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.Xml;

namespace TIAEKtool
{
    public class CreatePresetTypesTask : SequentialTask
    {
        TiaPortal portal;
        PlcTypeGroup typeGroup;
        string valueTypeName;
        string enableTypeName;
        IList<PresetTag> tags;
        public CreatePresetTypesTask(TiaPortal portal, IList<PresetTag> tags, PlcTypeGroup typeGroup, string value_type_name, string enable_type_name)
        {
            this.portal = portal;
            this.tags = tags;
            this.typeGroup = typeGroup;
            this.valueTypeName = value_type_name;
            this.enableTypeName = enable_type_name;
            Description = "Create types " + valueTypeName + " and " + enableTypeName;
        }
        protected override void DoWork()
        {
            lock (portal)
            {
                try
                {

                    // Type for preset values

                    PlcType type = typeGroup.Types.Find(valueTypeName);
                    PresetType preset_type;
                    if (type != null)
                    {
                        XmlDocument type_doc = TIAutils.ExportPlcTypeXML(type);
                        preset_type = new PresetType(valueTypeName, type_doc);
                    }
                    else
                    {
                        LogMessage(Severity.Info, "No type named " + valueTypeName + " found, creating it");
                        preset_type = new PresetType(valueTypeName);
                    }
                    foreach (var tag in tags)
                    {
                        preset_type.AddValueType(tag.tagPath, tag.labels, tag.defaultValue);
                    }
                    TIAutils.ImportPlcTypeXML(preset_type.Document, typeGroup);

                    // Type for enable flags

                    type = typeGroup.Types.Find(enableTypeName);
                    if (type != null)
                    {
                        XmlDocument type_doc = TIAutils.ExportPlcTypeXML(type);
                        preset_type = new PresetType(enableTypeName, type_doc);
                    }
                    else
                    {
                        LogMessage(Severity.Info, "No type named " + enableTypeName + " found, creating it");
                        preset_type = new PresetType(enableTypeName);
                    }
                    foreach (var tag in tags)
                    {
                        preset_type.AddEnableType(tag.tagPath);
                    }
                    TIAutils.ImportPlcTypeXML(preset_type.Document, typeGroup);
                }
                catch (Exception ex)
                {
                    LogMessage(Severity.Error, "Failed to update preset type:\n" + ex.Message);
                    return;
                }
            }
        }
    }
}