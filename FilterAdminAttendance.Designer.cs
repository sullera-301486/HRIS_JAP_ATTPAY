namespace HRIS_JAP_ATTPAY
{
    partial class FilterAdminAttendance
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
            this.pictureBoxSortLogo = new System.Windows.Forms.PictureBox();
            this.XpictureBox = new System.Windows.Forms.PictureBox();
            this.labelSearchFilters = new System.Windows.Forms.Label();
            this.labelSort = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.labelID = new System.Windows.Forms.Label();
            this.labelDepartment = new System.Windows.Forms.Label();
            this.labelPosition = new System.Windows.Forms.Label();
            this.comboBoxSort = new System.Windows.Forms.ComboBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.textBoxID = new System.Windows.Forms.TextBox();
            this.comboBoxDepartment = new System.Windows.Forms.ComboBox();
            this.comboBoxPosition = new System.Windows.Forms.ComboBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.checkBoxPresent = new System.Windows.Forms.CheckBox();
            this.checkBoxLate = new System.Windows.Forms.CheckBox();
            this.checkBoxAbsent = new System.Windows.Forms.CheckBox();
            this.checkBoxEarlyOut = new System.Windows.Forms.CheckBox();
            this.labelTimeIn = new System.Windows.Forms.Label();
            this.labelTimeOut = new System.Windows.Forms.Label();
            this.textBoxTimeIn = new System.Windows.Forms.TextBox();
            this.textBoxTimeOut = new System.Windows.Forms.TextBox();
            this.labelHoursWorked = new System.Windows.Forms.Label();
            this.labelOvertime = new System.Windows.Forms.Label();
            this.checkBoxEightHours = new System.Windows.Forms.CheckBox();
            this.checkBoxBelowEightHours = new System.Windows.Forms.CheckBox();
            this.checkBoxOneHour = new System.Windows.Forms.CheckBox();
            this.checkBoxAboveTwoHours = new System.Windows.Forms.CheckBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSortLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxSortLogo
            // 
            this.pictureBoxSortLogo.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.Filter;
            this.pictureBoxSortLogo.Location = new System.Drawing.Point(305, 79);
            this.pictureBoxSortLogo.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBoxSortLogo.Name = "pictureBoxSortLogo";
            this.pictureBoxSortLogo.Size = new System.Drawing.Size(32, 33);
            this.pictureBoxSortLogo.TabIndex = 20;
            this.pictureBoxSortLogo.TabStop = false;
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(438, 1);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(51, 50);
            this.XpictureBox.TabIndex = 19;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelSearchFilters
            // 
            this.labelSearchFilters.AutoSize = true;
            this.labelSearchFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSearchFilters.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(34)))), ((int)(((byte)(31)))));
            this.labelSearchFilters.Location = new System.Drawing.Point(15, 14);
            this.labelSearchFilters.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSearchFilters.Name = "labelSearchFilters";
            this.labelSearchFilters.Size = new System.Drawing.Size(198, 36);
            this.labelSearchFilters.TabIndex = 21;
            this.labelSearchFilters.Text = "Search Filters";
            // 
            // labelSort
            // 
            this.labelSort.AutoSize = true;
            this.labelSort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSort.Location = new System.Drawing.Point(253, 79);
            this.labelSort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSort.Name = "labelSort";
            this.labelSort.Size = new System.Drawing.Size(52, 25);
            this.labelSort.TabIndex = 23;
            this.labelSort.Text = "Sort";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelName.Location = new System.Drawing.Point(32, 114);
            this.labelName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(74, 25);
            this.labelName.TabIndex = 24;
            this.labelName.Text = "NAME";
            // 
            // labelID
            // 
            this.labelID.AutoSize = true;
            this.labelID.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelID.Location = new System.Drawing.Point(253, 114);
            this.labelID.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelID.Name = "labelID";
            this.labelID.Size = new System.Drawing.Size(33, 25);
            this.labelID.TabIndex = 25;
            this.labelID.Text = "ID";
            // 
            // labelDepartment
            // 
            this.labelDepartment.AutoSize = true;
            this.labelDepartment.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDepartment.Location = new System.Drawing.Point(32, 183);
            this.labelDepartment.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDepartment.Name = "labelDepartment";
            this.labelDepartment.Size = new System.Drawing.Size(159, 25);
            this.labelDepartment.TabIndex = 26;
            this.labelDepartment.Text = "DEPARTMENT";
            // 
            // labelPosition
            // 
            this.labelPosition.AutoSize = true;
            this.labelPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPosition.Location = new System.Drawing.Point(253, 183);
            this.labelPosition.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(116, 25);
            this.labelPosition.TabIndex = 27;
            this.labelPosition.Text = "POSITION";
            // 
            // comboBoxSort
            // 
            this.comboBoxSort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.comboBoxSort.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxSort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxSort.ForeColor = System.Drawing.Color.White;
            this.comboBoxSort.FormattingEnabled = true;
            this.comboBoxSort.Location = new System.Drawing.Point(343, 79);
            this.comboBoxSort.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxSort.Name = "comboBoxSort";
            this.comboBoxSort.Size = new System.Drawing.Size(113, 28);
            this.comboBoxSort.TabIndex = 28;
            // 
            // textBoxName
            // 
            this.textBoxName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxName.ForeColor = System.Drawing.Color.White;
            this.textBoxName.Location = new System.Drawing.Point(37, 143);
            this.textBoxName.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(197, 26);
            this.textBoxName.TabIndex = 30;
            this.textBoxName.Text = "Search name";
            // 
            // textBoxID
            // 
            this.textBoxID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxID.ForeColor = System.Drawing.Color.White;
            this.textBoxID.Location = new System.Drawing.Point(258, 143);
            this.textBoxID.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxID.Name = "textBoxID";
            this.textBoxID.Size = new System.Drawing.Size(194, 26);
            this.textBoxID.TabIndex = 31;
            this.textBoxID.Text = "Search ID";
            // 
            // comboBoxDepartment
            // 
            this.comboBoxDepartment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.comboBoxDepartment.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxDepartment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDepartment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxDepartment.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxDepartment.ForeColor = System.Drawing.Color.White;
            this.comboBoxDepartment.FormattingEnabled = true;
            this.comboBoxDepartment.Location = new System.Drawing.Point(37, 212);
            this.comboBoxDepartment.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxDepartment.Name = "comboBoxDepartment";
            this.comboBoxDepartment.Size = new System.Drawing.Size(196, 28);
            this.comboBoxDepartment.TabIndex = 32;
            // 
            // comboBoxPosition
            // 
            this.comboBoxPosition.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.comboBoxPosition.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPosition.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxPosition.ForeColor = System.Drawing.Color.White;
            this.comboBoxPosition.FormattingEnabled = true;
            this.comboBoxPosition.Location = new System.Drawing.Point(258, 212);
            this.comboBoxPosition.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPosition.Name = "comboBoxPosition";
            this.comboBoxPosition.Size = new System.Drawing.Size(194, 28);
            this.comboBoxPosition.TabIndex = 33;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(32, 255);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(100, 25);
            this.labelStatus.TabIndex = 34;
            this.labelStatus.Text = "STATUS";
            // 
            // checkBoxPresent
            // 
            this.checkBoxPresent.AutoSize = true;
            this.checkBoxPresent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkBoxPresent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxPresent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxPresent.Location = new System.Drawing.Point(37, 293);
            this.checkBoxPresent.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxPresent.Name = "checkBoxPresent";
            this.checkBoxPresent.Size = new System.Drawing.Size(97, 29);
            this.checkBoxPresent.TabIndex = 35;
            this.checkBoxPresent.Text = "Present";
            this.checkBoxPresent.UseVisualStyleBackColor = true;
            // 
            // checkBoxLate
            // 
            this.checkBoxLate.AutoSize = true;
            this.checkBoxLate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkBoxLate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxLate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxLate.Location = new System.Drawing.Point(37, 330);
            this.checkBoxLate.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxLate.Name = "checkBoxLate";
            this.checkBoxLate.Size = new System.Drawing.Size(68, 29);
            this.checkBoxLate.TabIndex = 36;
            this.checkBoxLate.Text = "Late";
            this.checkBoxLate.UseVisualStyleBackColor = true;
            // 
            // checkBoxAbsent
            // 
            this.checkBoxAbsent.AutoSize = true;
            this.checkBoxAbsent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkBoxAbsent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxAbsent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxAbsent.Location = new System.Drawing.Point(258, 293);
            this.checkBoxAbsent.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxAbsent.Name = "checkBoxAbsent";
            this.checkBoxAbsent.Size = new System.Drawing.Size(92, 29);
            this.checkBoxAbsent.TabIndex = 37;
            this.checkBoxAbsent.Text = "Absent";
            this.checkBoxAbsent.UseVisualStyleBackColor = true;
            // 
            // checkBoxEarlyOut
            // 
            this.checkBoxEarlyOut.AutoSize = true;
            this.checkBoxEarlyOut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkBoxEarlyOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxEarlyOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEarlyOut.Location = new System.Drawing.Point(258, 330);
            this.checkBoxEarlyOut.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxEarlyOut.Name = "checkBoxEarlyOut";
            this.checkBoxEarlyOut.Size = new System.Drawing.Size(111, 29);
            this.checkBoxEarlyOut.TabIndex = 38;
            this.checkBoxEarlyOut.Text = "Early Out";
            this.checkBoxEarlyOut.UseVisualStyleBackColor = true;
            // 
            // labelTimeIn
            // 
            this.labelTimeIn.AutoSize = true;
            this.labelTimeIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimeIn.Location = new System.Drawing.Point(32, 382);
            this.labelTimeIn.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTimeIn.Name = "labelTimeIn";
            this.labelTimeIn.Size = new System.Drawing.Size(91, 25);
            this.labelTimeIn.TabIndex = 39;
            this.labelTimeIn.Text = "TIME IN";
            // 
            // labelTimeOut
            // 
            this.labelTimeOut.AutoSize = true;
            this.labelTimeOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimeOut.Location = new System.Drawing.Point(253, 382);
            this.labelTimeOut.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTimeOut.Name = "labelTimeOut";
            this.labelTimeOut.Size = new System.Drawing.Size(116, 25);
            this.labelTimeOut.TabIndex = 40;
            this.labelTimeOut.Text = "TIME OUT";
            // 
            // textBoxTimeIn
            // 
            this.textBoxTimeIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxTimeIn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxTimeIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTimeIn.ForeColor = System.Drawing.Color.White;
            this.textBoxTimeIn.Location = new System.Drawing.Point(37, 411);
            this.textBoxTimeIn.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxTimeIn.Name = "textBoxTimeIn";
            this.textBoxTimeIn.Size = new System.Drawing.Size(197, 26);
            this.textBoxTimeIn.TabIndex = 41;
            this.textBoxTimeIn.Text = "Select time";
            // 
            // textBoxTimeOut
            // 
            this.textBoxTimeOut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxTimeOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxTimeOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTimeOut.ForeColor = System.Drawing.Color.White;
            this.textBoxTimeOut.Location = new System.Drawing.Point(258, 411);
            this.textBoxTimeOut.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxTimeOut.Name = "textBoxTimeOut";
            this.textBoxTimeOut.Size = new System.Drawing.Size(194, 26);
            this.textBoxTimeOut.TabIndex = 42;
            this.textBoxTimeOut.Text = "Select time";
            // 
            // labelHoursWorked
            // 
            this.labelHoursWorked.AutoSize = true;
            this.labelHoursWorked.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHoursWorked.Location = new System.Drawing.Point(32, 458);
            this.labelHoursWorked.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHoursWorked.Name = "labelHoursWorked";
            this.labelHoursWorked.Size = new System.Drawing.Size(190, 25);
            this.labelHoursWorked.TabIndex = 43;
            this.labelHoursWorked.Text = "HOURS WORKED";
            // 
            // labelOvertime
            // 
            this.labelOvertime.AutoSize = true;
            this.labelOvertime.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOvertime.Location = new System.Drawing.Point(253, 458);
            this.labelOvertime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelOvertime.Name = "labelOvertime";
            this.labelOvertime.Size = new System.Drawing.Size(124, 25);
            this.labelOvertime.TabIndex = 44;
            this.labelOvertime.Text = "OVERTIME";
            // 
            // checkBoxEightHours
            // 
            this.checkBoxEightHours.AutoSize = true;
            this.checkBoxEightHours.BackColor = System.Drawing.Color.White;
            this.checkBoxEightHours.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkBoxEightHours.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxEightHours.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEightHours.Location = new System.Drawing.Point(37, 487);
            this.checkBoxEightHours.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxEightHours.Name = "checkBoxEightHours";
            this.checkBoxEightHours.Size = new System.Drawing.Size(98, 29);
            this.checkBoxEightHours.TabIndex = 45;
            this.checkBoxEightHours.Text = "8 Hours";
            this.checkBoxEightHours.UseVisualStyleBackColor = false;
            // 
            // checkBoxBelowEightHours
            // 
            this.checkBoxBelowEightHours.AutoSize = true;
            this.checkBoxBelowEightHours.BackColor = System.Drawing.Color.White;
            this.checkBoxBelowEightHours.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkBoxBelowEightHours.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxBelowEightHours.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxBelowEightHours.Location = new System.Drawing.Point(37, 524);
            this.checkBoxBelowEightHours.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxBelowEightHours.Name = "checkBoxBelowEightHours";
            this.checkBoxBelowEightHours.Size = new System.Drawing.Size(156, 29);
            this.checkBoxBelowEightHours.TabIndex = 46;
            this.checkBoxBelowEightHours.Text = "Below 8 Hours";
            this.checkBoxBelowEightHours.UseVisualStyleBackColor = false;
            // 
            // checkBoxOneHour
            // 
            this.checkBoxOneHour.AutoSize = true;
            this.checkBoxOneHour.BackColor = System.Drawing.Color.White;
            this.checkBoxOneHour.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkBoxOneHour.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxOneHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxOneHour.Location = new System.Drawing.Point(258, 487);
            this.checkBoxOneHour.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxOneHour.Name = "checkBoxOneHour";
            this.checkBoxOneHour.Size = new System.Drawing.Size(88, 29);
            this.checkBoxOneHour.TabIndex = 47;
            this.checkBoxOneHour.Text = "1 Hour";
            this.checkBoxOneHour.UseVisualStyleBackColor = false;
            // 
            // checkBoxAboveTwoHours
            // 
            this.checkBoxAboveTwoHours.AutoSize = true;
            this.checkBoxAboveTwoHours.BackColor = System.Drawing.Color.White;
            this.checkBoxAboveTwoHours.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkBoxAboveTwoHours.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxAboveTwoHours.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxAboveTwoHours.Location = new System.Drawing.Point(258, 524);
            this.checkBoxAboveTwoHours.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxAboveTwoHours.Name = "checkBoxAboveTwoHours";
            this.checkBoxAboveTwoHours.Size = new System.Drawing.Size(115, 29);
            this.checkBoxAboveTwoHours.TabIndex = 48;
            this.checkBoxAboveTwoHours.Text = "2 Hours +";
            this.checkBoxAboveTwoHours.UseVisualStyleBackColor = false;
            // 
            // buttonApply
            // 
            this.buttonApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(81)))), ((int)(((byte)(148)))));
            this.buttonApply.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonApply.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonApply.ForeColor = System.Drawing.Color.White;
            this.buttonApply.Location = new System.Drawing.Point(57, 594);
            this.buttonApply.Margin = new System.Windows.Forms.Padding(4);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(176, 41);
            this.buttonApply.TabIndex = 49;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = false;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(154)))), ((int)(((byte)(154)))));
            this.buttonReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReset.ForeColor = System.Drawing.Color.White;
            this.buttonReset.Location = new System.Drawing.Point(258, 594);
            this.buttonReset.Margin = new System.Windows.Forms.Padding(4);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(176, 41);
            this.buttonReset.TabIndex = 50;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = false;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // FilterAdminAttendance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(493, 661);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.checkBoxAboveTwoHours);
            this.Controls.Add(this.checkBoxOneHour);
            this.Controls.Add(this.checkBoxBelowEightHours);
            this.Controls.Add(this.checkBoxEightHours);
            this.Controls.Add(this.labelOvertime);
            this.Controls.Add(this.labelHoursWorked);
            this.Controls.Add(this.textBoxTimeOut);
            this.Controls.Add(this.textBoxTimeIn);
            this.Controls.Add(this.labelTimeOut);
            this.Controls.Add(this.labelTimeIn);
            this.Controls.Add(this.checkBoxEarlyOut);
            this.Controls.Add(this.checkBoxAbsent);
            this.Controls.Add(this.checkBoxLate);
            this.Controls.Add(this.checkBoxPresent);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.comboBoxPosition);
            this.Controls.Add(this.comboBoxDepartment);
            this.Controls.Add(this.textBoxID);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.comboBoxSort);
            this.Controls.Add(this.labelPosition);
            this.Controls.Add(this.labelDepartment);
            this.Controls.Add(this.labelID);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.labelSort);
            this.Controls.Add(this.labelSearchFilters);
            this.Controls.Add(this.pictureBoxSortLogo);
            this.Controls.Add(this.XpictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FilterAdminAttendance";
            this.ShowInTaskbar = false;
            this.Text = "SearchFilters";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSortLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.PictureBox pictureBoxSortLogo;
        private System.Windows.Forms.Label labelSearchFilters;
        private System.Windows.Forms.Label labelSort;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelID;
        private System.Windows.Forms.Label labelDepartment;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.ComboBox comboBoxSort;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.TextBox textBoxID;
        private System.Windows.Forms.ComboBox comboBoxDepartment;
        private System.Windows.Forms.ComboBox comboBoxPosition;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.CheckBox checkBoxPresent;
        private System.Windows.Forms.CheckBox checkBoxLate;
        private System.Windows.Forms.CheckBox checkBoxAbsent;
        private System.Windows.Forms.CheckBox checkBoxEarlyOut;
        private System.Windows.Forms.Label labelTimeIn;
        private System.Windows.Forms.Label labelTimeOut;
        private System.Windows.Forms.TextBox textBoxTimeIn;
        private System.Windows.Forms.TextBox textBoxTimeOut;
        private System.Windows.Forms.Label labelHoursWorked;
        private System.Windows.Forms.Label labelOvertime;
        private System.Windows.Forms.CheckBox checkBoxEightHours;
        private System.Windows.Forms.CheckBox checkBoxBelowEightHours;
        private System.Windows.Forms.CheckBox checkBoxOneHour;
        private System.Windows.Forms.CheckBox checkBoxAboveTwoHours;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonReset;
    }
}