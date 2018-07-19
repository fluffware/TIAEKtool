using PLC.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TIAEKtool.Properties;

namespace TIAEKtool
{
    public class PresetDB
    {
        public class TagPathException : Exception
        {
            public TagPathException(string msg) : base(msg)
            {

            }
        }
        protected XmlElement static_section;
        protected XmlElement enable_array;
        protected XmlElement preset_array;
        protected XmlDocument doc;
        public XmlDocument Document { get => doc; }
      
        XmlNamespaceManager nsmgr;
        protected const string InterfaceNS = "http://www.siemens.com/automation/Openness/SW/Interface/v3";
        public PresetDB(string block_name, Constant array_length, XmlDocument doc = null)
        {
          
            NameTable nt = new NameTable();
            nsmgr = new XmlNamespaceManager(nt);
            nsmgr.AddNamespace("if", InterfaceNS);

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

            preset_array = GetMember(static_section, "Preset", array_type, false);

            enable_array = GetMember(static_section, "Enable", array_type, false);

            XmlElement name_elem = 
                (XmlElement)doc.SelectSingleNode("/Document/SW.Blocks.GlobalDB/AttributeList/Name", nsmgr);
            name_elem.InnerText = block_name;
        }
       

        protected const int SystemDefined = 1;
        protected const int Informative = 2;

        protected XmlElement CreateAttribute(string type, string name, string value, int flags = 0)
        {
            XmlElement elem = doc.CreateElement(type, InterfaceNS);

            elem.SetAttribute("Name", name);

            if ((flags & SystemDefined) != 0)
            {
                elem.SetAttribute("SystemDefined", "true");
            }
            if ((flags & Informative) != 0)
            {
                elem.SetAttribute("Informative", "true");
            }
            elem.InnerText = value;
            return elem;
        }

        public XmlElement GetMember(XmlNode parent_node, string name, DataType type, bool add_members)
        {
            XmlElement member_elem = parent_node.SelectSingleNode("if:Member[@Name='" + name + "']",nsmgr) as XmlElement;
            if (member_elem == null)
            {
                member_elem = doc.CreateElement("Member", InterfaceNS);
                member_elem.SetAttribute("Name", name);
                member_elem.SetAttribute("Datatype", type.ToString());
                member_elem.SetAttribute("Remanence", "Retain");
                member_elem.SetAttribute("Accessibility", "Public");
                XmlNode attr_list = doc.CreateElement("AttributeList", InterfaceNS);
                attr_list.AppendChild(CreateAttribute("BooleanAttribute", "ExternalAccessible", "true", SystemDefined));
                attr_list.AppendChild(CreateAttribute("BooleanAttribute", "ExternalVisible", "true", SystemDefined));
                attr_list.AppendChild(CreateAttribute("BooleanAttribute", "ExternalWritable", "true", SystemDefined));
                member_elem.AppendChild(attr_list);
                parent_node.AppendChild(member_elem);
            }
            string type_str = member_elem.GetAttribute("Datatype");
            string left;
            DataType mtype = DataTypeParser.Parse(type_str, out left);
            if ((mtype is STRUCT && type is STRUCT))
            {
                if (add_members)
                {
                    STRUCT struct_type = (STRUCT)type;
                    foreach (StructMember m in struct_type.Members)
                    {
                        GetMember(member_elem, m.Name, m.MemberType, true);
                    }
                }
            }
            else if ((mtype is ARRAY && type is ARRAY) && (((ARRAY)mtype).MemberType is STRUCT && ((ARRAY)type).MemberType is STRUCT))
            {
                if (add_members)
                {
                    STRUCT struct_type = (STRUCT)((ARRAY)type).MemberType;
                    foreach (StructMember m in struct_type.Members)
                    {
                        GetMember(member_elem, m.Name, m.MemberType, true);
                    }
                }
            }
            else
            {
                if (!mtype.Equals(type))
                    throw new TagPathException("Types " + mtype.ToDebug() + " and " + type.ToDebug() + " doesn't match for " + name);
            }

            return member_elem;
        }

        protected XmlNode AddPathValues(TagComponent path, TagComponent child, out int[] indices)
        {
            XmlNode parent_node;
            if (path.Parent == null)
            {
                parent_node = preset_array;
                indices = new int[0];
            }
            else
            {
                parent_node = AddPathValues(path.Parent, path, out indices);
            }

                string name = path.Name;
                DataType type = path.Type;
                parent_node = GetMember(parent_node, name, type, child == null || type is ARRAY);
            
           
            if (path is ArrayComponent) {
                ArrayComponent ac = (ArrayComponent)path;
                int[] new_indices = new int[indices.Length + ac.Indices.Length];
                Array.Copy(indices, new_indices, indices.Length);
                Array.Copy(ac.Indices, 0, new_indices, indices.Length, ac.Indices.Length);
                indices = new_indices;
            }
            return parent_node;
        }

        protected XmlNode AddPathEnable(TagComponent path, TagComponent child, out int[] indices)
        {
            XmlNode parent_node;
            if (path.Parent == null)
            {
                parent_node = enable_array;
                indices = new int[0];

            }
            else
            {
                parent_node = AddPathEnable(path.Parent, path, out indices);
            }

            string name = path.Name;
            DataType type;
            type = path.Type;
            if (child == null)
            {
                if (type is ARRAY)
                {
                    ARRAY array_type = (ARRAY)type;
                    type = new ARRAY() { Limits = array_type.Limits, MemberType = BOOL.Type };
                }
                else
                {
                    type = BOOL.Type;
                }
            }
            parent_node = GetMember(parent_node, name, type, type is ARRAY);

            if (path is ArrayComponent)
            {
                ArrayComponent ac = (ArrayComponent)path;
                int[] new_indices = new int[indices.Length + ac.Indices.Length];
                Array.Copy(indices, new_indices, indices.Length);
                Array.Copy(ac.Indices, 0, new_indices, indices.Length, ac.Indices.Length);
                indices = new_indices;
            }
            return parent_node;
        }
        public XmlNode AddPath(TagComponent path)
        {
            int[] indices;
            AddPathEnable(path, null, out indices);
            return AddPathValues(path, null, out indices);
        }

       
    }
}
