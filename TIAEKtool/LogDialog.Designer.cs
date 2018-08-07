namespace TIAEKtool
{
    partial class LogDialog
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btn_done = new System.Windows.Forms.Button();
            this.logListView = new System.Windows.Forms.DataGridView();
            this.severity = new System.Windows.Forms.DataGridViewButtonColumn();
            this.message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logListView)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.logListView, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btn_done);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 418);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(794, 29);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // btn_done
            // 
            this.btn_done.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_done.Location = new System.Drawing.Point(716, 3);
            this.btn_done.Name = "btn_done";
            this.btn_done.Size = new System.Drawing.Size(75, 23);
            this.btn_done.TabIndex = 2;
            this.btn_done.Text = "Done";
            this.btn_done.UseVisualStyleBackColor = true;
            // 
            // logListView
            // 
            this.logListView.AllowUserToAddRows = false;
            this.logListView.AllowUserToDeleteRows = false;
            this.logListView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.logListView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.logListView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.severity,
            this.message});
            this.logListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logListView.Location = new System.Drawing.Point(3, 3);
            this.logListView.Name = "logListView";
            this.logListView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.logListView.RowHeadersVisible = false;
            this.logListView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.logListView.Size = new System.Drawing.Size(794, 409);
            this.logListView.TabIndex = 1;
            // 
            // severity
            // 
            this.severity.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.severity.DataPropertyName = "Severity";
            this.severity.HeaderText = "Severity";
            this.severity.Name = "severity";
            this.severity.ReadOnly = true;
            this.severity.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.severity.Width = 51;
            // 
            // message
            // 
            this.message.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.message.DataPropertyName = "Message";
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.message.DefaultCellStyle = dataGridViewCellStyle1;
            this.message.HeaderText = "Message";
            this.message.Name = "message";
            this.message.ReadOnly = true;
            this.message.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.message.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // LogDialog
            // 
            this.AcceptButton = this.btn_done;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "LogDialog";
            this.Text = "Message log";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.logListView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btn_done;
        private System.Windows.Forms.DataGridView logListView;
        private System.Windows.Forms.DataGridViewButtonColumn severity;
        private System.Windows.Forms.DataGridViewTextBoxColumn message;
    }
}