namespace HRIS_JAP_ATTPAY
{
    partial class DeductionMatrix
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
            this.labelDeductionMatrix = new System.Windows.Forms.Label();
            this.labelUpdateLeaveData = new System.Windows.Forms.Label();
            this.panelDeductionMatrix = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(1017, 11);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(51, 50);
            this.XpictureBox.TabIndex = 2;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelDeductionMatrix
            // 
            this.labelDeductionMatrix.AutoSize = true;
            this.labelDeductionMatrix.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDeductionMatrix.ForeColor = System.Drawing.Color.Red;
            this.labelDeductionMatrix.Location = new System.Drawing.Point(13, 25);
            this.labelDeductionMatrix.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDeductionMatrix.Name = "labelDeductionMatrix";
            this.labelDeductionMatrix.Size = new System.Drawing.Size(272, 39);
            this.labelDeductionMatrix.TabIndex = 3;
            this.labelDeductionMatrix.Text = "Deduction Matrix";
            // 
            // labelUpdateLeaveData
            // 
            this.labelUpdateLeaveData.AutoSize = true;
            this.labelUpdateLeaveData.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUpdateLeaveData.ForeColor = System.Drawing.Color.Black;
            this.labelUpdateLeaveData.Location = new System.Drawing.Point(16, 80);
            this.labelUpdateLeaveData.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUpdateLeaveData.Name = "labelUpdateLeaveData";
            this.labelUpdateLeaveData.Size = new System.Drawing.Size(330, 29);
            this.labelUpdateLeaveData.TabIndex = 4;
            this.labelUpdateLeaveData.Text = "View Government Deductions";
            // 
            // panelDeductionMatrix
            // 
            this.panelDeductionMatrix.AutoScroll = true;
            this.panelDeductionMatrix.BackColor = System.Drawing.Color.Gray;
            this.panelDeductionMatrix.Location = new System.Drawing.Point(1, 134);
            this.panelDeductionMatrix.Name = "panelDeductionMatrix";
            this.panelDeductionMatrix.Size = new System.Drawing.Size(1079, 753);
            this.panelDeductionMatrix.TabIndex = 5;
            // 
            // DeductionMatrix
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1080, 880);
            this.Controls.Add(this.panelDeductionMatrix);
            this.Controls.Add(this.labelUpdateLeaveData);
            this.Controls.Add(this.labelDeductionMatrix);
            this.Controls.Add(this.XpictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DeductionMatrix";
            this.Text = "DeductionMatrix";
            this.Load += new System.EventHandler(this.DeductionMatrix_Load);
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Label labelDeductionMatrix;
        private System.Windows.Forms.Label labelUpdateLeaveData;
        private System.Windows.Forms.Panel panelDeductionMatrix;
    }
}