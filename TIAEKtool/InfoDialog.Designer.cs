namespace TIAtool
{
    partial class InfoDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.blockTree = new System.Windows.Forms.TreeView();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.ExportBtn = new System.Windows.Forms.Button();
            this.ImportBtn = new System.Windows.Forms.Button();
            this.DoneBtn = new System.Windows.Forms.Button();
            this.attrList = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.exportFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.importFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.blockTree, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.attrList, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(316, 318);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // blockTree
            // 
            this.blockTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blockTree.Location = new System.Drawing.Point(3, 3);
            this.blockTree.MinimumSize = new System.Drawing.Size(300, 200);
            this.blockTree.Name = "blockTree";
            this.blockTree.Size = new System.Drawing.Size(310, 204);
            this.blockTree.TabIndex = 0;
            this.blockTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.blockTree_AfterSelect);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.ExportBtn);
            this.flowLayoutPanel1.Controls.Add(this.ImportBtn);
            this.flowLayoutPanel1.Controls.Add(this.DoneBtn);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 213);
            this.flowLayoutPanel1.MinimumSize = new System.Drawing.Size(278, 29);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(310, 29);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // ExportBtn
            // 
            this.ExportBtn.Enabled = false;
            this.ExportBtn.Location = new System.Drawing.Point(3, 3);
            this.ExportBtn.Name = "ExportBtn";
            this.ExportBtn.Size = new System.Drawing.Size(75, 23);
            this.ExportBtn.TabIndex = 2;
            this.ExportBtn.Text = "Export";
            this.ExportBtn.UseVisualStyleBackColor = true;
            this.ExportBtn.Click += new System.EventHandler(this.ExportBtn_Click);
            // 
            // ImportBtn
            // 
            this.ImportBtn.Enabled = false;
            this.ImportBtn.Location = new System.Drawing.Point(84, 3);
            this.ImportBtn.Name = "ImportBtn";
            this.ImportBtn.Size = new System.Drawing.Size(75, 23);
            this.ImportBtn.TabIndex = 3;
            this.ImportBtn.Text = "Import";
            this.ImportBtn.UseVisualStyleBackColor = true;
            this.ImportBtn.Click += new System.EventHandler(this.ImportBtn_Click);
            // 
            // DoneBtn
            // 
            this.DoneBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.DoneBtn.Location = new System.Drawing.Point(165, 3);
            this.DoneBtn.Name = "DoneBtn";
            this.DoneBtn.Size = new System.Drawing.Size(75, 23);
            this.DoneBtn.TabIndex = 1;
            this.DoneBtn.Text = "Done";
            this.DoneBtn.UseVisualStyleBackColor = true;
            // 
            // attrList
            // 
            this.attrList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colValue});
            this.attrList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.attrList.Location = new System.Drawing.Point(3, 243);
            this.attrList.MinimumSize = new System.Drawing.Size(300, 200);
            this.attrList.Name = "attrList";
            this.attrList.Size = new System.Drawing.Size(310, 200);
            this.attrList.TabIndex = 2;
            this.attrList.UseCompatibleStateImageBehavior = false;
            this.attrList.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 94;
            // 
            // colValue
            // 
            this.colValue.Text = "Value";
            this.colValue.Width = 223;
            // 
            // exportFileDialog
            // 
            this.exportFileDialog.DefaultExt = "xml";
            this.exportFileDialog.Title = "Export node";
            // 
            // importFileDialog
            // 
            this.importFileDialog.DefaultExt = "xml";
            this.importFileDialog.Title = "Import node";
            // 
            // InfoDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(316, 318);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "InfoDialog";
            this.Text = "InfoDialog";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TreeView blockTree;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button DoneBtn;
        private System.Windows.Forms.Button ExportBtn;
        private System.Windows.Forms.ListView attrList;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colValue;
        private System.Windows.Forms.SaveFileDialog exportFileDialog;
        private System.Windows.Forms.Button ImportBtn;
        private System.Windows.Forms.OpenFileDialog importFileDialog;
    }
}