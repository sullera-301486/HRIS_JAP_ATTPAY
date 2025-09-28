namespace HRIS_JAP_ATTPAY
{
    partial class ScanRFID
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
            this.labelScanRFID = new System.Windows.Forms.Label();
            this.labelRFIDResult = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(351, 2);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(49, 50);
            this.XpictureBox.TabIndex = 5;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelScanRFID
            // 
            this.labelScanRFID.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelScanRFID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(34)))), ((int)(((byte)(31)))));
            this.labelScanRFID.Location = new System.Drawing.Point(2, 74);
            this.labelScanRFID.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelScanRFID.Name = "labelScanRFID";
            this.labelScanRFID.Size = new System.Drawing.Size(398, 56);
            this.labelScanRFID.TabIndex = 35;
            this.labelScanRFID.Text = "Scan RFID";
            this.labelScanRFID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelRFIDResult
            // 
            this.labelRFIDResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRFIDResult.Location = new System.Drawing.Point(3, 149);
            this.labelRFIDResult.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelRFIDResult.Name = "labelRFIDResult";
            this.labelRFIDResult.Size = new System.Drawing.Size(397, 41);
            this.labelRFIDResult.TabIndex = 36;
            this.labelRFIDResult.Text = "Waiting for RFID";
            this.labelRFIDResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ScanRFID
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(402, 293);
            this.Controls.Add(this.labelRFIDResult);
            this.Controls.Add(this.labelScanRFID);
            this.Controls.Add(this.XpictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ScanRFID";
            this.ShowInTaskbar = false;
            this.Text = "ScanRFID";
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Label labelScanRFID;
        private System.Windows.Forms.Label labelRFIDResult;
    }
}