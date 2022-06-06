using Siemens.Engineering;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.Hmi.TextGraphicList;
using Siemens.Engineering.HW;
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
        static XmlDocument LoadDoc(string file_name)
        {
            XmlDocument doc = new XmlDocument();
            using (var fs = File.Open(file_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                doc.Load(fs);
            }
            return doc;
        }

        static public XmlDocument ExportPlcBlockXML(PlcBlock block)
        {


            FileInfo path = TempFile.File("export_block_", "xml");

            try
            {
               
                block.Export(path, ExportOptions.WithDefaults);
                XmlDocument doc = LoadDoc(path.ToString());
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
                XmlWriter writer = XmlWriter.Create(path.ToString());
                doc.Save(writer);
                writer.Close();
               
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
                XmlDocument doc = LoadDoc(path.ToString());
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
                XmlWriter writer = XmlWriter.Create(path.ToString());
                doc.Save(writer);
                writer.Close();
                
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
                XmlDocument doc = LoadDoc(path.ToString());
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
                XmlWriter writer = XmlWriter.Create(path.ToString());
                doc.Save(writer);
                writer.Close();
               
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

        static public XmlDocument ExportScreenTemplateXML(ScreenTemplate template)
        {
            FileInfo path = TempFile.File("export_template_", "xml");

            try
            {

                template.Export(path, ExportOptions.WithDefaults);
                XmlDocument doc = LoadDoc(path.ToString());
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

        static public void ImportScreenTemplateXML(XmlDocument doc, ScreenTemplateFolder folder)
        {


            FileInfo path = TempFile.File("import_template_", "xml");

            try
            {
                XmlWriter writer = XmlWriter.Create(path.ToString());
                doc.Save(writer);
                writer.Close();
               
                folder.ScreenTemplates.Import(path, ImportOptions.Override);
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
                XmlDocument doc = LoadDoc(path.ToString());
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
                XmlWriter writer = XmlWriter.Create(path.ToString());
                doc.Save(writer);
                writer.Close();
               
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

        static public XmlDocument ExportTextListXML(TextList text_list)
        {
            FileInfo path = TempFile.File("export_text_list_", "xml");

            try
            {

                text_list.Export(path, ExportOptions.WithDefaults);
                XmlDocument doc = LoadDoc(path.ToString());
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

        static public void ImportTextListXML(XmlDocument doc, TextListComposition text_lists)
        {


            FileInfo path = TempFile.File("import_text_list_", "xml");

            try
            {
                XmlWriter writer = XmlWriter.Create(path.ToString());
                doc.Save(writer);
                writer.Close();
                text_lists.Import(path, ImportOptions.Override);
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

        static public string FindParentDeviceName(IEngineeringInstance node)
        {
            while (node != null)
            {
                
                if (node is Device dev)
                {
                    return dev.Name;
                }
                
                node = node.Parent;
             
            }
            return "<unknown>";
        }
    }

}



