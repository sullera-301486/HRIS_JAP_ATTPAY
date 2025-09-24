namespace HRIS_JAP_ATTPAY
{
    partial class AdminAttendance
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

        #region Component Designer generated code

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
            this.labelManageLeave = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelSearch = new System.Windows.Forms.Panel();
            this.textBoxSearchEmployee = new System.Windows.Forms.TextBox();
            this.pictureBoxSearch = new System.Windows.Forms.PictureBox();
            this.panelFilter = new System.Windows.Forms.Panel();
            this.labelFiltersName = new System.Windows.Forms.Label();
            this.pictureBoxFilters = new System.Windows.Forms.PictureBox();
            this.labelAdminAttendance = new System.Windows.Forms.Label();
            this.labelAttendanceDate = new System.Windows.Forms.Label();
            this.panelDate = new System.Windows.Forms.Panel();
            this.comboBoxSelectDate = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanelAttendance = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridViewAttendance = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column11 = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewImageColumn1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSearch)).BeginInit();
            this.panelFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFilters)).BeginInit();
            this.panelDate.SuspendLayout();
            this.tableLayoutPanelAttendance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAttendance)).BeginInit();
            this.SuspendLayout();
            // 
            // labelManageLeave
            // 
            this.labelManageLeave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelManageLeave.AutoSize = true;
            this.labelManageLeave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelManageLeave.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelManageLeave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.labelManageLeave.Location = new System.Drawing.Point(1264, 89);
            this.labelManageLeave.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelManageLeave.Name = "labelManageLeave";
            this.labelManageLeave.Size = new System.Drawing.Size(155, 25);
            this.labelManageLeave.TabIndex = 7;
            this.labelManageLeave.Text = "Manage Leave";
            this.labelManageLeave.Click += new System.EventHandler(this.labelManageLeave_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 8;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1F));
            this.tableLayoutPanel1.Controls.Add(this.panelSearch, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelFilter, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelAdminAttendance, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelManageLeave, 6, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelAttendanceDate, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panelDate, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 61F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 39F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1440, 114);
            this.tableLayoutPanel1.TabIndex = 15;
            // 
            // panelSearch
            // 
            this.panelSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.panelSearch.Controls.Add(this.textBoxSearchEmployee);
            this.panelSearch.Controls.Add(this.pictureBoxSearch);
            this.panelSearch.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSearch.Location = new System.Drawing.Point(1035, 17);
            this.panelSearch.Margin = new System.Windows.Forms.Padding(0);
            this.panelSearch.Name = "panelSearch";
            this.panelSearch.Size = new System.Drawing.Size(388, 52);
            this.panelSearch.TabIndex = 4;
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
            this.textBoxSearchEmployee.Size = new System.Drawing.Size(331, 42);
            this.textBoxSearchEmployee.TabIndex = 2;
            this.textBoxSearchEmployee.Text = "Find Employee";
            // 
            // pictureBoxSearch
            // 
            this.pictureBoxSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.pictureBoxSearch.BackgroundImage = global::HRIS_JAP_ATTPAY.Properties.Resources.search;
            this.pictureBoxSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxSearch.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureBoxSearch.Location = new System.Drawing.Point(331, 0);
            this.pictureBoxSearch.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxSearch.Name = "pictureBoxSearch";
            this.pictureBoxSearch.Size = new System.Drawing.Size(57, 52);
            this.pictureBoxSearch.TabIndex = 3;
            this.pictureBoxSearch.TabStop = false;
            // 
            // panelFilter
            // 
            this.panelFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFilter.Controls.Add(this.labelFiltersName);
            this.panelFilter.Controls.Add(this.pictureBoxFilters);
            this.panelFilter.Location = new System.Drawing.Point(874, 22);
            this.panelFilter.Margin = new System.Windows.Forms.Padding(0);
            this.panelFilter.Name = "panelFilter";
            this.panelFilter.Size = new System.Drawing.Size(147, 47);
            this.panelFilter.TabIndex = 10;
            // 
            // labelFiltersName
            // 
            this.labelFiltersName.AutoSize = true;
            this.labelFiltersName.BackColor = System.Drawing.Color.White;
            this.labelFiltersName.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFiltersName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(142)))), ((int)(((byte)(144)))));
            this.labelFiltersName.Location = new System.Drawing.Point(11, 9);
            this.labelFiltersName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelFiltersName.Name = "labelFiltersName";
            this.labelFiltersName.Size = new System.Drawing.Size(80, 29);
            this.labelFiltersName.TabIndex = 10;
            this.labelFiltersName.Text = "Filters";
            // 
            // pictureBoxFilters
            // 
            this.pictureBoxFilters.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxFilters.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.filter__1__1;
            this.pictureBoxFilters.Location = new System.Drawing.Point(97, 0);
            this.pictureBoxFilters.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBoxFilters.Name = "pictureBoxFilters";
            this.pictureBoxFilters.Size = new System.Drawing.Size(49, 46);
            this.pictureBoxFilters.TabIndex = 7;
            this.pictureBoxFilters.TabStop = false;
            this.pictureBoxFilters.Click += new System.EventHandler(this.pictureBoxFilters_Click);
            // 
            // labelAdminAttendance
            // 
            this.labelAdminAttendance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelAdminAttendance.AutoSize = true;
            this.labelAdminAttendance.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAdminAttendance.Location = new System.Drawing.Point(46, 0);
            this.labelAdminAttendance.Name = "labelAdminAttendance";
            this.labelAdminAttendance.Size = new System.Drawing.Size(349, 69);
            this.labelAdminAttendance.TabIndex = 11;
            this.labelAdminAttendance.Text = "Attendance and Time Tracking";
            // 
            // labelAttendanceDate
            // 
            this.labelAttendanceDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelAttendanceDate.AutoSize = true;
            this.labelAttendanceDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAttendanceDate.ForeColor = System.Drawing.Color.Black;
            this.labelAttendanceDate.Location = new System.Drawing.Point(49, 89);
            this.labelAttendanceDate.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelAttendanceDate.Name = "labelAttendanceDate";
            this.labelAttendanceDate.Size = new System.Drawing.Size(118, 25);
            this.labelAttendanceDate.TabIndex = 13;
            this.labelAttendanceDate.Text = "[Insert Date]\r\n";
            // 
            // panelDate
            // 
            this.panelDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDate.Controls.Add(this.comboBoxSelectDate);
            this.panelDate.Location = new System.Drawing.Point(609, 22);
            this.panelDate.Margin = new System.Windows.Forms.Padding(0);
            this.panelDate.Name = "panelDate";
            this.panelDate.Size = new System.Drawing.Size(226, 47);
            this.panelDate.TabIndex = 19;
            // 
            // comboBoxSelectDate
            // 
            this.comboBoxSelectDate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxSelectDate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSelectDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxSelectDate.FormattingEnabled = true;
            this.comboBoxSelectDate.Location = new System.Drawing.Point(0, 0);
            this.comboBoxSelectDate.Margin = new System.Windows.Forms.Padding(0);
            this.comboBoxSelectDate.Name = "comboBoxSelectDate";
            this.comboBoxSelectDate.Size = new System.Drawing.Size(226, 37);
            this.comboBoxSelectDate.TabIndex = 9;
            // 
            // tableLayoutPanelAttendance
            // 
            this.tableLayoutPanelAttendance.ColumnCount = 1;
            this.tableLayoutPanelAttendance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelAttendance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelAttendance.Controls.Add(this.dataGridViewAttendance, 0, 0);
            this.tableLayoutPanelAttendance.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanelAttendance.Location = new System.Drawing.Point(0, 194);
            this.tableLayoutPanelAttendance.Name = "tableLayoutPanelAttendance";
            this.tableLayoutPanelAttendance.RowCount = 1;
            this.tableLayoutPanelAttendance.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelAttendance.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 648F));
            this.tableLayoutPanelAttendance.Size = new System.Drawing.Size(1440, 648);
            this.tableLayoutPanelAttendance.TabIndex = 18;
            // 
            // dataGridViewAttendance
            // 
            this.dataGridViewAttendance.AllowUserToAddRows = false;
            this.dataGridViewAttendance.AllowUserToDeleteRows = false;
            this.dataGridViewAttendance.AllowUserToResizeColumns = false;
            this.dataGridViewAttendance.AllowUserToResizeRows = false;
            this.dataGridViewAttendance.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewAttendance.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewAttendance.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewAttendance.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridViewAttendance.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewAttendance.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewAttendance.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAttendance.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5,
            this.Column6,
            this.Column7,
            this.Column8,
            this.Column9,
            this.Column10,
            this.Column11});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Coral;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewAttendance.DefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridViewAttendance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewAttendance.EnableHeadersVisualStyles = false;
            this.dataGridViewAttendance.GridColor = System.Drawing.Color.White;
            this.dataGridViewAttendance.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewAttendance.Margin = new System.Windows.Forms.Padding(0);
            this.dataGridViewAttendance.Name = "dataGridViewAttendance";
            this.dataGridViewAttendance.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewAttendance.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewAttendance.RowHeadersVisible = false;
            this.dataGridViewAttendance.RowHeadersWidth = 51;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.Black;
            this.dataGridViewAttendance.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridViewAttendance.RowTemplate.DividerHeight = 10;
            this.dataGridViewAttendance.RowTemplate.Height = 40;
            this.dataGridViewAttendance.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewAttendance.Size = new System.Drawing.Size(1440, 648);
            this.dataGridViewAttendance.TabIndex = 0;
            this.dataGridViewAttendance.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewAttendance_CellContentClick);
            // 
            // Column1
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Column1.DefaultCellStyle = dataGridViewCellStyle2;
            this.Column1.FillWeight = 50F;
            this.Column1.HeaderText = "";
            this.Column1.MinimumWidth = 6;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column2
            // 
            this.Column2.FillWeight = 77F;
            this.Column2.HeaderText = "ID";
            this.Column2.MinimumWidth = 6;
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column3
            // 
            this.Column3.FillWeight = 124F;
            this.Column3.HeaderText = "Name";
            this.Column3.MinimumWidth = 6;
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column4
            // 
            this.Column4.FillWeight = 124F;
            this.Column4.HeaderText = "Time In";
            this.Column4.MinimumWidth = 6;
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column5
            // 
            this.Column5.FillWeight = 124F;
            this.Column5.HeaderText = "Time Out";
            this.Column5.MinimumWidth = 6;
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column6
            // 
            this.Column6.FillWeight = 124F;
            this.Column6.HeaderText = "Hours Worked";
            this.Column6.MinimumWidth = 6;
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            // 
            // Column7
            // 
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            this.Column7.DefaultCellStyle = dataGridViewCellStyle3;
            this.Column7.FillWeight = 59F;
            this.Column7.HeaderText = "Status";
            this.Column7.MinimumWidth = 6;
            this.Column7.Name = "Column7";
            this.Column7.ReadOnly = true;
            this.Column7.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column8
            // 
            this.Column8.FillWeight = 124F;
            this.Column8.HeaderText = "Overtime In";
            this.Column8.MinimumWidth = 6;
            this.Column8.Name = "Column8";
            this.Column8.ReadOnly = true;
            this.Column8.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column9
            // 
            this.Column9.FillWeight = 124F;
            this.Column9.HeaderText = "Overtime Out";
            this.Column9.MinimumWidth = 6;
            this.Column9.Name = "Column9";
            this.Column9.ReadOnly = true;
            this.Column9.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column10
            // 
            this.Column10.FillWeight = 124F;
            this.Column10.HeaderText = "Verification";
            this.Column10.MinimumWidth = 6;
            this.Column10.Name = "Column10";
            this.Column10.ReadOnly = true;
            this.Column10.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column11
            // 
            this.Column11.FillWeight = 17F;
            this.Column11.HeaderText = "";
            this.Column11.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.VerticalThreeDots;
            this.Column11.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.Column11.MinimumWidth = 6;
            this.Column11.Name = "Column11";
            this.Column11.ReadOnly = true;
            this.Column11.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // dataGridViewImageColumn1
            // 
            this.dataGridViewImageColumn1.FillWeight = 17F;
            this.dataGridViewImageColumn1.HeaderText = "";
            this.dataGridViewImageColumn1.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.VerticalThreeDots;
            this.dataGridViewImageColumn1.MinimumWidth = 6;
            this.dataGridViewImageColumn1.Name = "dataGridViewImageColumn1";
            this.dataGridViewImageColumn1.ReadOnly = true;
            this.dataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewImageColumn1.Width = 23;
            // 
            // AdminAttendance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutPanelAttendance);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "AdminAttendance";
            this.Size = new System.Drawing.Size(1440, 842);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panelSearch.ResumeLayout(false);
            this.panelSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSearch)).EndInit();
            this.panelFilter.ResumeLayout(false);
            this.panelFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFilters)).EndInit();
            this.panelDate.ResumeLayout(false);
            this.tableLayoutPanelAttendance.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAttendance)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label labelManageLeave;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.TextBox textBoxSearchEmployee;
        private System.Windows.Forms.PictureBox pictureBoxSearch;
        private System.Windows.Forms.Panel panelFilter;
        private System.Windows.Forms.Label labelFiltersName;
        private System.Windows.Forms.PictureBox pictureBoxFilters;
        private System.Windows.Forms.Label labelAdminAttendance;
        private System.Windows.Forms.Label labelAttendanceDate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAttendance;
        private System.Windows.Forms.DataGridView dataGridViewAttendance;
        private System.Windows.Forms.Panel panelDate;
        private System.Windows.Forms.ComboBox comboBoxSelectDate;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
        private System.Windows.Forms.DataGridViewImageColumn Column11;
    }
}
