namespace HRIS_JAP_ATTPAY
{
    partial class AdminMenu
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
            this.AdminOptionsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOverview = new System.Windows.Forms.Button();
            this.buttonEmployee = new System.Windows.Forms.Button();
            this.buttonAttendance = new System.Windows.Forms.Button();
            this.buttonPayroll = new System.Windows.Forms.Button();
            this.labelLogOut = new System.Windows.Forms.Label();
            this.AdminMiscTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBoxNotification = new System.Windows.Forms.PictureBox();
            this.pictureBoxUserProfile = new System.Windows.Forms.PictureBox();
            this.pictureBoxMenuLogo = new System.Windows.Forms.PictureBox();
            this.AdminOptionsTableLayoutPanel.SuspendLayout();
            this.AdminMiscTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNotification)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxUserProfile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMenuLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // AdminOptionsTableLayoutPanel
            // 
            this.AdminOptionsTableLayoutPanel.ColumnCount = 4;
            this.AdminOptionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.AdminOptionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.AdminOptionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.AdminOptionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.AdminOptionsTableLayoutPanel.Controls.Add(this.buttonOverview, 0, 0);
            this.AdminOptionsTableLayoutPanel.Controls.Add(this.buttonEmployee, 1, 0);
            this.AdminOptionsTableLayoutPanel.Controls.Add(this.buttonAttendance, 2, 0);
            this.AdminOptionsTableLayoutPanel.Controls.Add(this.buttonPayroll, 3, 0);
            this.AdminOptionsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.AdminOptionsTableLayoutPanel.Location = new System.Drawing.Point(0, 119);
            this.AdminOptionsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.AdminOptionsTableLayoutPanel.Name = "AdminOptionsTableLayoutPanel";
            this.AdminOptionsTableLayoutPanel.RowCount = 1;
            this.AdminOptionsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.AdminOptionsTableLayoutPanel.Size = new System.Drawing.Size(1440, 63);
            this.AdminOptionsTableLayoutPanel.TabIndex = 0;
            // 
            // buttonOverview
            // 
            this.buttonOverview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(112)))), ((int)(((byte)(175)))));
            this.buttonOverview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonOverview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOverview.FlatAppearance.BorderSize = 0;
            this.buttonOverview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonOverview.ForeColor = System.Drawing.Color.White;
            this.buttonOverview.Location = new System.Drawing.Point(0, 0);
            this.buttonOverview.Margin = new System.Windows.Forms.Padding(0);
            this.buttonOverview.Name = "buttonOverview";
            this.buttonOverview.Size = new System.Drawing.Size(360, 63);
            this.buttonOverview.TabIndex = 0;
            this.buttonOverview.Text = "DASHBOARD";
            this.buttonOverview.UseVisualStyleBackColor = false;
            this.buttonOverview.Click += new System.EventHandler(this.buttonOverview_Click);
            // 
            // buttonEmployee
            // 
            this.buttonEmployee.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(137)))), ((int)(((byte)(207)))));
            this.buttonEmployee.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonEmployee.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonEmployee.FlatAppearance.BorderSize = 0;
            this.buttonEmployee.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonEmployee.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.buttonEmployee.Location = new System.Drawing.Point(360, 0);
            this.buttonEmployee.Margin = new System.Windows.Forms.Padding(0);
            this.buttonEmployee.Name = "buttonEmployee";
            this.buttonEmployee.Size = new System.Drawing.Size(360, 63);
            this.buttonEmployee.TabIndex = 1;
            this.buttonEmployee.Text = "EMPLOYEE";
            this.buttonEmployee.UseVisualStyleBackColor = false;
            this.buttonEmployee.Click += new System.EventHandler(this.buttonEmployee_Click);
            // 
            // buttonAttendance
            // 
            this.buttonAttendance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(137)))), ((int)(((byte)(207)))));
            this.buttonAttendance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonAttendance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonAttendance.FlatAppearance.BorderSize = 0;
            this.buttonAttendance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAttendance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.buttonAttendance.Location = new System.Drawing.Point(720, 0);
            this.buttonAttendance.Margin = new System.Windows.Forms.Padding(0);
            this.buttonAttendance.Name = "buttonAttendance";
            this.buttonAttendance.Size = new System.Drawing.Size(360, 63);
            this.buttonAttendance.TabIndex = 2;
            this.buttonAttendance.Text = "ATTENDANCE";
            this.buttonAttendance.UseVisualStyleBackColor = false;
            this.buttonAttendance.Click += new System.EventHandler(this.buttonAttendance_Click);
            // 
            // buttonPayroll
            // 
            this.buttonPayroll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(137)))), ((int)(((byte)(207)))));
            this.buttonPayroll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonPayroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPayroll.FlatAppearance.BorderSize = 0;
            this.buttonPayroll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPayroll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.buttonPayroll.Location = new System.Drawing.Point(1080, 0);
            this.buttonPayroll.Margin = new System.Windows.Forms.Padding(0);
            this.buttonPayroll.Name = "buttonPayroll";
            this.buttonPayroll.Size = new System.Drawing.Size(360, 63);
            this.buttonPayroll.TabIndex = 3;
            this.buttonPayroll.Text = "PAYROLL";
            this.buttonPayroll.UseVisualStyleBackColor = false;
            this.buttonPayroll.Click += new System.EventHandler(this.buttonPayroll_Click);
            // 
            // labelLogOut
            // 
            this.labelLogOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLogOut.AutoSize = true;
            this.labelLogOut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelLogOut.ForeColor = System.Drawing.Color.White;
            this.labelLogOut.Location = new System.Drawing.Point(1325, 37);
            this.labelLogOut.Name = "labelLogOut";
            this.labelLogOut.Size = new System.Drawing.Size(112, 16);
            this.labelLogOut.TabIndex = 1;
            this.labelLogOut.Text = "Log Out";
            this.labelLogOut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelLogOut.Click += new System.EventHandler(this.labelLogOut_Click);
            // 
            // AdminMiscTableLayoutPanel
            // 
            this.AdminMiscTableLayoutPanel.ColumnCount = 9;
            this.AdminMiscTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46F));
            this.AdminMiscTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.5F));
            this.AdminMiscTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3F));
            this.AdminMiscTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.5F));
            this.AdminMiscTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3F));
            this.AdminMiscTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.5F));
            this.AdminMiscTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3F));
            this.AdminMiscTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.5F));
            this.AdminMiscTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.AdminMiscTableLayoutPanel.Controls.Add(this.labelLogOut, 8, 0);
            this.AdminMiscTableLayoutPanel.Controls.Add(this.pictureBoxNotification, 4, 0);
            this.AdminMiscTableLayoutPanel.Controls.Add(this.pictureBoxUserProfile, 6, 0);
            this.AdminMiscTableLayoutPanel.Controls.Add(this.pictureBoxMenuLogo, 0, 0);
            this.AdminMiscTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.AdminMiscTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.AdminMiscTableLayoutPanel.Name = "AdminMiscTableLayoutPanel";
            this.AdminMiscTableLayoutPanel.RowCount = 1;
            this.AdminMiscTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.AdminMiscTableLayoutPanel.Size = new System.Drawing.Size(1440, 90);
            this.AdminMiscTableLayoutPanel.TabIndex = 2;
            // 
            // pictureBoxNotification
            // 
            this.pictureBoxNotification.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxNotification.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pictureBoxNotification.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.Notification;
            this.pictureBoxNotification.Location = new System.Drawing.Point(1197, 24);
            this.pictureBoxNotification.Name = "pictureBoxNotification";
            this.pictureBoxNotification.Size = new System.Drawing.Size(37, 63);
            this.pictureBoxNotification.TabIndex = 2;
            this.pictureBoxNotification.TabStop = false;
            this.pictureBoxNotification.Click += new System.EventHandler(this.pictureBoxNotification_Click);
            // 
            // pictureBoxUserProfile
            // 
            this.pictureBoxUserProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxUserProfile.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pictureBoxUserProfile.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.UserProfile;
            this.pictureBoxUserProfile.Location = new System.Drawing.Point(1261, 24);
            this.pictureBoxUserProfile.Name = "pictureBoxUserProfile";
            this.pictureBoxUserProfile.Size = new System.Drawing.Size(37, 63);
            this.pictureBoxUserProfile.TabIndex = 3;
            this.pictureBoxUserProfile.TabStop = false;
            this.pictureBoxUserProfile.Click += new System.EventHandler(this.pictureBoxUserProfile_Click);
            // 
            // pictureBoxMenuLogo
            // 
            this.pictureBoxMenuLogo.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBoxMenuLogo.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.AltMenuLogo;
            this.pictureBoxMenuLogo.Location = new System.Drawing.Point(3, 3);
            this.pictureBoxMenuLogo.Name = "pictureBoxMenuLogo";
            this.pictureBoxMenuLogo.Size = new System.Drawing.Size(540, 84);
            this.pictureBoxMenuLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxMenuLogo.TabIndex = 4;
            this.pictureBoxMenuLogo.TabStop = false;
            // 
            // AdminMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(137)))), ((int)(((byte)(207)))));
            this.Controls.Add(this.AdminMiscTableLayoutPanel);
            this.Controls.Add(this.AdminOptionsTableLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "AdminMenu";
            this.Size = new System.Drawing.Size(1440, 182);
            this.Load += new System.EventHandler(this.AdminMenu_Load);
            this.AdminOptionsTableLayoutPanel.ResumeLayout(false);
            this.AdminMiscTableLayoutPanel.ResumeLayout(false);
            this.AdminMiscTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNotification)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxUserProfile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMenuLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel AdminOptionsTableLayoutPanel;
        private System.Windows.Forms.Button buttonAttendance;
        private System.Windows.Forms.Button buttonPayroll;
        private System.Windows.Forms.Label labelLogOut;
        private System.Windows.Forms.Button buttonOverview;
        private System.Windows.Forms.Button buttonEmployee;
        private System.Windows.Forms.TableLayoutPanel AdminMiscTableLayoutPanel;
        private System.Windows.Forms.PictureBox pictureBoxNotification;
        private System.Windows.Forms.PictureBox pictureBoxUserProfile;
        private System.Windows.Forms.PictureBox pictureBoxMenuLogo;
    }
}
