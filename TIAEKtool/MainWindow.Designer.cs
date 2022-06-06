namespace TIAtool
{
    partial class MainForm
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
                tiaThread.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tIAPortalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startTIAOpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startSyncTIAOpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectTreeView = new System.Windows.Forms.TreeView();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btn_connect = new System.Windows.Forms.Button();
            this.btn_disconnect = new System.Windows.Forms.Button();
            this.btn_preset = new System.Windows.Forms.Button();
            this.btn_preset_import = new System.Windows.Forms.Button();
            this.btn_hmi_tags = new System.Windows.Forms.Button();
            this.btn_copy = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.loadPresetList = new System.Windows.Forms.OpenFileDialog();
            this.btn_compile_download = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.tIAPortalToolStripMenuItem,
            this.languageToolStripMenuItem,
            this.debugToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(392, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(97, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.QuitToolStripMenuItem_Click);
            // 
            // tIAPortalToolStripMenuItem
            // 
            this.tIAPortalToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.disconnectToolStripMenuItem,
            this.browseToolStripMenuItem});
            this.tIAPortalToolStripMenuItem.Name = "tIAPortalToolStripMenuItem";
            this.tIAPortalToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.tIAPortalToolStripMenuItem.Text = "TIAPortal";
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.connectToolStripMenuItem.Text = "Connect";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.ConnectToolStripMenuItemClick);
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.disconnectToolStripMenuItem.Text = "Disconnect";
            this.disconnectToolStripMenuItem.Click += new System.EventHandler(this.DisconnectToolStripMenuItemClick);
            // 
            // browseToolStripMenuItem
            // 
            this.browseToolStripMenuItem.Name = "browseToolStripMenuItem";
            this.browseToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.browseToolStripMenuItem.Text = "Browse";
            this.browseToolStripMenuItem.Click += new System.EventHandler(this.BrowseToolStripMenuItem_Click);
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            this.languageToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.languageToolStripMenuItem.Text = "Language";
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startTIAOpToolStripMenuItem,
            this.startSyncTIAOpToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // startTIAOpToolStripMenuItem
            // 
            this.startTIAOpToolStripMenuItem.Name = "startTIAOpToolStripMenuItem";
            this.startTIAOpToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.startTIAOpToolStripMenuItem.Text = "Start TIA op";
            this.startTIAOpToolStripMenuItem.Click += new System.EventHandler(this.StartTIAOpToolStripMenuItem_Click);
            // 
            // startSyncTIAOpToolStripMenuItem
            // 
            this.startSyncTIAOpToolStripMenuItem.Name = "startSyncTIAOpToolStripMenuItem";
            this.startSyncTIAOpToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.startSyncTIAOpToolStripMenuItem.Text = "Start sync TIA op";
            this.startSyncTIAOpToolStripMenuItem.Click += new System.EventHandler(this.StartSyncTIAOpToolStripMenuItemClick);
            // 
            // projectTreeView
            // 
            this.projectTreeView.CheckBoxes = true;
            this.projectTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.projectTreeView.Location = new System.Drawing.Point(3, 3);
            this.projectTreeView.Name = "projectTreeView";
            this.projectTreeView.Size = new System.Drawing.Size(274, 357);
            this.projectTreeView.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btn_connect);
            this.flowLayoutPanel1.Controls.Add(this.btn_disconnect);
            this.flowLayoutPanel1.Controls.Add(this.btn_preset);
            this.flowLayoutPanel1.Controls.Add(this.btn_preset_import);
            this.flowLayoutPanel1.Controls.Add(this.btn_hmi_tags);
            this.flowLayoutPanel1.Controls.Add(this.btn_copy);
            this.flowLayoutPanel1.Controls.Add(this.btn_compile_download);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(283, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(106, 357);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // btn_connect
            // 
            this.btn_connect.Location = new System.Drawing.Point(3, 3);
            this.btn_connect.Name = "btn_connect";
            this.btn_connect.Size = new System.Drawing.Size(100, 23);
            this.btn_connect.TabIndex = 0;
            this.btn_connect.Text = "Connect";
            this.btn_connect.UseVisualStyleBackColor = true;
            this.btn_connect.Click += new System.EventHandler(this.BtnConnectClick);
            // 
            // btn_disconnect
            // 
            this.btn_disconnect.Location = new System.Drawing.Point(3, 32);
            this.btn_disconnect.Name = "btn_disconnect";
            this.btn_disconnect.Size = new System.Drawing.Size(100, 23);
            this.btn_disconnect.TabIndex = 1;
            this.btn_disconnect.Text = "Disconnect";
            this.btn_disconnect.UseVisualStyleBackColor = true;
            this.btn_disconnect.Click += new System.EventHandler(this.BtnDisconnectClick);
            // 
            // btn_preset
            // 
            this.btn_preset.Location = new System.Drawing.Point(3, 61);
            this.btn_preset.Name = "btn_preset";
            this.btn_preset.Size = new System.Drawing.Size(100, 23);
            this.btn_preset.TabIndex = 2;
            this.btn_preset.Text = "Preset";
            this.btn_preset.UseVisualStyleBackColor = true;
            this.btn_preset.Click += new System.EventHandler(this.BtnPresetClick);
            // 
            // btn_preset_import
            // 
            this.btn_preset_import.Location = new System.Drawing.Point(3, 90);
            this.btn_preset_import.Name = "btn_preset_import";
            this.btn_preset_import.Size = new System.Drawing.Size(100, 23);
            this.btn_preset_import.TabIndex = 5;
            this.btn_preset_import.Text = "Import Preset";
            this.btn_preset_import.UseVisualStyleBackColor = true;
            this.btn_preset_import.Click += new System.EventHandler(this.BtnPresetImportClick);
            // 
            // btn_hmi_tags
            // 
            this.btn_hmi_tags.Location = new System.Drawing.Point(3, 119);
            this.btn_hmi_tags.Name = "btn_hmi_tags";
            this.btn_hmi_tags.Size = new System.Drawing.Size(100, 23);
            this.btn_hmi_tags.TabIndex = 3;
            this.btn_hmi_tags.Text = "HMI tags";
            this.btn_hmi_tags.UseVisualStyleBackColor = true;
            this.btn_hmi_tags.Click += new System.EventHandler(this.BtnHmiTagsClick);
            // 
            // btn_copy
            // 
            this.btn_copy.Location = new System.Drawing.Point(3, 148);
            this.btn_copy.Name = "btn_copy";
            this.btn_copy.Size = new System.Drawing.Size(100, 23);
            this.btn_copy.TabIndex = 4;
            this.btn_copy.Text = "Copy";
            this.btn_copy.UseVisualStyleBackColor = true;
            this.btn_copy.Click += new System.EventHandler(this.BtnCopyClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.projectTreeView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(392, 363);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // loadPresetList
            // 
            this.loadPresetList.DefaultExt = "xlsx";
            this.loadPresetList.FileName = "Preset.xlsx";
            // 
            // btn_compile_download
            // 
            this.btn_compile_download.Location = new System.Drawing.Point(3, 177);
            this.btn_compile_download.Name = "btn_compile_download";
            this.btn_compile_download.Size = new System.Drawing.Size(100, 23);
            this.btn_compile_download.TabIndex = 6;
            this.btn_compile_download.Text = "Compl/Dld";
            this.btn_compile_download.UseVisualStyleBackColor = true;
            this.btn_compile_download.Click += new System.EventHandler(this.BtnCompileDownloadClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 387);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "TIA EK tool";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tIAPortalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.TreeView projectTreeView;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btn_connect;
        private System.Windows.Forms.Button btn_disconnect;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btn_preset;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.Button btn_hmi_tags;
        private System.Windows.Forms.Button btn_copy;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startTIAOpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startSyncTIAOpToolStripMenuItem;
        private System.Windows.Forms.Button btn_preset_import;
        private System.Windows.Forms.OpenFileDialog loadPresetList;
        private System.Windows.Forms.Button btn_compile_download;
    }
}

