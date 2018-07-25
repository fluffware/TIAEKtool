using Siemens.Engineering;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TIAtool;

namespace TIAEKtool
{
    static class TIAutils
    {
        static public XmlDocument ExportPlcBlockXML(PlcBlock block)
        {


            FileInfo path = TempFile.File("export_block_", "xml");

            try
            {
               
                block.Export(path, ExportOptions.WithDefaults);
                XmlDocument doc = new XmlDocument();
                doc.Load(path.ToString());
                return doc;
            }
            finally
            {
                try
                {
                    path.Delete();
                }
                catch (IOException e)
                {
                    Console.WriteLine("Failed to delete temporary file: " + e.Message);
                }
            }
          
        }

        static public void ImportPlcBlockXML(XmlDocument doc, PlcBlockGroup group)
        {


            FileInfo path = TempFile.File("import_block_", "xml");
           
            try {
                doc.Save(path.ToString());
                group.Blocks.Import(path, ImportOptions.Override);
            }
            finally
            {
                try
                {
                    path.Delete();
                }
                catch (IOException e)
                {
                    Console.WriteLine("Failed to delete temporary file: " + e.Message);
                }
            }
        }

        static public XmlDocument ExportScreenPopupXML(ScreenPopup popup)
        { 
            FileInfo path = TempFile.File("export_popup_", "xml");

            try
            {

                popup.Export(path, ExportOptions.WithDefaults);
                XmlDocument doc = new XmlDocument();
                doc.Load(path.ToString());
                return doc;
            }
            finally
            {
                try
                {
                    path.Delete();
                }
                catch (IOException e)
                {
                    Console.WriteLine("Failed to delete temporary file: " + e.Message);
                }
            }

        }

        static public void ImportScreenPopupXML(XmlDocument doc, ScreenPopupFolder folder)
        {


            FileInfo path = TempFile.File("import_popup_", "xml");

            try
            {
                doc.Save(path.ToString());
                folder.ScreenPopups.Import(path, ImportOptions.Override);
            }
            finally
            {
                try
                {
                    path.Delete();
                }
                catch (IOException e)
                {
                    Console.WriteLine("Failed to delete temporary file: " + e.Message);
                }
            }
        }
    }
}
