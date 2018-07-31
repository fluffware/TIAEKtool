using Siemens.Engineering;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
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

        static public XmlDocument ExportPlcTypeXML(PlcType type)
        {


            FileInfo path = TempFile.File("export_type_", "xml");

            try
            {

                type.Export(path, ExportOptions.WithDefaults);
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

        static public void ImportPlcTypeXML(XmlDocument doc, PlcTypeGroup group)
        {


            FileInfo path = TempFile.File("import_type_", "xml");

            try
            {
                doc.Save(path.ToString());
                group.Types.Import(path, ImportOptions.Override);
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

        static public XmlDocument ExportHMITagTableXML(TagTable tag_table)
        {
            FileInfo path = TempFile.File("export_tagtable_", "xml");

            try
            {

                tag_table.Export(path, ExportOptions.WithDefaults);
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

        static public void ImportHMITagTableXML(XmlDocument doc, TagFolder folder)
        {


            FileInfo path = TempFile.File("import_tagtable_", "xml");

            try
            {
                doc.Save(path.ToString());
                folder.TagTables.Import(path, ImportOptions.Override);
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



