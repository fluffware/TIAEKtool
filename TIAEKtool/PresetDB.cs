using System;
using System.Xml;
using TIAEktool.Plc.Types;
using TIAEKtool.Plc;
using TIAEKtool.Properties;

namespace TIAEKtool
{
    public class PresetDB : InterfaceType
    {
       
        protected XmlElement static_section;
        protected PathComponent enable_prefix;
        protected PathComponent preset_prefix;

        protected PathComponent enable_selected_prefix;
        protected PathComponent preset_selected_prefix;

   
        public PresetDB(string block_name, Constant array_length, XmlDocument doc = null)
        {
            has_remanence_attr = true;
            if (doc == null)
            {
                doc = new XmlDocument();
                doc.LoadXml(Resources.InitialPresetDB);
            }
            this.doc = doc;
            static_section =
                (XmlElement)doc.SelectSingleNode("/Document/SW.Blocks.GlobalDB/AttributeList/Interface/if:Sections/if:Section[@Name='Static']", nsmgr);
            if (static_section == null) throw new Exception("No section named 'Static' in XML");

            ARRAY array_type = new ARRAY();
            array_type.Limits.Add(new ArrayLimits(new IntegerLiteral(1), array_length));
            array_type.MemberType = new STRUCT();

            int[] preset_index = new int[1] { 1 };
            PathComponent preset_member = new MemberComponent("Preset", array_type);
            preset_prefix = new IndexComponent(preset_index,  preset_member);

            PathComponent enable_member = new MemberComponent("Enable", array_type);
            enable_prefix = new IndexComponent(preset_index,  enable_member);

            enable_selected_prefix = new MemberComponent("EnableSelected", new STRUCT());
            preset_selected_prefix = new MemberComponent("PresetSelected", new STRUCT());

            XmlElement name_elem =
                (XmlElement)doc.SelectSingleNode("/Document/SW.Blocks.GlobalDB/AttributeList/Name", nsmgr);
            name_elem.InnerText = block_name;
        }
       

        public XmlNode AddPath(PathComponent path, MultilingualText comment, string start_value)
        {
            int[] indices;
            AddPathEnable(static_section, path.PrependPath(enable_prefix), out indices);
            AddPathEnable(static_section, path.PrependPath(enable_selected_prefix), out indices);

            AddPathValues(static_section, path.PrependPath(preset_selected_prefix), null, out indices);
            PathComponent preset_path = path.PrependPath(preset_prefix);
            XmlElement value_node = AddPathValues(static_section, preset_path, null, out indices);
            return AddPathAttributes(value_node, preset_path, comment, start_value, indices);
         

        }

       
    }
}
