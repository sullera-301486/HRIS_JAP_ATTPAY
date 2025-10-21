namespace HRIS_JAP_ATTPAY
{
    partial class AdminArchives
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.XpictureBox = new System.Windows.Forms.PictureBox();
            this.labelArchivedEmployees = new System.Windows.Forms.Label();
            this.labelArchivedRecords = new System.Windows.Forms.Label();
            this.panelLine = new System.Windows.Forms.Panel();
            this.labelSelectAll = new System.Windows.Forms.Label();
            this.buttonRestoreSelected = new System.Windows.Forms.Button();
            this.tableLayoutPanelEmployee = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridViewEmployee = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.tableLayoutPanelEmployee.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEmployee)).BeginInit();
            this.SuspendLayout();
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(1063, 11);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(51, 50);
            this.XpictureBox.TabIndex = 29;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelArchivedEmployees
            // 
            this.labelArchivedEmployees.AutoSize = true;
            this.labelArchivedEmployees.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelArchivedEmployees.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(34)))), ((int)(((byte)(31)))));
            this.labelArchivedEmployees.Location = new System.Drawing.Point(11, 22);
            this.labelArchivedEmployees.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelArchivedEmployees.Name = "labelArchivedEmployees";
            this.labelArchivedEmployees.Size = new System.Drawing.Size(328, 39);
            this.labelArchivedEmployees.TabIndex = 30;
            this.labelArchivedEmployees.Text = "Archived Employees";
            // 
            // labelArchivedRecords
            // 
            this.labelArchivedRecords.AutoSize = true;
            this.labelArchivedRecords.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelArchivedRecords.ForeColor = System.Drawing.Color.Black;
            this.labelArchivedRecords.Location = new System.Drawing.Point(13, 73);
            this.labelArchivedRecords.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelArchivedRecords.Name = "labelArchivedRecords";
            this.labelArchivedRecords.Size = new System.Drawing.Size(356, 29);
            this.labelArchivedRecords.TabIndex = 31;
            this.labelArchivedRecords.Text = "Archived Records and Recovery";
            // 
            // panelLine
            // 
            this.panelLine.BackColor = System.Drawing.Color.Black;
            this.panelLine.Location = new System.Drawing.Point(-58, 150);
            this.panelLine.Name = "panelLine";
            this.panelLine.Size = new System.Drawing.Size(1194, 3);
            this.panelLine.TabIndex = 32;
            // 
            // labelSelectAll
            // 
            this.labelSelectAll.AutoSize = true;
            this.labelSelectAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelSelectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSelectAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(110)))));
            this.labelSelectAll.Location = new System.Drawing.Point(15, 119);
            this.labelSelectAll.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSelectAll.Name = "labelSelectAll";
            this.labelSelectAll.Size = new System.Drawing.Size(88, 24);
            this.labelSelectAll.TabIndex = 33;
            this.labelSelectAll.Text = "Select All";
            this.labelSelectAll.Click += new System.EventHandler(this.labelSelectAll_Click);
            // 
            // buttonRestoreSelected
            // 
            this.buttonRestoreSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRestoreSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(218)))), ((int)(((byte)(71)))));
            this.buttonRestoreSelected.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonRestoreSelected.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonRestoreSelected.FlatAppearance.BorderSize = 0;
            this.buttonRestoreSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRestoreSelected.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRestoreSelected.ForeColor = System.Drawing.Color.White;
            this.buttonRestoreSelected.Location = new System.Drawing.Point(918, 106);
            this.buttonRestoreSelected.Margin = new System.Windows.Forms.Padding(4);
            this.buttonRestoreSelected.Name = "buttonRestoreSelected";
            this.buttonRestoreSelected.Size = new System.Drawing.Size(197, 37);
            this.buttonRestoreSelected.TabIndex = 34;
            this.buttonRestoreSelected.Text = "Restore Selected";
            this.buttonRestoreSelected.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.buttonRestoreSelected.UseVisualStyleBackColor = false;
            this.buttonRestoreSelected.Click += new System.EventHandler(this.buttonRestoreSelected_Click);
            // 
            // tableLayoutPanelEmployee
            // 
            this.tableLayoutPanelEmployee.ColumnCount = 1;
            this.tableLayoutPanelEmployee.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelEmployee.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelEmployee.Controls.Add(this.dataGridViewEmployee, 0, 0);
            this.tableLayoutPanelEmployee.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanelEmployee.Location = new System.Drawing.Point(0, 172);
            this.tableLayoutPanelEmployee.Name = "tableLayoutPanelEmployee";
            this.tableLayoutPanelEmployee.RowCount = 1;
            this.tableLayoutPanelEmployee.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelEmployee.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 648F));
            this.tableLayoutPanelEmployee.Size = new System.Drawing.Size(1128, 549);
            this.tableLayoutPanelEmployee.TabIndex = 35;
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
            this.Column7});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Coral;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewEmployee.DefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridViewEmployee.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewEmployee.EnableHeadersVisualStyles = false;
            this.dataGridViewEmployee.GridColor = System.Drawing.Color.White;
            this.dataGridViewEmployee.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewEmployee.Margin = new System.Windows.Forms.Padding(0);
            this.dataGridViewEmployee.Name = "dataGridViewEmployee";
            this.dataGridViewEmployee.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewEmployee.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewEmployee.RowHeadersVisible = false;
            this.dataGridViewEmployee.RowHeadersWidth = 51;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.Black;
            this.dataGridViewEmployee.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridViewEmployee.RowTemplate.DividerHeight = 10;
            this.dataGridViewEmployee.RowTemplate.Height = 40;
            this.dataGridViewEmployee.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewEmployee.Size = new System.Drawing.Size(1128, 549);
            this.dataGridViewEmployee.TabIndex = 0;
            // 
            // Column1
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.NullValue = false;
            this.Column1.DefaultCellStyle = dataGridViewCellStyle2;
            this.Column1.FillWeight = 84F;
            this.Column1.HeaderText = "";
            this.Column1.MinimumWidth = 6;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // Column2
            // 
            this.Column2.FillWeight = 104F;
            this.Column2.HeaderText = "ID";
            this.Column2.MinimumWidth = 6;
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column3
            // 
            this.Column3.FillWeight = 168F;
            this.Column3.HeaderText = "Name";
            this.Column3.MinimumWidth = 6;
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column4
            // 
            this.Column4.FillWeight = 168F;
            this.Column4.HeaderText = "Department";
            this.Column4.MinimumWidth = 6;
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column5
            // 
            this.Column5.FillWeight = 168F;
            this.Column5.HeaderText = "Position";
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
            this.Column6.FillWeight = 168F;
            this.Column6.HeaderText = "Date Archived";
            this.Column6.MinimumWidth = 6;
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            this.Column6.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column7
            // 
            this.Column7.FillWeight = 120F;
            this.Column7.HeaderText = "";
            this.Column7.MinimumWidth = 6;
            this.Column7.Name = "Column7";
            this.Column7.ReadOnly = true;
            this.Column7.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // AdminArchives
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1128, 721);
            this.Controls.Add(this.tableLayoutPanelEmployee);
            this.Controls.Add(this.buttonRestoreSelected);
            this.Controls.Add(this.labelSelectAll);
            this.Controls.Add(this.panelLine);
            this.Controls.Add(this.labelArchivedRecords);
            this.Controls.Add(this.labelArchivedEmployees);
            this.Controls.Add(this.XpictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AdminArchives";
            this.ShowInTaskbar = false;
            this.Text = "AdminArchives";
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.tableLayoutPanelEmployee.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEmployee)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Label labelArchivedEmployees;
        private System.Windows.Forms.Label labelArchivedRecords;
        private System.Windows.Forms.Panel panelLine;
        private System.Windows.Forms.Label labelSelectAll;
        private System.Windows.Forms.Button buttonRestoreSelected;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelEmployee;
        private System.Windows.Forms.DataGridView dataGridViewEmployee;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewButtonColumn Column7;
    }
}