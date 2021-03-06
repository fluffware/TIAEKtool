﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TIAEKtool
{
    class XMLUtil
    {
        static readonly public XmlNamespaceManager nameSpaces;
        public MessageLog Log = null;
        public const string InterfaceNS = "http://www.siemens.com/automation/Openness/SW/Interface/v3";
        static XMLUtil()
        {
            NameTable nt = new NameTable();
            nameSpaces = new XmlNamespaceManager(nt);
            nameSpaces.AddNamespace("if", XMLUtil.InterfaceNS);
        }

        static public void SimpleValue(XmlWriter w, string name, string value, bool readOnly = false)
        {
            w.WriteStartElement(name);
            if (readOnly)
            {
                w.WriteAttributeString("ReadOnly", "true");
            }
            w.WriteString(value);
            w.WriteEndElement();
        }
        public const int SystemDefined = 1;
        public const int Informative = 2;

        static public void WriteAttribute(XmlWriter w, string type, string name, string value, int flags = 0)
        {
            w.WriteStartElement(type);
            w.WriteAttributeString("Name", name);
            if ((flags & SystemDefined) != 0)
            {
                w.WriteAttributeString("SystemDefined", "true");
            }
            if ((flags & Informative) != 0)
            {
                w.WriteAttributeString("Informative", "true");
            }
            w.WriteString(value);
            w.WriteEndElement();
        }
        static public void IntegerAttribute(XmlWriter w, string name, int value, int flags = 0)
        {
            WriteAttribute(w, "IntegerAttribute", name, value.ToString(), flags);
        }
        static public void BooleanAttribute(XmlWriter w, string name, bool value, int flags = 0)
        {
            WriteAttribute(w, "BooleanAttribute", name, value ? "true" : "false", flags);
        }

        static public void Link(XmlWriter w, string target_type, string name, string target_id = "@OpenLink")
        {
            w.WriteStartElement(target_type);
            w.WriteAttributeString("TargetID", target_id);
            w.WriteStartElement("Name");
            w.WriteString(name);
            w.WriteEndElement(); // Name
            w.WriteEndElement();
        }


        public static void CollectID(XmlElement top, IntSet idset)
        {
            if (top.HasAttribute("ID"))
            {
                string id_str = top.GetAttribute("ID");
                int id;
                if (int.TryParse(id_str, System.Globalization.NumberStyles.HexNumber, null, out id))
                {
                    idset.Add(id);

                }
            }
            foreach (var child in top.ChildNodes)
            {
                if (child is XmlElement)
                {
                    CollectID((XmlElement)child, idset);
                }
            }
        }

        public static void ReplaceID(XmlElement top, IntSet idset)
        {
            if (top.HasAttribute("ID"))
            {
                int id = idset.LowestFree();
                idset.Add(id);
                top.SetAttribute("ID", id.ToString("X"));
            }
            foreach (var child in top.ChildNodes)
            {
                if (child is XmlElement)
                {
                    ReplaceID((XmlElement)child, idset);
                }
            }
        }
    }
}
