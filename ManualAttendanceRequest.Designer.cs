namespace HRIS_JAP_ATTPAY
{
    partial class ManualAttendanceRequest
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.XpictureBox = new System.Windows.Forms.PictureBox();
            this.labelRequestAttendanceEntry = new System.Windows.Forms.Label();
            this.labelManualAttendanceRequest = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.labelStatusInput = new System.Windows.Forms.Label();
            this.labelDateInput = new System.Windows.Forms.Label();
            this.labelNameInput = new System.Windows.Forms.Label();
            this.labelIDInput = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelDate = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.labelID = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.labelOverTimeOut = new System.Windows.Forms.Label();
            this.labelOverTimeIn = new System.Windows.Forms.Label();
            this.labelTimeOut = new System.Windows.Forms.Label();
            this.labelTimeIn = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSendRequest = new System.Windows.Forms.Button();
            this.textBoxOverTimeOut = new System.Windows.Forms.TextBox();
            this.textBoxOverTimeIn = new System.Windows.Forms.TextBox();
            this.textBoxTimeOut = new System.Windows.Forms.TextBox();
            this.textBoxTimeIn = new System.Windows.Forms.TextBox();
            this.labelOvertimeInput = new System.Windows.Forms.Label();
            this.labelOvertime = new System.Windows.Forms.Label();
            this.labelHoursWorkedInput = new System.Windows.Forms.Label();
            this.labelHoursWorked = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.panel2.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.XpictureBox);
            this.panel1.Controls.Add(this.labelRequestAttendanceEntry);
            this.panel1.Controls.Add(this.labelManualAttendanceRequest);
            this.panel1.Location = new System.Drawing.Point(-1, -1);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(717, 105);
            this.panel1.TabIndex = 0;
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(657, 4);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(51, 50);
            this.XpictureBox.TabIndex = 1;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelRequestAttendanceEntry
            // 
            this.labelRequestAttendanceEntry.AutoSize = true;
            this.labelRequestAttendanceEntry.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRequestAttendanceEntry.ForeColor = System.Drawing.Color.Black;
            this.labelRequestAttendanceEntry.Location = new System.Drawing.Point(41, 70);
            this.labelRequestAttendanceEntry.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelRequestAttendanceEntry.Name = "labelRequestAttendanceEntry";
            this.labelRequestAttendanceEntry.Size = new System.Drawing.Size(229, 24);
            this.labelRequestAttendanceEntry.TabIndex = 0;
            this.labelRequestAttendanceEntry.Text = "Request Attendance Entry";
            // 
            // labelManualAttendanceRequest
            // 
            this.labelManualAttendanceRequest.AutoSize = true;
            this.labelManualAttendanceRequest.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelManualAttendanceRequest.ForeColor = System.Drawing.Color.Red;
            this.labelManualAttendanceRequest.Location = new System.Drawing.Point(39, 23);
            this.labelManualAttendanceRequest.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelManualAttendanceRequest.Name = "labelManualAttendanceRequest";
            this.labelManualAttendanceRequest.Size = new System.Drawing.Size(391, 36);
            this.labelManualAttendanceRequest.TabIndex = 0;
            this.labelManualAttendanceRequest.Text = "Manual Attendance Request";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.panelStatus);
            this.panel2.Controls.Add(this.labelDateInput);
            this.panel2.Controls.Add(this.labelNameInput);
            this.panel2.Controls.Add(this.labelIDInput);
            this.panel2.Controls.Add(this.labelStatus);
            this.panel2.Controls.Add(this.labelDate);
            this.panel2.Controls.Add(this.labelName);
            this.panel2.Controls.Add(this.labelID);
            this.panel2.Location = new System.Drawing.Point(0, 106);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(716, 96);
            this.panel2.TabIndex = 1;
            // 
            // panelStatus
            // 
            this.panelStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(163)))), ((int)(((byte)(64)))));
            this.panelStatus.Controls.Add(this.labelStatusInput);
            this.panelStatus.ForeColor = System.Drawing.Color.White;
            this.panelStatus.Location = new System.Drawing.Point(555, 46);
            this.panelStatus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(120, 43);
            this.panelStatus.TabIndex = 0;
            // 
            // labelStatusInput
            // 
            this.labelStatusInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(163)))), ((int)(((byte)(64)))));
            this.labelStatusInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatusInput.Location = new System.Drawing.Point(0, 0);
            this.labelStatusInput.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStatusInput.Name = "labelStatusInput";
            this.labelStatusInput.Size = new System.Drawing.Size(120, 43);
            this.labelStatusInput.TabIndex = 7;
            this.labelStatusInput.Text = "N/A";
            this.labelStatusInput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDateInput
            // 
            this.labelDateInput.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.labelDateInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDateInput.Location = new System.Drawing.Point(380, 52);
            this.labelDateInput.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDateInput.Name = "labelDateInput";
            this.labelDateInput.Size = new System.Drawing.Size(129, 31);
            this.labelDateInput.TabIndex = 6;
            this.labelDateInput.Text = "N/A";
            this.labelDateInput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelNameInput
            // 
            this.labelNameInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelNameInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNameInput.Location = new System.Drawing.Point(173, 52);
            this.labelNameInput.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelNameInput.Name = "labelNameInput";
            this.labelNameInput.Size = new System.Drawing.Size(179, 31);
            this.labelNameInput.TabIndex = 5;
            this.labelNameInput.Text = "N/A";
            this.labelNameInput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelIDInput
            // 
            this.labelIDInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelIDInput.Location = new System.Drawing.Point(40, 52);
            this.labelIDInput.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIDInput.Name = "labelIDInput";
            this.labelIDInput.Size = new System.Drawing.Size(125, 31);
            this.labelIDInput.TabIndex = 4;
            this.labelIDInput.Text = "N/A";
            this.labelIDInput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(577, 16);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(66, 24);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Status";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDate
            // 
            this.labelDate.AutoSize = true;
            this.labelDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDate.Location = new System.Drawing.Point(419, 16);
            this.labelDate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(52, 24);
            this.labelDate.TabIndex = 2;
            this.labelDate.Text = "Date";
            this.labelDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelName.Location = new System.Drawing.Point(231, 16);
            this.labelName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(65, 24);
            this.labelName.TabIndex = 1;
            this.labelName.Text = "Name";
            this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelID
            // 
            this.labelID.AutoSize = true;
            this.labelID.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelID.Location = new System.Drawing.Point(85, 16);
            this.labelID.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelID.Name = "labelID";
            this.labelID.Size = new System.Drawing.Size(29, 24);
            this.labelID.TabIndex = 0;
            this.labelID.Text = "ID";
            this.labelID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(137)))), ((int)(((byte)(207)))));
            this.panel3.Controls.Add(this.labelOverTimeOut);
            this.panel3.Controls.Add(this.labelOverTimeIn);
            this.panel3.Controls.Add(this.labelTimeOut);
            this.panel3.Controls.Add(this.labelTimeIn);
            this.panel3.ForeColor = System.Drawing.Color.White;
            this.panel3.Location = new System.Drawing.Point(0, 202);
            this.panel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(716, 58);
            this.panel3.TabIndex = 2;
            // 
            // labelOverTimeOut
            // 
            this.labelOverTimeOut.AutoSize = true;
            this.labelOverTimeOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOverTimeOut.Location = new System.Drawing.Point(541, 15);
            this.labelOverTimeOut.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelOverTimeOut.Name = "labelOverTimeOut";
            this.labelOverTimeOut.Size = new System.Drawing.Size(140, 25);
            this.labelOverTimeOut.TabIndex = 3;
            this.labelOverTimeOut.Text = "Overtime Out";
            this.labelOverTimeOut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelOverTimeIn
            // 
            this.labelOverTimeIn.AutoSize = true;
            this.labelOverTimeIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOverTimeIn.Location = new System.Drawing.Point(380, 15);
            this.labelOverTimeIn.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelOverTimeIn.Name = "labelOverTimeIn";
            this.labelOverTimeIn.Size = new System.Drawing.Size(123, 25);
            this.labelOverTimeIn.TabIndex = 2;
            this.labelOverTimeIn.Text = "Overtime In";
            this.labelOverTimeIn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelTimeOut
            // 
            this.labelTimeOut.AutoSize = true;
            this.labelTimeOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimeOut.Location = new System.Drawing.Point(216, 15);
            this.labelTimeOut.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTimeOut.Name = "labelTimeOut";
            this.labelTimeOut.Size = new System.Drawing.Size(101, 25);
            this.labelTimeOut.TabIndex = 1;
            this.labelTimeOut.Text = "Time Out";
            this.labelTimeOut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelTimeIn
            // 
            this.labelTimeIn.AutoSize = true;
            this.labelTimeIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimeIn.Location = new System.Drawing.Point(55, 15);
            this.labelTimeIn.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTimeIn.Name = "labelTimeIn";
            this.labelTimeIn.Size = new System.Drawing.Size(84, 25);
            this.labelTimeIn.TabIndex = 0;
            this.labelTimeIn.Text = "Time In";
            this.labelTimeIn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.Controls.Add(this.buttonCancel);
            this.panel4.Controls.Add(this.buttonSendRequest);
            this.panel4.Controls.Add(this.textBoxOverTimeOut);
            this.panel4.Controls.Add(this.textBoxOverTimeIn);
            this.panel4.Controls.Add(this.textBoxTimeOut);
            this.panel4.Controls.Add(this.textBoxTimeIn);
            this.panel4.Controls.Add(this.labelOvertimeInput);
            this.panel4.Controls.Add(this.labelOvertime);
            this.panel4.Controls.Add(this.labelHoursWorkedInput);
            this.panel4.Controls.Add(this.labelHoursWorked);
            this.panel4.Location = new System.Drawing.Point(0, 257);
            this.panel4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(716, 170);
            this.panel4.TabIndex = 3;
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(154)))), ((int)(((byte)(154)))));
            this.buttonCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(565, 117);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(117, 37);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSendRequest
            // 
            this.buttonSendRequest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(81)))), ((int)(((byte)(148)))));
            this.buttonSendRequest.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSendRequest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSendRequest.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSendRequest.ForeColor = System.Drawing.Color.White;
            this.buttonSendRequest.Location = new System.Drawing.Point(373, 117);
            this.buttonSendRequest.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonSendRequest.Name = "buttonSendRequest";
            this.buttonSendRequest.Size = new System.Drawing.Size(180, 37);
            this.buttonSendRequest.TabIndex = 9;
            this.buttonSendRequest.Text = "Send Request";
            this.buttonSendRequest.UseVisualStyleBackColor = false;
            this.buttonSendRequest.Click += new System.EventHandler(this.buttonSendRequest_Click);
            // 
            // textBoxOverTimeOut
            // 
            this.textBoxOverTimeOut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxOverTimeOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxOverTimeOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxOverTimeOut.ForeColor = System.Drawing.Color.White;
            this.textBoxOverTimeOut.Location = new System.Drawing.Point(552, 18);
            this.textBoxOverTimeOut.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxOverTimeOut.Name = "textBoxOverTimeOut";
            this.textBoxOverTimeOut.Size = new System.Drawing.Size(125, 30);
            this.textBoxOverTimeOut.TabIndex = 8;
            this.textBoxOverTimeOut.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxOverTimeIn
            // 
            this.textBoxOverTimeIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxOverTimeIn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxOverTimeIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxOverTimeIn.ForeColor = System.Drawing.Color.White;
            this.textBoxOverTimeIn.Location = new System.Drawing.Point(384, 18);
            this.textBoxOverTimeIn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxOverTimeIn.Name = "textBoxOverTimeIn";
            this.textBoxOverTimeIn.Size = new System.Drawing.Size(125, 30);
            this.textBoxOverTimeIn.TabIndex = 7;
            this.textBoxOverTimeIn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxTimeOut
            // 
            this.textBoxTimeOut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxTimeOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxTimeOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTimeOut.ForeColor = System.Drawing.Color.White;
            this.textBoxTimeOut.Location = new System.Drawing.Point(205, 18);
            this.textBoxTimeOut.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxTimeOut.Name = "textBoxTimeOut";
            this.textBoxTimeOut.Size = new System.Drawing.Size(125, 30);
            this.textBoxTimeOut.TabIndex = 6;
            this.textBoxTimeOut.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxTimeIn
            // 
            this.textBoxTimeIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxTimeIn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxTimeIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTimeIn.ForeColor = System.Drawing.Color.White;
            this.textBoxTimeIn.Location = new System.Drawing.Point(36, 18);
            this.textBoxTimeIn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxTimeIn.Name = "textBoxTimeIn";
            this.textBoxTimeIn.Size = new System.Drawing.Size(125, 30);
            this.textBoxTimeIn.TabIndex = 0;
            this.textBoxTimeIn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelOvertimeInput
            // 
            this.labelOvertimeInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOvertimeInput.Location = new System.Drawing.Point(496, 79);
            this.labelOvertimeInput.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelOvertimeInput.Name = "labelOvertimeInput";
            this.labelOvertimeInput.Size = new System.Drawing.Size(160, 22);
            this.labelOvertimeInput.TabIndex = 5;
            this.labelOvertimeInput.Text = "N/A";
            this.labelOvertimeInput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelOvertime
            // 
            this.labelOvertime.AutoSize = true;
            this.labelOvertime.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOvertime.Location = new System.Drawing.Point(387, 79);
            this.labelOvertime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelOvertime.Name = "labelOvertime";
            this.labelOvertime.Size = new System.Drawing.Size(94, 24);
            this.labelOvertime.TabIndex = 4;
            this.labelOvertime.Text = "Overtime";
            // 
            // labelHoursWorkedInput
            // 
            this.labelHoursWorkedInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHoursWorkedInput.Location = new System.Drawing.Point(193, 79);
            this.labelHoursWorkedInput.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHoursWorkedInput.Name = "labelHoursWorkedInput";
            this.labelHoursWorkedInput.Size = new System.Drawing.Size(159, 22);
            this.labelHoursWorkedInput.TabIndex = 3;
            this.labelHoursWorkedInput.Text = "N/A";
            this.labelHoursWorkedInput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelHoursWorked
            // 
            this.labelHoursWorked.AutoSize = true;
            this.labelHoursWorked.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHoursWorked.Location = new System.Drawing.Point(28, 79);
            this.labelHoursWorked.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHoursWorked.Name = "labelHoursWorked";
            this.labelHoursWorked.Size = new System.Drawing.Size(144, 24);
            this.labelHoursWorked.TabIndex = 2;
            this.labelHoursWorked.Text = "Hours Worked";
            // 
            // ManualAttendanceRequest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(713, 428);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ManualAttendanceRequest";
            this.ShowInTaskbar = false;
            this.Text = "EditAttendance";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panelStatus.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label labelManualAttendanceRequest;
        private System.Windows.Forms.Label labelRequestAttendanceEntry;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelID;
        private System.Windows.Forms.Label labelDateInput;
        private System.Windows.Forms.Label labelNameInput;
        private System.Windows.Forms.Label labelIDInput;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.Label labelStatusInput;
        private System.Windows.Forms.Label labelOverTimeOut;
        private System.Windows.Forms.Label labelOverTimeIn;
        private System.Windows.Forms.Label labelTimeOut;
        private System.Windows.Forms.Label labelTimeIn;
        private System.Windows.Forms.Label labelHoursWorked;
        private System.Windows.Forms.Label labelOvertimeInput;
        private System.Windows.Forms.Label labelOvertime;
        private System.Windows.Forms.Label labelHoursWorkedInput;
        private System.Windows.Forms.TextBox textBoxTimeIn;
        private System.Windows.Forms.TextBox textBoxOverTimeOut;
        private System.Windows.Forms.TextBox textBoxOverTimeIn;
        private System.Windows.Forms.TextBox textBoxTimeOut;
        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSendRequest;
    }
}