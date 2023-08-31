using System;
using System.Text;
using System.Xml;
using TIAEktool.Plc.Types;
using TIAEKtool.Plc;

namespace TIAEKtool
{
    public class InterfaceType
    {
        [SerializableAttribute]
        public class TagPathException : Exception
        {
            public TagPathException(string msg) : base(msg)
            {

            }
        }

        protected bool has_remanence_attr;
        protected bool has_external_attrs;
        protected XmlDocument doc;
        public XmlDocument Document { get => doc; }

        protected XmlNamespaceManager nsmgr;
        

        public InterfaceType()
        {
            NameTable nt = new NameTable();
            nsmgr = new XmlNamespaceManager(nt);
            nsmgr.AddNamespace("if", XMLUtil.InterfaceNS);
        }

        protected const int SystemDefined = 1;
        protected const int Informative = 2;

        protected XmlElement CreateAttribute(string type, string name, string value, int flags = 0)
        {
            XmlElement elem = doc.CreateElement(type, XMLUtil.InterfaceNS);

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
            XmlElement member_elem = parent_node.SelectSingleNode("if:Member[@Name='" + name + "']", nsmgr) as XmlElement;
            if (member_elem == null)
            {
                member_elem = doc.CreateElement("Member", XMLUtil.InterfaceNS);
                member_elem.SetAttribute("Name", name);
                member_elem.SetAttribute("Datatype", type.ToString());
                if (has_remanence_attr)
                {
                    member_elem.SetAttribute("Remanence", "Retain");
                }
                member_elem.SetAttribute("Accessibility", "Public");
                XmlNode attr_list = doc.CreateElement("AttributeList", XMLUtil.InterfaceNS);
                if (has_external_attrs)
                {
                    attr_list.AppendChild(CreateAttribute("BooleanAttribute", "ExternalAccessible", "true", SystemDefined));
                    attr_list.AppendChild(CreateAttribute("BooleanAttribute", "ExternalVisible", "true", SystemDefined));
                    attr_list.AppendChild(CreateAttribute("BooleanAttribute", "ExternalWritable", "true", SystemDefined));
                }
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

        protected XmlElement AddPathValues(XmlElement top_parent, PathComponent path, PathComponent child, out int[] indices)
        {
            XmlElement parent_node;
            if (path.Parent == null)
            {
                parent_node = top_parent;
                indices = new int[0];
            }
            else
            {
                parent_node = AddPathValues(top_parent, path.Parent, path, out indices);
            }

            if (path is MemberComponent)
            {
                MemberComponent mc = (MemberComponent)path;
                string name = mc.Name;
                DataType type = mc.Type;
                parent_node = GetMember(parent_node, name, type, child == null);
            }


            if (path is IndexComponent)
            {
                IndexComponent ac = (IndexComponent)path;
                int[] new_indices = new int[indices.Length + ac.Indices.Length];
                Array.Copy(indices, new_indices, indices.Length);
                Array.Copy(ac.Indices, 0, new_indices, indices.Length, ac.Indices.Length);
                indices = new_indices;

                if (child == null) // Add all members to children

                    if (path.Type is STRUCT)
                    {
                        STRUCT struct_type = (STRUCT)path.Type;
                        foreach (StructMember m in struct_type.Members)
                        {
                            GetMember(parent_node, m.Name, m.MemberType, true);
                        }

                    }
            }
            return parent_node;
        }

        protected XmlNode AddPathAttributes(XmlElement elem, PathComponent path, MultilingualText comment, string start_value, int[] indices)
        {
            if (indices.Length > 0)
            {
                StringBuilder sub_path = new StringBuilder(indices[0].ToString());
                for (int i = 1; i < indices.Length; i++)
                {
                    sub_path.Append("," + indices[i]);
                }
                XmlElement sub_elem = elem.SelectSingleNode("if:Subelement[@Path='" + sub_path + "']", nsmgr) as XmlElement;
                if (sub_elem == null)
                {
                    sub_elem = doc.CreateElement("Subelement", XMLUtil.InterfaceNS);

                    sub_elem.SetAttribute("Path", sub_path.ToString());
                    elem.AppendChild(sub_elem);
                }

                elem = sub_elem;
            }
            if (comment != null)
            {
                XmlElement comment_elem = elem.SelectSingleNode("if:Comment", nsmgr) as XmlElement;
                if (comment_elem == null)
                {
                    comment_elem = doc.CreateElement("Comment", XMLUtil.InterfaceNS);
                    elem.AppendChild(comment_elem);
                }

                foreach (string culture in comment.Cultures)
                {
                    XmlElement mt_elem = elem.SelectSingleNode("if:MultiLanguageText[@Lang='" + culture + "']", nsmgr) as XmlElement;
                    if (mt_elem == null)
                    {
                        mt_elem = doc.CreateElement("MultiLanguageText", XMLUtil.InterfaceNS);
                        mt_elem.SetAttribute("Lang", culture);
                        comment_elem.AppendChild(mt_elem);
                    }
                    mt_elem.InnerText = comment[culture];

                }

            }
            if (start_value != null)
            {
                if (elem.SelectSingleNode("if:StartValue", nsmgr) == null)
                {
                    XmlElement start_elem = doc.CreateElement("StartValue", XMLUtil.InterfaceNS);
                    elem.AppendChild(start_elem);
                    start_elem.InnerText = start_value; // Only set start value if there wasn't one before
                }

            }
            return elem;
        }

        protected XmlNode AddPathEnable(XmlElement top_parent, PathComponent path, out int[] indices)
        {

            if (path is IndexComponent)
            {
                IndexComponent ic = (IndexComponent)path;
                MemberComponent mc = (MemberComponent)path.Parent;
                ARRAY array_type = new ARRAY() { Limits = ((ARRAY)mc.Type).Limits, MemberType = BOOL.Type };
                MemberComponent mc_copy = new MemberComponent(mc.Name, array_type, mc.Parent);
                path = new IndexComponent(ic.Indices, mc_copy);
            }
            else
            {
                MemberComponent mc = (MemberComponent)path;
                path = new MemberComponent(mc.Name, BOOL.Type, mc.Parent);
            }
            Console.WriteLine("Path: " + path + " Type: " + path.Type);
            return AddPathValues(top_parent, path, null, out indices);
        }

    }
}
