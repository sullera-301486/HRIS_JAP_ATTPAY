namespace HRIS_JAP_ATTPAY
{
    partial class ManageLeave
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
            this.XpictureBox = new System.Windows.Forms.PictureBox();
            this.labelLeaveManagement = new System.Windows.Forms.Label();
            this.labelAddLeave = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(1018, 12);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(50, 50);
            this.XpictureBox.TabIndex = 2;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelLeaveManagement
            // 
            this.labelLeaveManagement.AutoSize = true;
            this.labelLeaveManagement.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLeaveManagement.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(36)))), ((int)(((byte)(30)))));
            this.labelLeaveManagement.Location = new System.Drawing.Point(338, 47);
            this.labelLeaveManagement.Name = "labelLeaveManagement";
            this.labelLeaveManagement.Size = new System.Drawing.Size(369, 46);
            this.labelLeaveManagement.TabIndex = 3;
            this.labelLeaveManagement.Text = "Leave Management";
            this.labelLeaveManagement.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelAddLeave
            // 
            this.labelAddLeave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAddLeave.AutoSize = true;
            this.labelAddLeave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelAddLeave.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAddLeave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.labelAddLeave.Location = new System.Drawing.Point(828, 189);
            this.labelAddLeave.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAddLeave.Name = "labelAddLeave";
            this.labelAddLeave.Size = new System.Drawing.Size(135, 25);
            this.labelAddLeave.TabIndex = 12;
            this.labelAddLeave.Text = "Add Leave +";
            this.labelAddLeave.Click += new System.EventHandler(this.labelAddLeave_Click);
            // 
            // ManageLeave
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1080, 768);
            this.Controls.Add(this.labelAddLeave);
            this.Controls.Add(this.labelLeaveManagement);
            this.Controls.Add(this.XpictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ManageLeave";
            this.ShowInTaskbar = false;
            this.Text = "ManageLeave";
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Label labelLeaveManagement;
        private System.Windows.Forms.Label labelAddLeave;
    }
}