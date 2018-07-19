using PLC.Types;
using Siemens.Engineering;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace TIAEKtool
{
    public partial class PresetGenerate : Form
    {
        protected PresetTagList presetList;
        TagParser parser;
        PlcBlockGroup resultGroup;
        public PresetGenerate(TiaPortal portal, IEngineeringCompositionOrObject top)
        {
            InitializeComponent();
            FormClosing += FormClosingEventHandler;
            presetListView.AutoGenerateColumns = false;
            presetList = new PresetTagList();
            presetList.Culture = "sv-SE";
            presetListView.DataSource = presetList;

            writeButton.Enabled = false;
            parser = new TagParser(portal);
            parser.HandleTag += HandleTag;
            parser.ParseDone += ParseDone;
            parser.ParseAsync(top);

            IEngineeringCompositionOrObject node = top;
            while (node.Parent is PlcBlockGroup) node = node.Parent;
            resultGroup = node as PlcBlockGroup;
        }


        protected void FormClosingEventHandler(object sender, FormClosingEventArgs e)
        {
            parser.CancelParse();
        }

        public void HandleTag(object source, TagParser.HandleTagEventArgs ev)
        {
            presetList.AddTag(new PresetTag() { tagPath = ev.Path, labels = ev.Comment});
        }

        public void ParseDone(object source, TagParser.ParseDoneEventArgs ev)
        {
            writeButton.Enabled = resultGroup != null;
        }

        private void PresetGenerate_Load(object sender, EventArgs e)
        {

        }


        private void writeButton_Click(object sender, EventArgs e)
        {
            string group_name = "main";
            try
            {
                string block_name = "sDB_Preset_"+group_name;
                PresetDB db;
                PlcBlock block = resultGroup.Blocks.Find(block_name);
                Constant preset_count = new GlobalConstant("PresetCount_"+group_name);
                if (block != null)
                {
                    XmlDocument block_doc = TIAutils.ExportPlcBlockXML(block);
                    db = new PresetDB(block_name, preset_count, block_doc);
                }
                else
                {
                    db = new PresetDB(block_name, preset_count);
                }
                foreach (PresetTagList.Row r in presetList)
                {
                    db.AddPath(r.Tag.tagPath);
                }
                TIAutils.ImportPlcBlockXML(db.Document, resultGroup);
            } catch(Exception ex)
            {
                MessageBox.Show(this, "Failed to update preset DB: "+ex.Message);
                return;
            }

            try
            {
                string block_name = "Preset_" + group_name;
                PresetSCL scl = new PresetSCL(block_name);
                TIAutils.ImportPlcBlockXML(scl.Document, resultGroup);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to update preset SCL block: " + ex.Message);
                return;
            }
        }
    }

 
}
