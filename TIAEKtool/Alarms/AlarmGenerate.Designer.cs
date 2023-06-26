namespace TIAEKtool
{
    partial class AlarmGenerate
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
                parser.Dispose();
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
            this.alarmListView = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.writeButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.cultureComboBox = new System.Windows.Forms.ComboBox();
            this.saveAlarmList = new System.Windows.Forms.SaveFileDialog();
            this.sinkListView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Order = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.alarmListColumnGroup = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.alarmListColumnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.alarmListColumnLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.alarmListColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.edge = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.delay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.alarmListView)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sinkListView)).BeginInit();
            this.SuspendLayout();
            // 
            // alarmListView
            // 
            this.alarmListView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.alarmListView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.alarmListView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.Order,
            this.alarmListColumnGroup,
            this.alarmListColumnPath,
            this.alarmListColumnLabel,
            this.alarmListColumnType,
            this.edge,
            this.delay});
            this.alarmListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.alarmListView.Location = new System.Drawing.Point(3, 33);
            this.alarmListView.Name = "alarmListView";
            this.alarmListView.Size = new System.Drawing.Size(794, 235);
            this.alarmListView.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.sinkListView, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.alarmListView, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 88.66666F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 178F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.writeButton);
            this.flowLayoutPanel1.Controls.Add(this.exportButton);
            this.flowLayoutPanel1.Controls.Add(this.cultureComboBox);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(794, 24);
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
            this.writeButton.Click += new System.EventHandler(this.WriteButton_Click);
            // 
            // exportButton
            // 
            this.exportButton.AutoSize = true;
            this.exportButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exportButton.Location = new System.Drawing.Point(84, 3);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 1;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // cultureComboBox
            // 
            this.cultureComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cultureComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.flowLayoutPanel1.SetFlowBreak(this.cultureComboBox, true);
            this.cultureComboBox.FormattingEnabled = true;
            this.cultureComboBox.Location = new System.Drawing.Point(165, 3);
            this.cultureComboBox.Name = "cultureComboBox";
            this.cultureComboBox.Size = new System.Drawing.Size(121, 21);
            this.cultureComboBox.TabIndex = 2;
            this.cultureComboBox.SelectedIndexChanged += new System.EventHandler(this.CultureComboBox_SelectedIndexChanged);
            // 
            // saveAlarmList
            // 
            this.saveAlarmList.DefaultExt = "xlsx";
            this.saveAlarmList.FileName = "Presets";
            this.saveAlarmList.Filter = "EXCEL files (*.xlsx)|*.xlsx";
            this.saveAlarmList.Title = "Save prest list";
            // 
            // sinkListView
            // 
            this.sinkListView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.sinkListView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.sinkListView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn4});
            this.sinkListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sinkListView.Location = new System.Drawing.Point(3, 274);
            this.sinkListView.Name = "sinkListView";
            this.sinkListView.Size = new System.Drawing.Size(794, 173);
            this.sinkListView.TabIndex = 2;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Name";
            this.dataGridViewTextBoxColumn3.HeaderText = "Name";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 60;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn5.DataPropertyName = "Label";
            this.dataGridViewTextBoxColumn5.HeaderText = "Label";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            this.dataGridViewTextBoxColumn5.Width = 58;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn4.DataPropertyName = "PlcTag";
            this.dataGridViewTextBoxColumn4.HeaderText = "PLC path";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Width = 76;
            // 
            // id
            // 
            this.id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.id.DataPropertyName = "Id";
            this.id.HeaderText = "ID";
            this.id.Name = "id";
            this.id.ReadOnly = true;
            this.id.Width = 43;
            // 
            // Order
            // 
            this.Order.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Order.DataPropertyName = "Priority";
            this.Order.HeaderText = "Priority";
            this.Order.Name = "Order";
            this.Order.ReadOnly = true;
            this.Order.Width = 63;
            // 
            // alarmListColumnGroup
            // 
            this.alarmListColumnGroup.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.alarmListColumnGroup.DataPropertyName = "AlarmClass";
            this.alarmListColumnGroup.HeaderText = "Alarm class";
            this.alarmListColumnGroup.Name = "alarmListColumnGroup";
            this.alarmListColumnGroup.ReadOnly = true;
            this.alarmListColumnGroup.Width = 85;
            // 
            // alarmListColumnPath
            // 
            this.alarmListColumnPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.alarmListColumnPath.DataPropertyName = "PlcTag";
            this.alarmListColumnPath.HeaderText = "PLC path";
            this.alarmListColumnPath.Name = "alarmListColumnPath";
            this.alarmListColumnPath.ReadOnly = true;
            this.alarmListColumnPath.Width = 76;
            // 
            // alarmListColumnLabel
            // 
            this.alarmListColumnLabel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.alarmListColumnLabel.DataPropertyName = "AlarmText";
            this.alarmListColumnLabel.HeaderText = "Text";
            this.alarmListColumnLabel.Name = "alarmListColumnLabel";
            this.alarmListColumnLabel.ReadOnly = true;
            this.alarmListColumnLabel.Width = 53;
            // 
            // alarmListColumnType
            // 
            this.alarmListColumnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.alarmListColumnType.DataPropertyName = "Sinks";
            this.alarmListColumnType.HeaderText = "Sinks";
            this.alarmListColumnType.Name = "alarmListColumnType";
            this.alarmListColumnType.ReadOnly = true;
            this.alarmListColumnType.Width = 58;
            // 
            // edge
            // 
            this.edge.DataPropertyName = "Edge";
            this.edge.HeaderText = "Edge";
            this.edge.Items.AddRange(new object[] {
            "Rising",
            "Falling"});
            this.edge.Name = "edge";
            this.edge.ReadOnly = true;
            this.edge.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.edge.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // delay
            // 
            this.delay.DataPropertyName = "Delay";
            this.delay.HeaderText = "Delay";
            this.delay.Name = "delay";
            this.delay.ReadOnly = true;
            // 
            // AlarmGenerate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AlarmGenerate";
            this.Text = "Generate Alarms";
            ((System.ComponentModel.ISupportInitialize)(this.alarmListView)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sinkListView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridView alarmListView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button writeButton;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.SaveFileDialog saveAlarmList;
        private System.Windows.Forms.ComboBox cultureComboBox;
        private System.Windows.Forms.DataGridView sinkListView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn Order;
        private System.Windows.Forms.DataGridViewTextBoxColumn alarmListColumnGroup;
        private System.Windows.Forms.DataGridViewTextBoxColumn alarmListColumnPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn alarmListColumnLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn alarmListColumnType;
        private System.Windows.Forms.DataGridViewComboBoxColumn edge;
        private System.Windows.Forms.DataGridViewTextBoxColumn delay;
    }
}