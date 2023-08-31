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
    public class PresetStoreSCL : BuildSCL
    {
      
        public PresetStoreSCL(string block_name, string value_type_name, XmlDocument doc) : base(block_name)
        {
            if (doc == null)
            {
                doc = new XmlDocument();
                doc.LoadXml(Resources.InitialPresetStoreSCL);
            }
            SetDocument(doc);
            
            XmlElement value_param = doc.SelectSingleNode("/Document/SW.Blocks.FC/AttributeList/Interface/if:Sections/if:Section[@Name='Output']/if:Member[@Name='Value']",nsmgr) as XmlElement;
            if (value_param == null) throw new Exception("No output parameter named Value");
            value_param.SetAttribute("Datatype", "\"" + value_type_name + "\"");
          
        }


        // 
        public void AddStore(PathComponent comp)
        {
           
            builder.Push(structured_text);

            builder.LocalVariable();
            builder.Down();
            builder.Symbol();
            builder.Down();

            builder.Component("Value");
          

            builder.Token(".");
            builder.SymbolAddComponents(comp);
            builder.Pop(); // End symbol
            builder.Pop(); // End GlobalVariable

            builder.Blank();
            builder.Token(":=");
            builder.Blank();

            builder.GlobalVariable();
            builder.Down();
            builder.Symbol();
            builder.Down();
            builder.SymbolAddComponents(comp);
            builder.Pop();
            builder.Pop();

            builder.Token(";");
            builder.NewLine();

        }
    }
}
