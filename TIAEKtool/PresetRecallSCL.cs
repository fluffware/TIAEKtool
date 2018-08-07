using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TIAEKtool.Properties;

namespace TIAEKtool
{
    public class PresetRecallSCL : PresetSCL
    {
      
        public PresetRecallSCL(string block_name, string value_type_name, string enable_type_name, XmlDocument doc) : base(block_name)
        {
            if (doc == null)
            {
                doc = new XmlDocument();
                doc.LoadXml(Resources.InitialPresetRecallSCL);
            }
            SetDocument(doc);
            
            XmlElement value_param = doc.SelectSingleNode("/Document/SW.Blocks.FC/AttributeList/Interface/if:Sections/if:Section[@Name='Input']/if:Member[@Name='Value']",nsmgr) as XmlElement;
            if (value_param == null) throw new Exception("No input parameter named Value");
            value_param.SetAttribute("Datatype", "\"" + value_type_name + "\"");

            XmlElement enable_param = doc.SelectSingleNode("/Document/SW.Blocks.FC/AttributeList/Interface/if:Sections/if:Section[@Name='Input']/if:Member[@Name='Enable']", nsmgr) as XmlElement;
            if (enable_param == null) throw new Exception("No input parameter named Index");
            enable_param.SetAttribute("Datatype", "\"" + enable_type_name + "\"");

        }



        
        public void AddRecall(PathComponent comp)
        {
            builder.Push(structured_text);

            // If <enable> THEN
            builder.Token("IF");
            builder.Blank();

            builder.LocalVariable();
            builder.Down();
            builder.Symbol();
            builder.Down();

            builder.Component("Enable");
            
            builder.Token(".");
            builder.SymbolAddComponents(comp);
            builder.Pop(); // End symbol
            builder.Pop(); // End LocalVariable

            builder.Blank();
            builder.Token("THEN");
            builder.NewLine();

            builder.Blank(4); // Indent
            // <path>
            builder.GlobalVariable();
            builder.Down();
            builder.Symbol();
            builder.Down();
            builder.SymbolAddComponents(comp);
            builder.Pop();
            builder.Pop();
            
            // :=
            builder.Blank();
            builder.Token(":=");
            builder.Blank();

            // "Value.<path> 
            builder.LocalVariable();
            builder.Down();
            builder.Symbol();
            builder.Down();

            builder.Component("Value");

            builder.Token(".");
            builder.SymbolAddComponents(comp);
            builder.Pop(); // End symbol
            builder.Pop(); // End GlobalVariable

            builder.Token(";");
            builder.NewLine();

            builder.Token("END_IF");
            builder.Token(";");
            builder.NewLine();
        }
        
    }
}
