using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TIAEktool.Plc.Types;

namespace TIAEKtool.Plc
{
    public class TagCacheXML : ITagCache
    {
        readonly BlockXmlCache block_cache;
        readonly ConstantLookup constants;

        public TagCacheXML(BlockXmlCache block_cache,
                    ConstantLookup constants)
        {
            this.block_cache = block_cache;
            this.constants = constants;
        }

        protected  static Tag ParseTag(XmlElement tag_elem, PathComponent path)
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
            return tag;
        }

        public class Limits
        {
            public int Low;
            public int High;
        }
        protected static XmlElement GetPathElement(XmlElement tag_element, List<Limits> limits, List<int> indices,
            PathComponent path, ConstantLookup constants, PathComponent ignore_prefix = null, bool create = false)
        {

            XmlElement elem = tag_element;
            if (path.Parent != null)
            {
                if (ignore_prefix == null || !ignore_prefix.Equals(path.Parent))
                {

                    elem = GetPathElement(tag_element, limits, indices, path.Parent, constants, ignore_prefix,create);
                    if (elem == null) return null;
                }
            }
            if (path is MemberComponent member_path)
            {
                if (!(elem.SelectSingleNode("./if:Member[@Name='" + member_path.Name + "'] | ./if:Sections/if:Section/if:Member[@Name='" + member_path.Name + "']", XMLUtil.nameSpaces) is XmlElement child_elem))
                {
                    if (create)
                    {
                        child_elem = elem.OwnerDocument.CreateElement("Member", XMLUtil.InterfaceNS);
                        child_elem.SetAttribute("Name", member_path.Name);
                        child_elem.SetAttribute("Datatype", member_path.Type.ToString());
                        elem.AppendChild(child_elem);
                    }
                    else
                    {
                        return null;
                        //throw new Exception("Unable to find path " + member_path + " in element " + (elem?.GetAttribute("Name") ?? "<Unknown>"));
                    }
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

        private XmlElement GetStaticSection(PathComponent path, out XmlDocument doc, out PathComponent block_path)
        {
            block_path = path;
            while (block_path.Parent != null) block_path = block_path.Parent;
            doc = block_cache.GetXML((block_path as MemberComponent).Name);
            XmlNode block_attrs = doc.SelectSingleNode("/Document/SW.Blocks.GlobalDB/AttributeList", XMLUtil.nameSpaces);
            if (block_attrs == null) return null;

            XmlNode block_name_node = block_attrs.SelectSingleNode("Name", XMLUtil.nameSpaces);
            if (block_name_node == null) throw new XmlException("Missing block name");

            if (block_attrs.SelectSingleNode("./Interface/if:Sections/if:Section[@Name='Static']", XMLUtil.nameSpaces)
               is XmlElement static_section)
            {
                return static_section;
            } else {
                throw new XmlException("Missing static section of block"); 
            }

        }

        public void Add(Tag tag)
        {
            XmlElement static_section = GetStaticSection(tag.Path, out var doc, out var block_path);

            List<Limits> limits = new List<Limits>();
            List<int> indices = new List<int>();
            XmlElement tag_elem = GetPathElement(static_section, limits, indices,
             tag.Path, constants, block_path, true);
            string indices_str = string.Join(",", indices.Select(x => x.ToString()));
            XmlElement subelement;
            block_cache.SetChanged(((MemberComponent)block_path).Name);
            if (indices.Count > 0)
            {
                subelement = tag_elem.SelectSingleNode("if:Subelement[@Path=\"" + indices_str + "\"]", XMLUtil.nameSpaces) as XmlElement;
                if (subelement == null)
                {
                    subelement = doc.CreateElement("Subelement", XMLUtil.InterfaceNS);
                    subelement.SetAttribute("Path", indices_str);
                }
            }
            else
            {
                subelement = tag_elem;
            }
            if (tag.Comment != null)
            {
            
               
                if (!(subelement.SelectSingleNode("if:Comment", XMLUtil.nameSpaces) is XmlElement comment))
                {
                    comment = doc.CreateElement("Comment", XMLUtil.InterfaceNS);
                    subelement.AppendChild(comment);
                }

                foreach (string lang in tag.Comment.Cultures)
                {
                    if (!( comment.SelectSingleNode("if:MultiLanguageText[@Lang=\"" + lang + "\"]", XMLUtil.nameSpaces) is XmlElement multi))
                  
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
                if (!(subelement.SelectSingleNode("if:StartValue", XMLUtil.nameSpaces) is XmlElement startvalue))
               
                {
                    startvalue = doc.CreateElement("StartValue", XMLUtil.InterfaceNS);
                    subelement.AppendChild(startvalue);
                }
                startvalue.InnerText = PlcValue.ValueToString(tag.StartValue);
            }
          
           
        }

        public Tag Find(PathComponent path)
        {

            XmlElement static_section = GetStaticSection(path, out var doc, out var block_path);

            List<Limits> limits = new List<Limits>();
            List<int> indices = new List<int>();
            XmlElement tag_elem = GetPathElement(static_section, limits, indices,
             path, constants, block_path);
            if (tag_elem == null) return null;
            string indices_str = string.Join(",", indices.Select(x => x.ToString()));
            XmlElement subelement;
            if (indices.Count > 0)
            {
                subelement = tag_elem.SelectSingleNode("if:Subelement[@Path=\"" + indices_str + "\"]", XMLUtil.nameSpaces) as XmlElement;
                if (subelement == null) return null;
            }
            else
            {
                subelement = tag_elem;
            }
            return ParseTag(subelement, path);
        }

        public IEnumerator<Tag> GetEnumerator(PathComponent prefix)
        {
            throw new NotImplementedException();
        }
    }
}
