namespace HRIS_JAP_ATTPAY
{
    partial class LeaveRequest
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
            this.labelRequestLeaveEntry = new System.Windows.Forms.Label();
            this.labelLeaveRequest = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelDash = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSendRequest = new System.Windows.Forms.Button();
            this.textBoxReasonInput = new System.Windows.Forms.TextBox();
            this.textBoxEndPeriod = new System.Windows.Forms.TextBox();
            this.textBoxStartPeriod = new System.Windows.Forms.TextBox();
            this.comboBoxLeaveTypeInput = new System.Windows.Forms.ComboBox();
            this.labelReason = new System.Windows.Forms.Label();
            this.labelPeriod = new System.Windows.Forms.Label();
            this.labelLeaveType = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.comboBoxName = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.XpictureBox);
            this.panel1.Controls.Add(this.labelRequestLeaveEntry);
            this.panel1.Controls.Add(this.labelLeaveRequest);
            this.panel1.Location = new System.Drawing.Point(-2, -1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(441, 88);
            this.panel1.TabIndex = 1;
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(398, 2);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(38, 41);
            this.XpictureBox.TabIndex = 19;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelRequestLeaveEntry
            // 
            this.labelRequestLeaveEntry.AutoSize = true;
            this.labelRequestLeaveEntry.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRequestLeaveEntry.ForeColor = System.Drawing.Color.Black;
            this.labelRequestLeaveEntry.Location = new System.Drawing.Point(31, 57);
            this.labelRequestLeaveEntry.Name = "labelRequestLeaveEntry";
            this.labelRequestLeaveEntry.Size = new System.Drawing.Size(144, 18);
            this.labelRequestLeaveEntry.TabIndex = 0;
            this.labelRequestLeaveEntry.Text = "Request Leave Entry";
            // 
            // labelLeaveRequest
            // 
            this.labelLeaveRequest.AutoSize = true;
            this.labelLeaveRequest.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLeaveRequest.ForeColor = System.Drawing.Color.Red;
            this.labelLeaveRequest.Location = new System.Drawing.Point(29, 19);
            this.labelLeaveRequest.Name = "labelLeaveRequest";
            this.labelLeaveRequest.Size = new System.Drawing.Size(174, 29);
            this.labelLeaveRequest.TabIndex = 0;
            this.labelLeaveRequest.Text = "Leave Request";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.comboBoxName);
            this.panel2.Controls.Add(this.labelDash);
            this.panel2.Controls.Add(this.buttonCancel);
            this.panel2.Controls.Add(this.buttonSendRequest);
            this.panel2.Controls.Add(this.textBoxReasonInput);
            this.panel2.Controls.Add(this.textBoxEndPeriod);
            this.panel2.Controls.Add(this.textBoxStartPeriod);
            this.panel2.Controls.Add(this.comboBoxLeaveTypeInput);
            this.panel2.Controls.Add(this.labelReason);
            this.panel2.Controls.Add(this.labelPeriod);
            this.panel2.Controls.Add(this.labelLeaveType);
            this.panel2.Controls.Add(this.labelName);
            this.panel2.Location = new System.Drawing.Point(0, 89);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(439, 262);
            this.panel2.TabIndex = 2;
            // 
            // labelDash
            // 
            this.labelDash.AutoSize = true;
            this.labelDash.Location = new System.Drawing.Point(262, 75);
            this.labelDash.Name = "labelDash";
            this.labelDash.Size = new System.Drawing.Size(13, 13);
            this.labelDash.TabIndex = 2;
            this.labelDash.Text = "_";
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(154)))), ((int)(((byte)(154)))));
            this.buttonCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(339, 214);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(88, 30);
            this.buttonCancel.TabIndex = 12;
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
            this.buttonSendRequest.Location = new System.Drawing.Point(195, 214);
            this.buttonSendRequest.Name = "buttonSendRequest";
            this.buttonSendRequest.Size = new System.Drawing.Size(135, 30);
            this.buttonSendRequest.TabIndex = 11;
            this.buttonSendRequest.Text = "Send Request";
            this.buttonSendRequest.UseVisualStyleBackColor = false;
            this.buttonSendRequest.Click += new System.EventHandler(this.buttonSendRequest_Click);
            // 
            // textBoxReasonInput
            // 
            this.textBoxReasonInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxReasonInput.Location = new System.Drawing.Point(32, 128);
            this.textBoxReasonInput.Multiline = true;
            this.textBoxReasonInput.Name = "textBoxReasonInput";
            this.textBoxReasonInput.Size = new System.Drawing.Size(365, 63);
            this.textBoxReasonInput.TabIndex = 8;
            // 
            // textBoxEndPeriod
            // 
            this.textBoxEndPeriod.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxEndPeriod.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxEndPeriod.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxEndPeriod.ForeColor = System.Drawing.Color.White;
            this.textBoxEndPeriod.Location = new System.Drawing.Point(279, 77);
            this.textBoxEndPeriod.Name = "textBoxEndPeriod";
            this.textBoxEndPeriod.Size = new System.Drawing.Size(118, 19);
            this.textBoxEndPeriod.TabIndex = 7;
            this.textBoxEndPeriod.Text = "mm/dd/yyyy";
            this.textBoxEndPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxStartPeriod
            // 
            this.textBoxStartPeriod.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxStartPeriod.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxStartPeriod.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxStartPeriod.ForeColor = System.Drawing.Color.White;
            this.textBoxStartPeriod.Location = new System.Drawing.Point(137, 77);
            this.textBoxStartPeriod.Name = "textBoxStartPeriod";
            this.textBoxStartPeriod.Size = new System.Drawing.Size(120, 19);
            this.textBoxStartPeriod.TabIndex = 6;
            this.textBoxStartPeriod.Text = "mm/dd/yyyy";
            this.textBoxStartPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // comboBoxLeaveTypeInput
            // 
            this.comboBoxLeaveTypeInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.comboBoxLeaveTypeInput.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxLeaveTypeInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLeaveTypeInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxLeaveTypeInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxLeaveTypeInput.ForeColor = System.Drawing.Color.White;
            this.comboBoxLeaveTypeInput.FormattingEnabled = true;
            this.comboBoxLeaveTypeInput.Items.AddRange(new object[] {
            "Vacation",
            "Sick"});
            this.comboBoxLeaveTypeInput.Location = new System.Drawing.Point(137, 42);
            this.comboBoxLeaveTypeInput.Name = "comboBoxLeaveTypeInput";
            this.comboBoxLeaveTypeInput.Size = new System.Drawing.Size(260, 24);
            this.comboBoxLeaveTypeInput.TabIndex = 5;
            // 
            // labelReason
            // 
            this.labelReason.AutoSize = true;
            this.labelReason.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelReason.Location = new System.Drawing.Point(28, 105);
            this.labelReason.Name = "labelReason";
            this.labelReason.Size = new System.Drawing.Size(71, 20);
            this.labelReason.TabIndex = 3;
            this.labelReason.Text = "Reason";
            // 
            // labelPeriod
            // 
            this.labelPeriod.AutoSize = true;
            this.labelPeriod.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPeriod.Location = new System.Drawing.Point(28, 76);
            this.labelPeriod.Name = "labelPeriod";
            this.labelPeriod.Size = new System.Drawing.Size(60, 20);
            this.labelPeriod.TabIndex = 2;
            this.labelPeriod.Text = "Period";
            // 
            // labelLeaveType
            // 
            this.labelLeaveType.AutoSize = true;
            this.labelLeaveType.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLeaveType.Location = new System.Drawing.Point(28, 46);
            this.labelLeaveType.Name = "labelLeaveType";
            this.labelLeaveType.Size = new System.Drawing.Size(100, 20);
            this.labelLeaveType.TabIndex = 1;
            this.labelLeaveType.Text = "Leave Type";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelName.Location = new System.Drawing.Point(28, 15);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(55, 20);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name";
            // 
            // comboBoxName
            // 
            this.comboBoxName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.comboBoxName.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxName.ForeColor = System.Drawing.Color.White;
            this.comboBoxName.FormattingEnabled = true;
            this.comboBoxName.Items.AddRange(new object[] {
            "Vacation",
            "Sick"});
            this.comboBoxName.Location = new System.Drawing.Point(137, 15);
            this.comboBoxName.Name = "comboBoxName";
            this.comboBoxName.Size = new System.Drawing.Size(260, 24);
            this.comboBoxName.TabIndex = 13;
            // 
            // LeaveRequest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(437, 350);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LeaveRequest";
            this.ShowInTaskbar = false;
            this.Text = "LeaveRequest";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelRequestLeaveEntry;
        private System.Windows.Forms.Label labelLeaveRequest;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelReason;
        private System.Windows.Forms.Label labelPeriod;
        private System.Windows.Forms.Label labelLeaveType;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.ComboBox comboBoxLeaveTypeInput;
        private System.Windows.Forms.TextBox textBoxEndPeriod;
        private System.Windows.Forms.TextBox textBoxStartPeriod;
        private System.Windows.Forms.TextBox textBoxReasonInput;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSendRequest;
        private System.Windows.Forms.Label labelDash;
        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.ComboBox comboBoxName;
    }
}