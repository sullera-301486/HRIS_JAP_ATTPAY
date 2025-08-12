namespace HRIS_JAP_ATTPAY
{
    partial class ConfirmPayrollExportAll
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonConfirm = new System.Windows.Forms.Button();
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelRequestConfirm = new System.Windows.Forms.Label();
            this.XpictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(154)))), ((int)(((byte)(154)))));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(173, 152);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(101, 30);
            this.buttonCancel.TabIndex = 27;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            // 
            // buttonConfirm
            // 
            this.buttonConfirm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(135)))), ((int)(((byte)(57)))));
            this.buttonConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonConfirm.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonConfirm.ForeColor = System.Drawing.Color.White;
            this.buttonConfirm.Location = new System.Drawing.Point(54, 152);
            this.buttonConfirm.Name = "buttonConfirm";
            this.buttonConfirm.Size = new System.Drawing.Size(101, 30);
            this.buttonConfirm.TabIndex = 26;
            this.buttonConfirm.Text = "Confirm";
            this.buttonConfirm.UseVisualStyleBackColor = false;
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMessage.Location = new System.Drawing.Point(35, 73);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(253, 54);
            this.labelMessage.TabIndex = 25;
            this.labelMessage.Text = "Please confirm that you want to\r\n download all payroll data as an Excel\r\n file. D" +
    "o you want to continue?";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelRequestConfirm
            // 
            this.labelRequestConfirm.AutoSize = true;
            this.labelRequestConfirm.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRequestConfirm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(34)))), ((int)(((byte)(31)))));
            this.labelRequestConfirm.Location = new System.Drawing.Point(49, 35);
            this.labelRequestConfirm.Name = "labelRequestConfirm";
            this.labelRequestConfirm.Size = new System.Drawing.Size(226, 25);
            this.labelRequestConfirm.TabIndex = 24;
            this.labelRequestConfirm.Text = "Confirm Payroll Export";
            // 
            // XpictureBox
            // 
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(284, -2);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(38, 41);
            this.XpictureBox.TabIndex = 28;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // ConfirmPayrollExportAll
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(323, 192);
            this.Controls.Add(this.XpictureBox);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonConfirm);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.labelRequestConfirm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ConfirmPayrollExportAll";
            this.ShowInTaskbar = false;
            this.Text = "ConfirmPayrollExportAll";
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonConfirm;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelRequestConfirm;
    }
}