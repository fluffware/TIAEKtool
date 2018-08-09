namespace TIAEKtool
{
    partial class PresetGenerate
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
            this.presetListView = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.writeButton = new System.Windows.Forms.Button();
            this.presetListColumnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.presetListColumnLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.presetListColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.presetListColumnNoStore = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.presetListColumnValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.presetListColumnUnit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.presetListColumnPrecision = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.presetListView)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // presetListView
            // 
            this.presetListView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.presetListView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.presetListView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.presetListColumnPath,
            this.presetListColumnLabel,
            this.presetListColumnType,
            this.presetListColumnNoStore,
            this.presetListColumnValue,
            this.presetListColumnUnit,
            this.presetListColumnPrecision});
            this.presetListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.presetListView.Location = new System.Drawing.Point(3, 53);
            this.presetListView.Name = "presetListView";
            this.presetListView.Size = new System.Drawing.Size(794, 394);
            this.presetListView.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.presetListView, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 88.66666F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.writeButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(794, 44);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // writeButton
            // 
            this.writeButton.AutoSize = true;
            this.writeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.writeButton.Location = new System.Drawing.Point(3, 3);
            this.writeButton.Name = "writeButton";
            this.writeButton.Size = new System.Drawing.Size(75, 23);
            this.writeButton.TabIndex = 0;
            this.writeButton.Text = "Write";
            this.writeButton.UseVisualStyleBackColor = true;
            this.writeButton.Click += new System.EventHandler(this.writeButton_Click);
            // 
            // presetListColumnPath
            // 
            this.presetListColumnPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.presetListColumnPath.DataPropertyName = "Path";
            this.presetListColumnPath.HeaderText = "Tag path";
            this.presetListColumnPath.Name = "presetListColumnPath";
            this.presetListColumnPath.ReadOnly = true;
            this.presetListColumnPath.Width = 75;
            // 
            // presetListColumnLabel
            // 
            this.presetListColumnLabel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.presetListColumnLabel.DataPropertyName = "Label";
            this.presetListColumnLabel.HeaderText = "Label";
            this.presetListColumnLabel.Name = "presetListColumnLabel";
            this.presetListColumnLabel.ReadOnly = true;
            this.presetListColumnLabel.Width = 58;
            // 
            // presetListColumnType
            // 
            this.presetListColumnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.presetListColumnType.DataPropertyName = "Type";
            this.presetListColumnType.HeaderText = "Type";
            this.presetListColumnType.Name = "presetListColumnType";
            this.presetListColumnType.ReadOnly = true;
            this.presetListColumnType.Width = 56;
            // 
            // presetListColumnNoStore
            // 
            this.presetListColumnNoStore.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.presetListColumnNoStore.DataPropertyName = "NoStore";
            this.presetListColumnNoStore.HeaderText = "No store";
            this.presetListColumnNoStore.Name = "presetListColumnNoStore";
            this.presetListColumnNoStore.ReadOnly = true;
            this.presetListColumnNoStore.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.presetListColumnNoStore.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.presetListColumnNoStore.Width = 72;
            // 
            // presetListColumnValue
            // 
            this.presetListColumnValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.presetListColumnValue.DataPropertyName = "DefaultValue";
            this.presetListColumnValue.HeaderText = "Value";
            this.presetListColumnValue.Name = "presetListColumnValue";
            this.presetListColumnValue.ReadOnly = true;
            this.presetListColumnValue.Width = 59;
            // 
            // presetListColumnUnit
            // 
            this.presetListColumnUnit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.presetListColumnUnit.DataPropertyName = "Unit";
            this.presetListColumnUnit.HeaderText = "Unit";
            this.presetListColumnUnit.Name = "presetListColumnUnit";
            this.presetListColumnUnit.ReadOnly = true;
            this.presetListColumnUnit.Width = 51;
            // 
            // presetListColumnPrecision
            // 
            this.presetListColumnPrecision.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.presetListColumnPrecision.DataPropertyName = "Precision";
            this.presetListColumnPrecision.HeaderText = "Precision";
            this.presetListColumnPrecision.Name = "presetListColumnPrecision";
            this.presetListColumnPrecision.ReadOnly = true;
            this.presetListColumnPrecision.Width = 75;
            // 
            // PresetGenerate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PresetGenerate";
            this.Text = "Generate Presets";
            this.Load += new System.EventHandler(this.PresetGenerate_Load);
            ((System.ComponentModel.ISupportInitialize)(this.presetListView)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridView presetListView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button writeButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn presetListColumnPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn presetListColumnLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn presetListColumnType;
        private System.Windows.Forms.DataGridViewCheckBoxColumn presetListColumnNoStore;
        private System.Windows.Forms.DataGridViewTextBoxColumn presetListColumnValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn presetListColumnUnit;
        private System.Windows.Forms.DataGridViewTextBoxColumn presetListColumnPrecision;
    }
}