using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TIAEKtool.Plc;
using TIAEKtool.Properties;

namespace TIAEKtool
{
    public class PresetType : InterfaceType
    {
        protected XmlElement type_section;
        

        public PresetType(string type_name, XmlDocument doc = null)
        {

            has_remanence_attr = false;

            if (doc == null)
            {
                doc = new XmlDocument();
                doc.LoadXml(Resources.InitialPresetType);
            }
            this.doc = doc;
            type_section =
                (XmlElement)doc.SelectSingleNode("/Document/SW.Types.PlcStruct/AttributeList/Interface/if:Sections/if:Section[@Name='None']", nsmgr);
            if (type_section == null) throw new Exception("No section named 'None' in XML");

            
            XmlElement name_elem =
                (XmlElement)doc.SelectSingleNode("/Document/SW.Types.PlcStruct/AttributeList/Name", nsmgr);
            name_elem.InnerText = type_name;
        }

        public XmlNode AddValueType(PathComponent path, MultilingualText comment, string start_value)
        {
            int[] indices;
            XmlElement value_node = AddPathValues(type_section, path, null, out indices);
            return AddPathAttributes(value_node, path, comment, start_value, indices);
        }

        public XmlNode AddEnableType(PathComponent path)
        {
            int[] indices;
            return AddPathEnable(type_section, path, out indices);
        }
    }
}
