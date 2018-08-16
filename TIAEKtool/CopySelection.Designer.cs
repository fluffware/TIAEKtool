namespace TIAEKtool
{
    partial class CopySelection
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
            this.mainHBox = new System.Windows.Forms.TableLayoutPanel();
            this.bottomPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.btn_copy = new System.Windows.Forms.Button();
            this.check_overwrite = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupFrom = new System.Windows.Forms.GroupBox();
            this.fromTreeView = new System.Windows.Forms.TreeView();
            this.groupTo = new System.Windows.Forms.GroupBox();
            this.toTreeView = new System.Windows.Forms.TreeView();
            this.mainHBox.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupFrom.SuspendLayout();
            this.groupTo.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainHBox
            // 
            this.mainHBox.ColumnCount = 1;
            this.mainHBox.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainHBox.Controls.Add(this.bottomPanel, 0, 1);
            this.mainHBox.Controls.Add(this.splitContainer1, 0, 0);
            this.mainHBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainHBox.Location = new System.Drawing.Point(0, 0);
            this.mainHBox.Name = "mainHBox";
            this.mainHBox.RowCount = 2;
            this.mainHBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainHBox.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainHBox.Size = new System.Drawing.Size(800, 450);
            this.mainHBox.TabIndex = 0;
            // 
            // bottomPanel
            // 
            this.bottomPanel.AutoSize = true;
            this.bottomPanel.Controls.Add(this.btn_cancel);
            this.bottomPanel.Controls.Add(this.btn_copy);
            this.bottomPanel.Controls.Add(this.check_overwrite);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.bottomPanel.Location = new System.Drawing.Point(3, 418);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(794, 29);
            this.bottomPanel.TabIndex = 0;
            // 
            // btn_cancel
            // 
            this.btn_cancel.Location = new System.Drawing.Point(716, 3);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 1;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // btn_copy
            // 
            this.btn_copy.Location = new System.Drawing.Point(635, 3);
            this.btn_copy.Name = "btn_copy";
            this.btn_copy.Size = new System.Drawing.Size(75, 23);
            this.btn_copy.TabIndex = 0;
            this.btn_copy.Text = "Copy";
            this.btn_copy.UseVisualStyleBackColor = true;
            this.btn_copy.Click += new System.EventHandler(this.btn_copy_Click);
            // 
            // check_overwrite
            // 
            this.check_overwrite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.check_overwrite.AutoSize = true;
            this.check_overwrite.Location = new System.Drawing.Point(558, 3);
            this.check_overwrite.Name = "check_overwrite";
            this.check_overwrite.Size = new System.Drawing.Size(71, 23);
            this.check_overwrite.TabIndex = 2;
            this.check_overwrite.Text = "Overwrite";
            this.check_overwrite.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupFrom);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupTo);
            this.splitContainer1.Size = new System.Drawing.Size(794, 409);
            this.splitContainer1.SplitterDistance = 208;
            this.splitContainer1.TabIndex = 1;
            // 
            // groupFrom
            // 
            this.groupFrom.Controls.Add(this.fromTreeView);
            this.groupFrom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupFrom.Location = new System.Drawing.Point(0, 0);
            this.groupFrom.Name = "groupFrom";
            this.groupFrom.Size = new System.Drawing.Size(794, 208);
            this.groupFrom.TabIndex = 0;
            this.groupFrom.TabStop = false;
            this.groupFrom.Text = "From";
            // 
            // fromTreeView
            // 
            this.fromTreeView.CheckBoxes = true;
            this.fromTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fromTreeView.Location = new System.Drawing.Point(3, 16);
            this.fromTreeView.Name = "fromTreeView";
            this.fromTreeView.Size = new System.Drawing.Size(788, 189);
            this.fromTreeView.TabIndex = 0;
            // 
            // groupTo
            // 
            this.groupTo.Controls.Add(this.toTreeView);
            this.groupTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupTo.Location = new System.Drawing.Point(0, 0);
            this.groupTo.Name = "groupTo";
            this.groupTo.Size = new System.Drawing.Size(794, 197);
            this.groupTo.TabIndex = 1;
            this.groupTo.TabStop = false;
            this.groupTo.Text = "To";
            // 
            // toTreeView
            // 
            this.toTreeView.CheckBoxes = true;
            this.toTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toTreeView.Location = new System.Drawing.Point(3, 16);
            this.toTreeView.Name = "toTreeView";
            this.toTreeView.Size = new System.Drawing.Size(788, 178);
            this.toTreeView.TabIndex = 0;
            // 
            // CopySelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.mainHBox);
            this.Name = "CopySelection";
            this.Text = "CopySelection";
            this.mainHBox.ResumeLayout(false);
            this.mainHBox.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupFrom.ResumeLayout(false);
            this.groupTo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainHBox;
        private System.Windows.Forms.FlowLayoutPanel bottomPanel;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Button btn_copy;
        private System.Windows.Forms.CheckBox check_overwrite;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupFrom;
        private System.Windows.Forms.TreeView fromTreeView;
        private System.Windows.Forms.GroupBox groupTo;
        private System.Windows.Forms.TreeView toTreeView;
    }
}