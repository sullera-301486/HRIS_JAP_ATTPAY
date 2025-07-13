namespace HRIS_JAP_ATTPAY
{
    partial class HRNotification
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
            this.labelNotification = new System.Windows.Forms.Label();
            this.XpictureBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // labelNotification
            // 
            this.labelNotification.AutoSize = true;
            this.labelNotification.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(34)))), ((int)(((byte)(31)))));
            this.labelNotification.Location = new System.Drawing.Point(58, 62);
            this.labelNotification.Name = "labelNotification";
            this.labelNotification.Size = new System.Drawing.Size(80, 16);
            this.labelNotification.TabIndex = 0;
            this.labelNotification.Text = "Notifications";
            // 
            // XpictureBox
            // 
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(654, 28);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(50, 50);
            this.XpictureBox.TabIndex = 1;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(290, 238);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "temporary HR Tag";
            // 
            // HRNotification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 512);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.XpictureBox);
            this.Controls.Add(this.labelNotification);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "HRNotification";
            this.ShowInTaskbar = false;
            this.Text = "HRNotification";
            this.Load += new System.EventHandler(this.HRNotification_Load);
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelNotification;
        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Label label1;
    }
}