namespace HRIS_JAP_ATTPAY
{
    partial class AddEmployeeScanRFID
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
            this.labelRFIDResult = new System.Windows.Forms.Label();
            this.labelRFIDScan = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(367, 3);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(51, 50);
            this.XpictureBox.TabIndex = 29;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelRFIDResult
            // 
            this.labelRFIDResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRFIDResult.Location = new System.Drawing.Point(9, 166);
            this.labelRFIDResult.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelRFIDResult.Name = "labelRFIDResult";
            this.labelRFIDResult.Size = new System.Drawing.Size(394, 31);
            this.labelRFIDResult.TabIndex = 35;
            this.labelRFIDResult.Text = "Waiting for RFID";
            this.labelRFIDResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelRFIDScan
            // 
            this.labelRFIDScan.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRFIDScan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(34)))), ((int)(((byte)(31)))));
            this.labelRFIDScan.Location = new System.Drawing.Point(13, 82);
            this.labelRFIDScan.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelRFIDScan.Name = "labelRFIDScan";
            this.labelRFIDScan.Size = new System.Drawing.Size(394, 53);
            this.labelRFIDScan.TabIndex = 36;
            this.labelRFIDScan.Text = "Scan RFID";
            this.labelRFIDScan.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AddEmployeeScanRFID
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(420, 340);
            this.Controls.Add(this.labelRFIDScan);
            this.Controls.Add(this.labelRFIDResult);
            this.Controls.Add(this.XpictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AddEmployeeScanRFID";
            this.ShowInTaskbar = false;
            this.Text = "AddEmployeeScanRFID";
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Label labelRFIDResult;
        private System.Windows.Forms.Label labelRFIDScan;
    }
}