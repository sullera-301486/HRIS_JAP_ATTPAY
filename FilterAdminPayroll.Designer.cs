namespace HRIS_JAP_ATTPAY
{
    partial class FilterAdminPayroll
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
            this.labelSearchFilters = new System.Windows.Forms.Label();
            this.XpictureBox = new System.Windows.Forms.PictureBox();
            this.labelDateRange = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.labelDepartment = new System.Windows.Forms.Label();
            this.labelSalary = new System.Windows.Forms.Label();
            this.labelGrossPay = new System.Windows.Forms.Label();
            this.labelNetPay = new System.Windows.Forms.Label();
            this.labelUnusedLeave = new System.Windows.Forms.Label();
            this.textBoxStartDate = new System.Windows.Forms.TextBox();
            this.textBoxEndDate = new System.Windows.Forms.TextBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.textBoxID = new System.Windows.Forms.TextBox();
            this.labelID = new System.Windows.Forms.Label();
            this.labelPosition = new System.Windows.Forms.Label();
            this.comboBoxDepartment = new System.Windows.Forms.ComboBox();
            this.comboBoxPosition = new System.Windows.Forms.ComboBox();
            this.comboBoxSort = new System.Windows.Forms.ComboBox();
            this.labelSort = new System.Windows.Forms.Label();
            this.pictureBoxSortLogo = new System.Windows.Forms.PictureBox();
            this.textBoxSalaryMinimum = new System.Windows.Forms.TextBox();
            this.textBoxSalaryMaximum = new System.Windows.Forms.TextBox();
            this.textBoxGrossPayMinimum = new System.Windows.Forms.TextBox();
            this.textBoxNetPayMinimum = new System.Windows.Forms.TextBox();
            this.textBoxGrossPayMaximum = new System.Windows.Forms.TextBox();
            this.textBoxNetPayMaximum = new System.Windows.Forms.TextBox();
            this.labelOvertimeHours = new System.Windows.Forms.Label();
            this.comboBoxUnusedLeave = new System.Windows.Forms.ComboBox();
            this.comboBoxOvertimeHours = new System.Windows.Forms.ComboBox();
            this.labelDashA = new System.Windows.Forms.Label();
            this.labelDashB = new System.Windows.Forms.Label();
            this.labelDashC = new System.Windows.Forms.Label();
            this.labelDashD = new System.Windows.Forms.Label();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonApply = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSortLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // labelSearchFilters
            // 
            this.labelSearchFilters.AutoSize = true;
            this.labelSearchFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSearchFilters.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(34)))), ((int)(((byte)(31)))));
            this.labelSearchFilters.Location = new System.Drawing.Point(3, 17);
            this.labelSearchFilters.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSearchFilters.Name = "labelSearchFilters";
            this.labelSearchFilters.Size = new System.Drawing.Size(198, 36);
            this.labelSearchFilters.TabIndex = 53;
            this.labelSearchFilters.Text = "Search Filters";
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(437, 0);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(51, 50);
            this.XpictureBox.TabIndex = 52;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelDateRange
            // 
            this.labelDateRange.AutoSize = true;
            this.labelDateRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDateRange.Location = new System.Drawing.Point(33, 78);
            this.labelDateRange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDateRange.Name = "labelDateRange";
            this.labelDateRange.Size = new System.Drawing.Size(150, 25);
            this.labelDateRange.TabIndex = 54;
            this.labelDateRange.Text = "DATE RANGE";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelName.Location = new System.Drawing.Point(33, 150);
            this.labelName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(74, 25);
            this.labelName.TabIndex = 55;
            this.labelName.Text = "NAME";
            // 
            // labelDepartment
            // 
            this.labelDepartment.AutoSize = true;
            this.labelDepartment.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDepartment.Location = new System.Drawing.Point(33, 223);
            this.labelDepartment.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDepartment.Name = "labelDepartment";
            this.labelDepartment.Size = new System.Drawing.Size(159, 25);
            this.labelDepartment.TabIndex = 56;
            this.labelDepartment.Text = "DEPARTMENT";
            // 
            // labelSalary
            // 
            this.labelSalary.AutoSize = true;
            this.labelSalary.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSalary.Location = new System.Drawing.Point(33, 298);
            this.labelSalary.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSalary.Name = "labelSalary";
            this.labelSalary.Size = new System.Drawing.Size(97, 25);
            this.labelSalary.TabIndex = 57;
            this.labelSalary.Text = "SALARY";
            // 
            // labelGrossPay
            // 
            this.labelGrossPay.AutoSize = true;
            this.labelGrossPay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGrossPay.Location = new System.Drawing.Point(33, 370);
            this.labelGrossPay.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelGrossPay.Name = "labelGrossPay";
            this.labelGrossPay.Size = new System.Drawing.Size(138, 25);
            this.labelGrossPay.TabIndex = 58;
            this.labelGrossPay.Text = "GROSS PAY";
            // 
            // labelNetPay
            // 
            this.labelNetPay.AutoSize = true;
            this.labelNetPay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNetPay.Location = new System.Drawing.Point(33, 443);
            this.labelNetPay.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelNetPay.Name = "labelNetPay";
            this.labelNetPay.Size = new System.Drawing.Size(104, 25);
            this.labelNetPay.TabIndex = 59;
            this.labelNetPay.Text = "NET PAY";
            // 
            // labelUnusedLeave
            // 
            this.labelUnusedLeave.AutoSize = true;
            this.labelUnusedLeave.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUnusedLeave.Location = new System.Drawing.Point(33, 516);
            this.labelUnusedLeave.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUnusedLeave.Name = "labelUnusedLeave";
            this.labelUnusedLeave.Size = new System.Drawing.Size(177, 25);
            this.labelUnusedLeave.TabIndex = 60;
            this.labelUnusedLeave.Text = "UNUSED LEAVE";
            // 
            // textBoxStartDate
            // 
            this.textBoxStartDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxStartDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxStartDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxStartDate.ForeColor = System.Drawing.Color.White;
            this.textBoxStartDate.Location = new System.Drawing.Point(39, 112);
            this.textBoxStartDate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxStartDate.Name = "textBoxStartDate";
            this.textBoxStartDate.Size = new System.Drawing.Size(183, 26);
            this.textBoxStartDate.TabIndex = 61;
            this.textBoxStartDate.Text = "Start date";
            // 
            // textBoxEndDate
            // 
            this.textBoxEndDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxEndDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxEndDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxEndDate.ForeColor = System.Drawing.Color.White;
            this.textBoxEndDate.Location = new System.Drawing.Point(259, 112);
            this.textBoxEndDate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxEndDate.Name = "textBoxEndDate";
            this.textBoxEndDate.Size = new System.Drawing.Size(183, 26);
            this.textBoxEndDate.TabIndex = 62;
            this.textBoxEndDate.Text = "End date";
            // 
            // textBoxName
            // 
            this.textBoxName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxName.ForeColor = System.Drawing.Color.White;
            this.textBoxName.Location = new System.Drawing.Point(39, 185);
            this.textBoxName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(183, 26);
            this.textBoxName.TabIndex = 63;
            this.textBoxName.Text = "Search name";
            // 
            // textBoxID
            // 
            this.textBoxID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxID.ForeColor = System.Drawing.Color.White;
            this.textBoxID.Location = new System.Drawing.Point(259, 185);
            this.textBoxID.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxID.Name = "textBoxID";
            this.textBoxID.Size = new System.Drawing.Size(183, 26);
            this.textBoxID.TabIndex = 64;
            this.textBoxID.Text = "Search ID";
            // 
            // labelID
            // 
            this.labelID.AutoSize = true;
            this.labelID.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelID.Location = new System.Drawing.Point(253, 150);
            this.labelID.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelID.Name = "labelID";
            this.labelID.Size = new System.Drawing.Size(33, 25);
            this.labelID.TabIndex = 65;
            this.labelID.Text = "ID";
            // 
            // labelPosition
            // 
            this.labelPosition.AutoSize = true;
            this.labelPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPosition.Location = new System.Drawing.Point(253, 223);
            this.labelPosition.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(116, 25);
            this.labelPosition.TabIndex = 66;
            this.labelPosition.Text = "POSITION";
            // 
            // comboBoxDepartment
            // 
            this.comboBoxDepartment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.comboBoxDepartment.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxDepartment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxDepartment.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxDepartment.ForeColor = System.Drawing.Color.White;
            this.comboBoxDepartment.FormattingEnabled = true;
            this.comboBoxDepartment.Location = new System.Drawing.Point(39, 257);
            this.comboBoxDepartment.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxDepartment.Name = "comboBoxDepartment";
            this.comboBoxDepartment.Size = new System.Drawing.Size(183, 28);
            this.comboBoxDepartment.TabIndex = 67;
            this.comboBoxDepartment.Text = "Select department";
            // 
            // comboBoxPosition
            // 
            this.comboBoxPosition.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.comboBoxPosition.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxPosition.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxPosition.ForeColor = System.Drawing.Color.White;
            this.comboBoxPosition.FormattingEnabled = true;
            this.comboBoxPosition.Location = new System.Drawing.Point(259, 257);
            this.comboBoxPosition.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxPosition.Name = "comboBoxPosition";
            this.comboBoxPosition.Size = new System.Drawing.Size(183, 28);
            this.comboBoxPosition.TabIndex = 68;
            this.comboBoxPosition.Text = "Select position";
            // 
            // comboBoxSort
            // 
            this.comboBoxSort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.comboBoxSort.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxSort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxSort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxSort.ForeColor = System.Drawing.Color.White;
            this.comboBoxSort.FormattingEnabled = true;
            this.comboBoxSort.Location = new System.Drawing.Point(356, 75);
            this.comboBoxSort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxSort.Name = "comboBoxSort";
            this.comboBoxSort.Size = new System.Drawing.Size(85, 28);
            this.comboBoxSort.TabIndex = 71;
            this.comboBoxSort.Text = "A-Z";
            // 
            // labelSort
            // 
            this.labelSort.AutoSize = true;
            this.labelSort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSort.Location = new System.Drawing.Point(253, 78);
            this.labelSort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSort.Name = "labelSort";
            this.labelSort.Size = new System.Drawing.Size(52, 25);
            this.labelSort.TabIndex = 70;
            this.labelSort.Text = "Sort";
            // 
            // pictureBoxSortLogo
            // 
            this.pictureBoxSortLogo.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.Filter;
            this.pictureBoxSortLogo.Location = new System.Drawing.Point(316, 73);
            this.pictureBoxSortLogo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBoxSortLogo.Name = "pictureBoxSortLogo";
            this.pictureBoxSortLogo.Size = new System.Drawing.Size(32, 33);
            this.pictureBoxSortLogo.TabIndex = 69;
            this.pictureBoxSortLogo.TabStop = false;
            // 
            // textBoxSalaryMinimum
            // 
            this.textBoxSalaryMinimum.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxSalaryMinimum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxSalaryMinimum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSalaryMinimum.ForeColor = System.Drawing.Color.White;
            this.textBoxSalaryMinimum.Location = new System.Drawing.Point(39, 332);
            this.textBoxSalaryMinimum.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxSalaryMinimum.Name = "textBoxSalaryMinimum";
            this.textBoxSalaryMinimum.Size = new System.Drawing.Size(183, 26);
            this.textBoxSalaryMinimum.TabIndex = 72;
            this.textBoxSalaryMinimum.Text = "Minimum";
            // 
            // textBoxSalaryMaximum
            // 
            this.textBoxSalaryMaximum.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxSalaryMaximum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxSalaryMaximum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSalaryMaximum.ForeColor = System.Drawing.Color.White;
            this.textBoxSalaryMaximum.Location = new System.Drawing.Point(259, 332);
            this.textBoxSalaryMaximum.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxSalaryMaximum.Name = "textBoxSalaryMaximum";
            this.textBoxSalaryMaximum.Size = new System.Drawing.Size(183, 26);
            this.textBoxSalaryMaximum.TabIndex = 73;
            this.textBoxSalaryMaximum.Text = "Maximum";
            // 
            // textBoxGrossPayMinimum
            // 
            this.textBoxGrossPayMinimum.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxGrossPayMinimum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxGrossPayMinimum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxGrossPayMinimum.ForeColor = System.Drawing.Color.White;
            this.textBoxGrossPayMinimum.Location = new System.Drawing.Point(39, 405);
            this.textBoxGrossPayMinimum.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxGrossPayMinimum.Name = "textBoxGrossPayMinimum";
            this.textBoxGrossPayMinimum.Size = new System.Drawing.Size(183, 26);
            this.textBoxGrossPayMinimum.TabIndex = 74;
            this.textBoxGrossPayMinimum.Text = "Minimum";
            // 
            // textBoxNetPayMinimum
            // 
            this.textBoxNetPayMinimum.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxNetPayMinimum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxNetPayMinimum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxNetPayMinimum.ForeColor = System.Drawing.Color.White;
            this.textBoxNetPayMinimum.Location = new System.Drawing.Point(39, 478);
            this.textBoxNetPayMinimum.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxNetPayMinimum.Name = "textBoxNetPayMinimum";
            this.textBoxNetPayMinimum.Size = new System.Drawing.Size(183, 26);
            this.textBoxNetPayMinimum.TabIndex = 75;
            this.textBoxNetPayMinimum.Text = "Minimum";
            // 
            // textBoxGrossPayMaximum
            // 
            this.textBoxGrossPayMaximum.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxGrossPayMaximum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxGrossPayMaximum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxGrossPayMaximum.ForeColor = System.Drawing.Color.White;
            this.textBoxGrossPayMaximum.Location = new System.Drawing.Point(259, 405);
            this.textBoxGrossPayMaximum.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxGrossPayMaximum.Name = "textBoxGrossPayMaximum";
            this.textBoxGrossPayMaximum.Size = new System.Drawing.Size(183, 26);
            this.textBoxGrossPayMaximum.TabIndex = 76;
            this.textBoxGrossPayMaximum.Text = "Maximum";
            // 
            // textBoxNetPayMaximum
            // 
            this.textBoxNetPayMaximum.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxNetPayMaximum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxNetPayMaximum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxNetPayMaximum.ForeColor = System.Drawing.Color.White;
            this.textBoxNetPayMaximum.Location = new System.Drawing.Point(259, 478);
            this.textBoxNetPayMaximum.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxNetPayMaximum.Name = "textBoxNetPayMaximum";
            this.textBoxNetPayMaximum.Size = new System.Drawing.Size(183, 26);
            this.textBoxNetPayMaximum.TabIndex = 77;
            this.textBoxNetPayMaximum.Text = "Maximum";
            // 
            // labelOvertimeHours
            // 
            this.labelOvertimeHours.AutoSize = true;
            this.labelOvertimeHours.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOvertimeHours.Location = new System.Drawing.Point(253, 516);
            this.labelOvertimeHours.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelOvertimeHours.Name = "labelOvertimeHours";
            this.labelOvertimeHours.Size = new System.Drawing.Size(206, 25);
            this.labelOvertimeHours.TabIndex = 80;
            this.labelOvertimeHours.Text = "OVERTIME HOURS";
            // 
            // comboBoxUnusedLeave
            // 
            this.comboBoxUnusedLeave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.comboBoxUnusedLeave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxUnusedLeave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxUnusedLeave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxUnusedLeave.ForeColor = System.Drawing.Color.White;
            this.comboBoxUnusedLeave.FormattingEnabled = true;
            this.comboBoxUnusedLeave.Location = new System.Drawing.Point(39, 548);
            this.comboBoxUnusedLeave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxUnusedLeave.Name = "comboBoxUnusedLeave";
            this.comboBoxUnusedLeave.Size = new System.Drawing.Size(183, 28);
            this.comboBoxUnusedLeave.TabIndex = 81;
            this.comboBoxUnusedLeave.Text = "Select no. of leaves";
            // 
            // comboBoxOvertimeHours
            // 
            this.comboBoxOvertimeHours.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.comboBoxOvertimeHours.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxOvertimeHours.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxOvertimeHours.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxOvertimeHours.ForeColor = System.Drawing.Color.White;
            this.comboBoxOvertimeHours.FormattingEnabled = true;
            this.comboBoxOvertimeHours.Location = new System.Drawing.Point(259, 548);
            this.comboBoxOvertimeHours.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxOvertimeHours.Name = "comboBoxOvertimeHours";
            this.comboBoxOvertimeHours.Size = new System.Drawing.Size(183, 28);
            this.comboBoxOvertimeHours.TabIndex = 82;
            this.comboBoxOvertimeHours.Text = "Select no. of hours";
            // 
            // labelDashA
            // 
            this.labelDashA.AutoSize = true;
            this.labelDashA.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDashA.Location = new System.Drawing.Point(231, 114);
            this.labelDashA.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDashA.Name = "labelDashA";
            this.labelDashA.Size = new System.Drawing.Size(20, 25);
            this.labelDashA.TabIndex = 86;
            this.labelDashA.Text = "-";
            // 
            // labelDashB
            // 
            this.labelDashB.AutoSize = true;
            this.labelDashB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDashB.Location = new System.Drawing.Point(231, 335);
            this.labelDashB.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDashB.Name = "labelDashB";
            this.labelDashB.Size = new System.Drawing.Size(20, 25);
            this.labelDashB.TabIndex = 87;
            this.labelDashB.Text = "-";
            // 
            // labelDashC
            // 
            this.labelDashC.AutoSize = true;
            this.labelDashC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDashC.Location = new System.Drawing.Point(231, 407);
            this.labelDashC.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDashC.Name = "labelDashC";
            this.labelDashC.Size = new System.Drawing.Size(20, 25);
            this.labelDashC.TabIndex = 88;
            this.labelDashC.Text = "-";
            // 
            // labelDashD
            // 
            this.labelDashD.AutoSize = true;
            this.labelDashD.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDashD.Location = new System.Drawing.Point(231, 480);
            this.labelDashD.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDashD.Name = "labelDashD";
            this.labelDashD.Size = new System.Drawing.Size(20, 25);
            this.labelDashD.TabIndex = 89;
            this.labelDashD.Text = "-";
            // 
            // buttonReset
            // 
            this.buttonReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(154)))), ((int)(((byte)(154)))));
            this.buttonReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReset.ForeColor = System.Drawing.Color.White;
            this.buttonReset.Location = new System.Drawing.Point(273, 617);
            this.buttonReset.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(176, 41);
            this.buttonReset.TabIndex = 91;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = false;
            // 
            // buttonApply
            // 
            this.buttonApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(81)))), ((int)(((byte)(148)))));
            this.buttonApply.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonApply.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonApply.ForeColor = System.Drawing.Color.White;
            this.buttonApply.Location = new System.Drawing.Point(48, 617);
            this.buttonApply.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(176, 41);
            this.buttonApply.TabIndex = 90;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = false;
            // 
            // FilterAdminPayroll
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(493, 668);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.labelDashD);
            this.Controls.Add(this.labelDashC);
            this.Controls.Add(this.labelDashB);
            this.Controls.Add(this.labelDashA);
            this.Controls.Add(this.comboBoxOvertimeHours);
            this.Controls.Add(this.comboBoxUnusedLeave);
            this.Controls.Add(this.labelOvertimeHours);
            this.Controls.Add(this.textBoxNetPayMaximum);
            this.Controls.Add(this.textBoxGrossPayMaximum);
            this.Controls.Add(this.textBoxNetPayMinimum);
            this.Controls.Add(this.textBoxGrossPayMinimum);
            this.Controls.Add(this.textBoxSalaryMaximum);
            this.Controls.Add(this.textBoxSalaryMinimum);
            this.Controls.Add(this.comboBoxSort);
            this.Controls.Add(this.labelSort);
            this.Controls.Add(this.pictureBoxSortLogo);
            this.Controls.Add(this.comboBoxPosition);
            this.Controls.Add(this.comboBoxDepartment);
            this.Controls.Add(this.labelPosition);
            this.Controls.Add(this.labelID);
            this.Controls.Add(this.textBoxID);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.textBoxEndDate);
            this.Controls.Add(this.textBoxStartDate);
            this.Controls.Add(this.labelUnusedLeave);
            this.Controls.Add(this.labelNetPay);
            this.Controls.Add(this.labelGrossPay);
            this.Controls.Add(this.labelSalary);
            this.Controls.Add(this.labelDepartment);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.labelDateRange);
            this.Controls.Add(this.labelSearchFilters);
            this.Controls.Add(this.XpictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FilterAdminPayroll";
            this.ShowInTaskbar = false;
            this.Text = "FilterAdminPayroll";
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSortLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSearchFilters;
        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Label labelDateRange;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelDepartment;
        private System.Windows.Forms.Label labelSalary;
        private System.Windows.Forms.Label labelGrossPay;
        private System.Windows.Forms.Label labelNetPay;
        private System.Windows.Forms.Label labelUnusedLeave;
        private System.Windows.Forms.TextBox textBoxStartDate;
        private System.Windows.Forms.TextBox textBoxEndDate;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.TextBox textBoxID;
        private System.Windows.Forms.Label labelID;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.ComboBox comboBoxDepartment;
        private System.Windows.Forms.ComboBox comboBoxPosition;
        private System.Windows.Forms.ComboBox comboBoxSort;
        private System.Windows.Forms.Label labelSort;
        private System.Windows.Forms.PictureBox pictureBoxSortLogo;
        private System.Windows.Forms.TextBox textBoxSalaryMinimum;
        private System.Windows.Forms.TextBox textBoxSalaryMaximum;
        private System.Windows.Forms.TextBox textBoxGrossPayMinimum;
        private System.Windows.Forms.TextBox textBoxNetPayMinimum;
        private System.Windows.Forms.TextBox textBoxGrossPayMaximum;
        private System.Windows.Forms.TextBox textBoxNetPayMaximum;
        private System.Windows.Forms.Label labelOvertimeHours;
        private System.Windows.Forms.ComboBox comboBoxUnusedLeave;
        private System.Windows.Forms.ComboBox comboBoxOvertimeHours;
        private System.Windows.Forms.Label labelDashA;
        private System.Windows.Forms.Label labelDashB;
        private System.Windows.Forms.Label labelDashC;
        private System.Windows.Forms.Label labelDashD;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonApply;
    }
}