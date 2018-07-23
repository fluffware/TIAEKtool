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
            PlcBlockGroup top_group = (PlcBlockGroup)node;
            resultGroup = top_group.Groups.Find("Preset");
            if (resultGroup == null)
            {
                resultGroup = top_group.Groups.Create("Preset");
            }
        }


        protected void FormClosingEventHandler(object sender, FormClosingEventArgs e)
        {
            parser.CancelParse();
        }

        public void HandleTag(object source, TagParser.HandleTagEventArgs ev)
        {
            PresetTag preset = new PresetTag() { tagPath = ev.Path };
           
            foreach (string c in ev.Comment.Cultures)
            {
                PresetCommentParser.Parse(ev.Comment[c], c, preset);
             
            }
            if (preset.labels != null)
            {
                presetList.AddTag(preset);
            }
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
            Dictionary<string, List<PresetTag>> tag_groups = new Dictionary<string, List<PresetTag>>();
            foreach (PresetTagList.Row r in presetList)
            {
                List<PresetTag> tags;
                if (!tag_groups.TryGetValue(r.Tag.presetGroup, out tags))
                {
                    tags = new List<PresetTag>();

                    tag_groups[r.Tag.presetGroup] = tags;
                }
                tags.Add(r.Tag);
            }


            // Create databases for all groups
            foreach (string group_name in tag_groups.Keys)
            {

                string db_name = "sDB_Preset_" + group_name;
                var tags = tag_groups[group_name];
                try
                {
                  
                    PresetDB db;
                    PlcBlock block = resultGroup.Blocks.Find(db_name);
                    Constant preset_count = new GlobalConstant("PresetCount_" + group_name);
                    if (block != null)
                    {
                        XmlDocument block_doc = TIAutils.ExportPlcBlockXML(block);
                        db = new PresetDB(db_name, preset_count, block_doc);
                    }
                    else
                    {
                        db = new PresetDB(db_name, preset_count);
                    }
                    foreach (var tag in tags)
                    {
                        db.AddPath(tag.tagPath, tag.labels, tag.defaultValue);
                    }
                    TIAutils.ImportPlcBlockXML(db.Document, resultGroup);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to update preset DB: " + ex.Message);
                    return;
                }

                try
                {
                    string block_name = "PresetStore_" + group_name;
                    PresetSCL scl = new PresetSCL(block_name, db_name);
                    foreach (var tag in tags)
                    {
                        if (!tag.noStore)
                        {
                            scl.AddStore(tag.tagPath);
                        }
                    }
                    TIAutils.ImportPlcBlockXML(scl.Document, resultGroup);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to update preset store SCL block: " + ex.Message);
                    return;
                }

                try
                {
                    string block_name = "PresetRecall_" + group_name;
                    PresetSCL scl = new PresetSCL(block_name, db_name);
                    foreach (var tag in tags)
                    {

                    }
                    TIAutils.ImportPlcBlockXML(scl.Document, resultGroup);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to update preset recall SCL block: " + ex.Message);
                    return;
                }
            }
        }
    }

 
}
