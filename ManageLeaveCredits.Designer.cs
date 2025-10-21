namespace HRIS_JAP_ATTPAY
{
    partial class ManageLeaveCredits
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageLeaveCredits));
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelViewLeaveRecords = new System.Windows.Forms.Label();
            this.labelLeaveManagement = new System.Windows.Forms.Label();
            this.XpictureBox = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelEmployeeLeaveCredits = new System.Windows.Forms.Label();
            this.labelManageLeaveCredits = new System.Windows.Forms.Label();
            this.labelAddLeave = new System.Windows.Forms.Label();
            this.panelSearch = new System.Windows.Forms.Panel();
            this.textBoxSearchEmployee = new System.Windows.Forms.TextBox();
            this.pictureBoxSearch = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridViewEmployee = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column8 = new System.Windows.Forms.DataGridViewImageColumn();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.panel2.SuspendLayout();
            this.panelSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSearch)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEmployee)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.labelViewLeaveRecords);
            this.panel1.Controls.Add(this.labelLeaveManagement);
            this.panel1.Controls.Add(this.XpictureBox);
            this.panel1.Location = new System.Drawing.Point(-3, 1);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1083, 148);
            this.panel1.TabIndex = 14;
            // 
            // labelViewLeaveRecords
            // 
            this.labelViewLeaveRecords.AutoSize = true;
            this.labelViewLeaveRecords.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelViewLeaveRecords.Location = new System.Drawing.Point(423, 90);
            this.labelViewLeaveRecords.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelViewLeaveRecords.Name = "labelViewLeaveRecords";
            this.labelViewLeaveRecords.Size = new System.Drawing.Size(234, 29);
            this.labelViewLeaveRecords.TabIndex = 4;
            this.labelViewLeaveRecords.Text = "View Leave Records";
            // 
            // labelLeaveManagement
            // 
            this.labelLeaveManagement.AutoSize = true;
            this.labelLeaveManagement.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLeaveManagement.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(36)))), ((int)(((byte)(30)))));
            this.labelLeaveManagement.Location = new System.Drawing.Point(351, 30);
            this.labelLeaveManagement.Name = "labelLeaveManagement";
            this.labelLeaveManagement.Size = new System.Drawing.Size(369, 46);
            this.labelLeaveManagement.TabIndex = 3;
            this.labelLeaveManagement.Text = "Leave Management";
            this.labelLeaveManagement.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(1021, 2);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(51, 50);
            this.XpictureBox.TabIndex = 2;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click_1);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.tableLayoutPanel1);
            this.panel2.Controls.Add(this.panelSearch);
            this.panel2.Controls.Add(this.labelAddLeave);
            this.panel2.Controls.Add(this.labelManageLeaveCredits);
            this.panel2.Controls.Add(this.labelEmployeeLeaveCredits);
            this.panel2.Location = new System.Drawing.Point(-3, 151);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1083, 618);
            this.panel2.TabIndex = 15;
            // 
            // labelEmployeeLeaveCredits
            // 
            this.labelEmployeeLeaveCredits.AutoSize = true;
            this.labelEmployeeLeaveCredits.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEmployeeLeaveCredits.Location = new System.Drawing.Point(45, 20);
            this.labelEmployeeLeaveCredits.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelEmployeeLeaveCredits.Name = "labelEmployeeLeaveCredits";
            this.labelEmployeeLeaveCredits.Size = new System.Drawing.Size(358, 36);
            this.labelEmployeeLeaveCredits.TabIndex = 13;
            this.labelEmployeeLeaveCredits.Text = "Employee Leave Credits";
            // 
            // labelManageLeaveCredits
            // 
            this.labelManageLeaveCredits.AutoSize = true;
            this.labelManageLeaveCredits.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelManageLeaveCredits.Location = new System.Drawing.Point(46, 75);
            this.labelManageLeaveCredits.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelManageLeaveCredits.Name = "labelManageLeaveCredits";
            this.labelManageLeaveCredits.Size = new System.Drawing.Size(254, 29);
            this.labelManageLeaveCredits.TabIndex = 5;
            this.labelManageLeaveCredits.Text = "Manage Leave Credits";
            // 
            // labelAddLeave
            // 
            this.labelAddLeave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAddLeave.AutoSize = true;
            this.labelAddLeave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelAddLeave.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAddLeave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.labelAddLeave.Location = new System.Drawing.Point(923, 20);
            this.labelAddLeave.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAddLeave.Name = "labelAddLeave";
            this.labelAddLeave.Size = new System.Drawing.Size(135, 25);
            this.labelAddLeave.TabIndex = 14;
            this.labelAddLeave.Text = "Add Leave +";
            this.labelAddLeave.Click += new System.EventHandler(this.labelAddLeave_Click);
            // 
            // panelSearch
            // 
            this.panelSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.panelSearch.Controls.Add(this.textBoxSearchEmployee);
            this.panelSearch.Controls.Add(this.pictureBoxSearch);
            this.panelSearch.Location = new System.Drawing.Point(626, 52);
            this.panelSearch.Margin = new System.Windows.Forms.Padding(0);
            this.panelSearch.Name = "panelSearch";
            this.panelSearch.Size = new System.Drawing.Size(432, 52);
            this.panelSearch.TabIndex = 15;
            // 
            // textBoxSearchEmployee
            // 
            this.textBoxSearchEmployee.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxSearchEmployee.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxSearchEmployee.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxSearchEmployee.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSearchEmployee.ForeColor = System.Drawing.Color.White;
            this.textBoxSearchEmployee.Location = new System.Drawing.Point(0, 10);
            this.textBoxSearchEmployee.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxSearchEmployee.Multiline = true;
            this.textBoxSearchEmployee.Name = "textBoxSearchEmployee";
            this.textBoxSearchEmployee.Size = new System.Drawing.Size(375, 42);
            this.textBoxSearchEmployee.TabIndex = 2;
            this.textBoxSearchEmployee.Text = "Find Employee";
            // 
            // pictureBoxSearch
            // 
            this.pictureBoxSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.pictureBoxSearch.BackgroundImage = global::HRIS_JAP_ATTPAY.Properties.Resources.search;
            this.pictureBoxSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxSearch.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureBoxSearch.Location = new System.Drawing.Point(375, 0);
            this.pictureBoxSearch.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxSearch.Name = "pictureBoxSearch";
            this.pictureBoxSearch.Size = new System.Drawing.Size(57, 52);
            this.pictureBoxSearch.TabIndex = 3;
            this.pictureBoxSearch.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.dataGridViewEmployee, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 123);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1083, 495);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // dataGridViewEmployee
            // 
            this.dataGridViewEmployee.AllowUserToAddRows = false;
            this.dataGridViewEmployee.AllowUserToDeleteRows = false;
            this.dataGridViewEmployee.AllowUserToResizeColumns = false;
            this.dataGridViewEmployee.AllowUserToResizeRows = false;
            this.dataGridViewEmployee.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewEmployee.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewEmployee.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewEmployee.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridViewEmployee.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewEmployee.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewEmployee.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewEmployee.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5,
            this.Column6,
            this.Column7,
            this.Column8});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Coral;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewEmployee.DefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewEmployee.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewEmployee.EnableHeadersVisualStyles = false;
            this.dataGridViewEmployee.GridColor = System.Drawing.Color.White;
            this.dataGridViewEmployee.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewEmployee.Margin = new System.Windows.Forms.Padding(0);
            this.dataGridViewEmployee.Name = "dataGridViewEmployee";
            this.dataGridViewEmployee.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewEmployee.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridViewEmployee.RowHeadersVisible = false;
            this.dataGridViewEmployee.RowHeadersWidth = 51;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.Black;
            this.dataGridViewEmployee.RowsDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridViewEmployee.RowTemplate.DividerHeight = 10;
            this.dataGridViewEmployee.RowTemplate.Height = 40;
            this.dataGridViewEmployee.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewEmployee.Size = new System.Drawing.Size(1083, 495);
            this.dataGridViewEmployee.TabIndex = 1;
            // 
            // Column1
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Column1.DefaultCellStyle = dataGridViewCellStyle2;
            this.Column1.FillWeight = 104F;
            this.Column1.HeaderText = "ID";
            this.Column1.MinimumWidth = 6;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column2
            // 
            this.Column2.FillWeight = 168F;
            this.Column2.HeaderText = "Name";
            this.Column2.MinimumWidth = 6;
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column3
            // 
            this.Column3.FillWeight = 104F;
            this.Column3.HeaderText = "SL Credit";
            this.Column3.MinimumWidth = 6;
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column4
            // 
            this.Column4.FillWeight = 104F;
            this.Column4.HeaderText = "SL Left";
            this.Column4.MinimumWidth = 6;
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column5
            // 
            this.Column5.FillWeight = 104F;
            this.Column5.HeaderText = "VL Credit";
            this.Column5.MinimumWidth = 6;
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column6
            // 
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            this.Column6.DefaultCellStyle = dataGridViewCellStyle3;
            this.Column6.FillWeight = 104F;
            this.Column6.HeaderText = "VL Left";
            this.Column6.MinimumWidth = 6;
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            this.Column6.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column7
            // 
            this.Column7.FillWeight = 168F;
            this.Column7.HeaderText = "Last Updated";
            this.Column7.MinimumWidth = 6;
            this.Column7.Name = "Column7";
            this.Column7.ReadOnly = true;
            this.Column7.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column8
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.NullValue = ((object)(resources.GetObject("dataGridViewCellStyle4.NullValue")));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black;
            this.Column8.DefaultCellStyle = dataGridViewCellStyle4;
            this.Column8.FillWeight = 23F;
            this.Column8.HeaderText = "";
            this.Column8.Image = ((System.Drawing.Image)(resources.GetObject("Column8.Image")));
            this.Column8.MinimumWidth = 6;
            this.Column8.Name = "Column8";
            this.Column8.ReadOnly = true;
            this.Column8.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column8.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // ManageLeaveCredits
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1080, 768);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ManageLeaveCredits";
            this.ShowInTaskbar = false;
            this.Text = "ManageLeaveCredits";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panelSearch.ResumeLayout(false);
            this.panelSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSearch)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEmployee)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelViewLeaveRecords;
        private System.Windows.Forms.Label labelLeaveManagement;
        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelEmployeeLeaveCredits;
        private System.Windows.Forms.Label labelManageLeaveCredits;
        private System.Windows.Forms.Label labelAddLeave;
        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.TextBox textBoxSearchEmployee;
        private System.Windows.Forms.PictureBox pictureBoxSearch;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dataGridViewEmployee;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.DataGridViewImageColumn Column8;
    }
}