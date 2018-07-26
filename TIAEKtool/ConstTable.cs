using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TIAEKtool
{
   
    class ConstTable
    {
         public class Constant
         {
             public string Name { get; protected set; }
             public object Value { get; protected set; }
             public string Comment { get; protected set; } 

             public Constant(string name, Object value, string comment)
             {
                 Name = name;
                 Value = value;
                 Comment = comment;
             }
         }

      
        public static FileInfo buildFile(string table_name, IEnumerable<Constant> constants)
         {
            
            XmlWriterSettings settings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Document, 
                Encoding = Encoding.UTF8, 
                Indent = true };
            FileInfo file;
            using (Stream stream = TempFile.Open("AlarmConst","xml", out file))
            {
                int id = 0;
                XmlWriter w = XmlWriter.Create(stream, settings);
                w.WriteStartDocument();
                w.WriteStartElement("Document");

                w.WriteStartElement("DocumentInfo");
                w.WriteEndElement(); // DocumentInfo
                w.WriteStartElement("SW.Tags.PlcTagTable");
                w.WriteAttributeString("ID", (id++).ToString());
                w.WriteStartElement("AttributeList");
                XMLUtil.SimpleValue(w, "Name", table_name);
                w.WriteEndElement(); // AttributeList



                w.WriteStartElement("ObjectList");
                foreach (Constant c in constants)
                {

                    w.WriteStartElement("SW.Tags.PlcUserConstant");
                    w.WriteAttributeString("ID", (id++).ToString());
                    w.WriteAttributeString("CompositionName", "UserConstants");
                    w.WriteStartElement("AttributeList");
                    XMLUtil.SimpleValue(w, "DataTypeName", "Int");
                    XMLUtil.SimpleValue(w, "Name", c.Name);
                    XMLUtil.SimpleValue(w, "Value", c.Value.ToString());
                    w.WriteEndElement(); // AttributeList

                  

                    w.WriteStartElement("ObjectList");
                    w.WriteStartElement("MultilingualText");
                    w.WriteAttributeString("ID", (id++).ToString());
                    w.WriteAttributeString("CompositionName", "Comment");

                    w.WriteStartElement("ObjectList");
                    w.WriteStartElement("MultilingualTextItem");
                    w.WriteAttributeString("ID", (id++).ToString());
                    w.WriteAttributeString("CompositionName", "Items");

                    w.WriteStartElement("AttributeList");
                    XMLUtil.SimpleValue(w, "Culture", "sv-SE");
                    XMLUtil.SimpleValue(w, "Text", c.Comment);
                   
                    w.WriteEndElement(); // AttributeList
                    w.WriteEndElement(); // MultilingualTextItem
                    w.WriteEndElement(); // ObjectList
                    w.WriteEndElement(); // MultilingualText
                    w.WriteEndElement(); // ObjectList

                    w.WriteEndElement(); // SW.Tags.PlcUserConstant
                }

               
                w.WriteEndElement(); // ObjectList
                w.WriteEndElement(); // SW.Tags.PlcUserConstant
                w.WriteEndElement(); // Document
                w.Close();
            }
            return file;
         }

        static private void readObjectList(XmlReader r, out string comment)
        {
            comment = null;
            if (!r.ReadToDescendant("MultilingualText")) return;
            if (!r.ReadToDescendant("Value")) return;
            comment = r.ReadElementContentAsString();
        }

        static private void readAttributeList(XmlReader r, out string name, out string value_str, out string type_str)
        {
            name = null;
            value_str = null;
            type_str = null;
            r.Read(); // Move to first child
            while (r.Read())
            {
                if (r.IsStartElement())
                {
                    if (r.LocalName == "Name")
                    {
                        name = r.ReadElementContentAsString();
                    }
                    else if (r.LocalName == "Value")
                    {
                        value_str = r.ReadElementContentAsString();
                    }
                    else if (r.LocalName == "DataTypeName")
                    {
                        type_str = r.ReadElementContentAsString();
                    }
                    else {
                        r.Skip();
                    }
                }
            }
        
        }
      
        static private void readConstant(XmlReader r, List<Constant> constants)
        {

          
            string comment = null;
            string name = null;
            string value_str = null;
            string type = null;
            r.Read(); // Move to first child
            while(r.Read())
            {
                if (r.IsStartElement())
                {
                    if (r.LocalName == "ObjectList")
                    {
                        readObjectList(r.ReadSubtree(), out comment);
                    }
                    else if (r.LocalName == "AttributeList")
                    {
                        readAttributeList(r.ReadSubtree(), out name, out value_str, out type);
                    }
                    else
                    {
                        r.Skip();
                    }
                }
            }
            if (name != null && value_str != null && type != null)
            {
                if (type == "Int")
                {
                    object value = int.Parse(value_str);
                    constants.Add(new Constant(name, value, comment));
                }
            }
        }
        static public List<Constant> getConstants(FileInfo file)
        {
            List<Constant> constants = new List<Constant>();

            XmlReaderSettings settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Document,
            };
            using (Stream stream = file.Open(FileMode.Open))
            {
                XmlReader r = XmlReader.Create(stream, settings);
                if (!r.ReadToFollowing("Document")) throw new XmlException("Top node Document not found");
                if (!r.ReadToDescendant("SW.Tags.PlcTagTable")) throw new XmlException("SW.Tags.PlcTagTable not found");
                if (!r.ReadToDescendant("SW.Tags.PlcUserConstant")) throw new XmlException("SW.Tags.PlcUserConstant not found");
                do
                {
                    if (r.GetAttribute("CompositionName") == "UserConstants")
                    {
                     readConstant(r.ReadSubtree(), constants);
                    }
                } while (r.ReadToNextSibling("SW.Tags.PlcUserConstant"));
            }
            return constants;
        }
    }
}
