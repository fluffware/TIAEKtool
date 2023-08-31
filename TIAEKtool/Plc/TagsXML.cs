using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TIAEktool.Plc.Types;

namespace TIAEKtool.Plc
{
    public static class TagsXML
    {
        /// <summary>
        /// Handle an XML element that's part of a variable path
        /// </summary>
        /// <param name="elemet"></param>
        /// <param name="parent">Path of parent</param>
        /// <returns>True if the parser should go deeper</returns>
        public delegate bool ElementHandler(XmlElement elemet, PathComponent parent);

        public static bool ParseTag(XmlElement tag_elem, PathComponent path, TagCacheTree tag_cache)
        {
            Tag tag = new Tag(path)
            {
                Comment = null,
                StartValue = null
            };

            if (tag_elem.SelectSingleNode("if:Comment", XMLUtil.nameSpaces) is XmlElement comment_elem)
            {
                MultilingualText comment = new MultilingualText();
                XmlNodeList text_elems = comment_elem.SelectNodes("if:MultiLanguageText", XMLUtil.nameSpaces);
                foreach (XmlNode t in text_elems)
                {
                    XmlElement mlt = t as XmlElement;
                    string lang = mlt.GetAttribute("Lang");

                    string text = mlt.InnerText;
                    comment.AddText(lang, text);
                }
                tag.Comment = comment;
            }
            if (tag_elem.SelectSingleNode("if:StartValue", XMLUtil.nameSpaces) is XmlElement start_value_elem)
            {
                tag.StartValue = PlcValue.ParseValue(start_value_elem.InnerText, tag.Path.Type);
            }

            tag_cache.Add(tag);
            return true;
        }

        static readonly char[] path_sep = new char[] { ',' };
        public static void ParseSubelement(XmlElement subelement, PathComponent parent, ElementHandler tag_handler)
        {

            string indices_str = subelement.GetAttribute("Path");
            string[] index_strings = indices_str.Split(path_sep);
            int[] indices = new int[index_strings.Length];
            for (int i = 0; i < index_strings.Length; i++)
            {
                indices[i] = int.Parse(index_strings[i]);
            }

            int subs_count = PathComponentUtils.SubstituteIndices(parent, out PathComponent subs, (indices as IList<int>).GetEnumerator());
            if (subs_count != indices.Length)
            {
                if (!(subs is IndexComponent ic)
                    || (subs_count != (indices.Length + ic.Indices.Length)))
                {
                    throw new Exception("Length of path in subelement doesn't match number of indices in path");
                }
                // It's the path of the array itself not an array item.
                subs = subs.Parent;
            }
            tag_handler(subelement, subs);
        }




        public static MemberComponent ParseMember(XmlElement member_elem, PathComponent parent, ElementHandler tag_handler)
        {
            string name = member_elem.GetAttribute("Name");



            string type_str = member_elem.GetAttribute("Datatype");
            DataType type = DataTypeParser.Parse(type_str, out string left);
            MemberComponent member = new MemberComponent(name, type, parent);
            PathComponent child_path = member;

            if (type is ARRAY array)
            {

                child_path = PathComponentUtils.SubstituteIndicesLow(new IndexComponent(member));
            }
            bool parse_subelement = tag_handler(member_elem, member);

            if (parse_subelement)
            {
                XmlNodeList sub_elems = member_elem.SelectNodes("if:Subelement", XMLUtil.nameSpaces);
                foreach (XmlNode s in sub_elems)
                {

                    ParseSubelement(s as XmlElement, child_path, tag_handler);

                }

                XmlNodeList member_elems = member_elem.SelectNodes("if:Member", XMLUtil.nameSpaces);
                foreach (XmlNode m in member_elems)
                {
                    MemberComponent submember = ParseMember((XmlElement)m, child_path, tag_handler);
                    if (child_path.Type is STRUCT struct_type)
                    {
                        struct_type.Members.Add(new StructMember() { Name = submember.Name, MemberType = submember.Type });
                    }
                }
            }

            return member;

        }

      
        public static void ParseBlock(XmlDocument doc, ElementHandler member_handler)
        {
            XmlNode block_attrs = doc.SelectSingleNode("/Document/SW.Blocks.GlobalDB/AttributeList", XMLUtil.nameSpaces);
            if (block_attrs == null) return;

            XmlNode block_name_node = block_attrs.SelectSingleNode("Name", XMLUtil.nameSpaces);
            if (block_name_node == null) throw new XmlException("Missing block name");
            string block_name = block_name_node.InnerText;
            XmlNode static_section = block_attrs.SelectSingleNode("./Interface/if:Sections/if:Section[@Name='Static']", XMLUtil.nameSpaces);
            if (static_section == null) throw new XmlException("Missing static section of block");
            PathComponent parent = new MemberComponent(block_name, new STRUCT());

            XmlNode member_node = static_section.FirstChild;
            while (member_node != null)
            {
                if (member_node.NodeType == XmlNodeType.Element && member_node.Name == "Member")
                {
                    member_handler(member_node as XmlElement, parent);
                }
                member_node = member_node.NextSibling;
            }

        }


        public static void ParseTags(XmlDocument doc, TagCacheTree tag_cache)
        {
            ParseBlock(doc, (elem, path) =>
                        {

                            TagsXML.ParseMember(elem, path, (child_elem, child_path) =>
                            {
                                TagsXML.ParseTag(child_elem, child_path, tag_cache);
                                return true;
                            });
                            return true;
                        });
        }
        private static string ComponentToXPath(PathComponent path, ref List<int> indices)
        {
            string xpath = "";
            if (path.Parent != null)
            {
                xpath = ComponentToXPath(path.Parent, ref indices);
                if (xpath == null) return null;
            }
            if (path is MemberComponent member_path)
            {

                return xpath + "/if:Member[@Name='" + member_path.Name + "'] | /if:Sections/if:Section/if:Member[@Name='" + member_path.Name + "']";

            }
            else if (path is IndexComponent index_path)
            {
                indices.AddRange(index_path.Indices);
                return xpath;
            }
            else
            {
                return null;
            }
        }

        public class Limits
        {
            public int Low;
            public int High;
        }
        public static XmlElement GetPathElement(XmlElement tag_element, ref List<Limits> limits, ref  List<int> indices, PathComponent path, ConstantLookup constants, PathComponent ignore_prefix = null)
        {

            XmlElement elem = tag_element;
            if (path.Parent != null)
            {
                if (ignore_prefix == null || !ignore_prefix.Equals(path.Parent))
                {

                    elem = GetPathElement(tag_element, ref limits,ref indices, path.Parent, constants, ignore_prefix);
                    if (elem == null) return null;
                }
            }
            if (path is MemberComponent member_path)
            {
                if (!(elem.SelectSingleNode("./if:Member[@Name='" + member_path.Name + "'] | ./if:Sections/if:Section/if:Member[@Name='" + member_path.Name + "']", XMLUtil.nameSpaces) is XmlElement child_elem))
                {
                    throw new Exception("Unable to find path " + member_path + " in element " + (elem?.GetAttribute("Name") ?? "<Unknown>"));
                }
                DataType type = DataTypeParser.Parse(child_elem.GetAttribute("Datatype"), out string _);
                if (type is ARRAY array)
                {
                    limits.AddRange(array.Limits.Select(x => new Limits() { Low = x.LowLimit.ResolveInt(constants), High = x.HighLimit.ResolveInt(constants) }));
                }
                return child_elem;
            }
            else if (path is IndexComponent index_path)
            {
                indices.AddRange(index_path.Indices);
                return elem;
            }
            else
            {
                return null;
            }

        }

        public abstract class ReadWriteBlock
        {
            public abstract XmlDocument ReadBlock(string name);
            public abstract void WriteBlock(string name, XmlDocument doc);
        }
        public static void WriteTags(TagCacheTree tag_cache, ReadWriteBlock blocks, ConstantLookup constants)
        {

            XmlDocument doc = null;
            XmlElement static_section = null;
            PathComponent top_path = null;
            foreach (Tag tag in tag_cache)
            {

                if (top_path == null || top_path.MatchPrefix(tag.Path) != 0)
                {
                    if (top_path != null)
                    {
                        blocks.WriteBlock(top_path.ToString(), doc);
                    }
                    top_path = tag.Path;
                    while (top_path.Parent != null) top_path = top_path.Parent;
                    doc = blocks.ReadBlock(top_path.ToString());
                    XmlNode block_attrs = doc.SelectSingleNode("/Document/SW.Blocks.GlobalDB/AttributeList", XMLUtil.nameSpaces);
                    if (block_attrs == null) return;

                    XmlNode block_name_node = block_attrs.SelectSingleNode("Name", XMLUtil.nameSpaces);
                    if (block_name_node == null) throw new XmlException("Missing block name");
                    string block_name = block_name_node.InnerText;
                    static_section = block_attrs.SelectSingleNode("./Interface/if:Sections/if:Section[@Name='Static']", XMLUtil.nameSpaces) as XmlElement;
                    if (static_section == null) throw new XmlException("Missing static section of block");
                    top_path = new MemberComponent(block_name, new STRUCT());
                }

                List<Limits> limits = new List<Limits>();
                List<int> indices = new List<int>();
                XmlElement elem = GetPathElement(static_section, ref limits, ref indices, tag.Path, constants, top_path);
                string indices_str = string.Join(",", indices.Select(x => x.ToString()));
                XmlElement subelement;
                if (tag.Comment != null || tag.StartValue != null)
                {
                    if (indices.Count > 0)
                    {
                        subelement = elem.SelectSingleNode("if:Subelement[@Path=\"" + indices_str + "\"]", XMLUtil.nameSpaces) as XmlElement;
                        if (subelement == null)
                        {
                            subelement = doc.CreateElement("Subelement", XMLUtil.InterfaceNS);
                            subelement.SetAttribute("Path", indices_str);
                        }
                    }
                    else
                    {
                        subelement = elem;
                    }
                    if (tag.Comment != null)
                    {
                        XmlElement comment = subelement.SelectSingleNode("if:Comment", XMLUtil.nameSpaces) as XmlElement;
                        if (comment == null)
                        {
                            comment = doc.CreateElement("Comment", XMLUtil.InterfaceNS);
                            subelement.AppendChild(comment);
                        }

                        foreach (string lang in tag.Comment.Cultures)
                        {
                            XmlElement multi = comment.SelectSingleNode("if:MultiLanguageText[@Lang=\"" + lang + "\"]", XMLUtil.nameSpaces) as XmlElement;
                            if (multi == null)
                            {
                                multi = doc.CreateElement("if:MultiLanguageText", XMLUtil.InterfaceNS);
                                multi.SetAttribute("Lang", lang);
                                comment.AppendChild(multi);
                            }
                            multi.InnerText = tag.Comment[lang];
                        }
                    }
                    if (tag.StartValue != null)
                    {
                        XmlElement startvalue = subelement.SelectSingleNode("if:StartValue", XMLUtil.nameSpaces) as XmlElement;
                        if (startvalue == null)
                        {
                            startvalue = doc.CreateElement("StartValue", XMLUtil.InterfaceNS);
                            subelement.AppendChild(startvalue);
                        }
                        startvalue.InnerText = PlcValue.ValueToString(tag.StartValue);
                    }

                }

            }
            if (top_path != null)
            {
                blocks.WriteBlock(top_path.ToString(), doc);
            }
        }
    }
}
