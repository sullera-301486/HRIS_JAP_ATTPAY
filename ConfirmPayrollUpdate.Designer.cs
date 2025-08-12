namespace HRIS_JAP_ATTPAY
{
    partial class ConfirmPayrollUpdate
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonConfirm = new System.Windows.Forms.Button();
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelRequestConfirm = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // XpictureBox
            // 
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(274, 2);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(38, 41);
            this.XpictureBox.TabIndex = 33;
            this.XpictureBox.TabStop = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(154)))), ((int)(((byte)(154)))));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(166, 160);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(101, 30);
            this.buttonCancel.TabIndex = 32;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            // 
            // buttonConfirm
            // 
            this.buttonConfirm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(81)))), ((int)(((byte)(148)))));
            this.buttonConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonConfirm.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonConfirm.ForeColor = System.Drawing.Color.White;
            this.buttonConfirm.Location = new System.Drawing.Point(47, 160);
            this.buttonConfirm.Name = "buttonConfirm";
            this.buttonConfirm.Size = new System.Drawing.Size(101, 30);
            this.buttonConfirm.TabIndex = 31;
            this.buttonConfirm.Text = "Confirm";
            this.buttonConfirm.UseVisualStyleBackColor = false;
            this.buttonConfirm.Click += new System.EventHandler(this.buttonConfirm_Click);
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMessage.Location = new System.Drawing.Point(40, 84);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(238, 54);
            this.labelMessage.TabIndex = 30;
            this.labelMessage.Text = "Are you sure you want to save the \r\nchanges to this employee’s payroll \r\ninformat" +
    "ion?";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelRequestConfirm
            // 
            this.labelRequestConfirm.AutoSize = true;
            this.labelRequestConfirm.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRequestConfirm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(34)))), ((int)(((byte)(31)))));
            this.labelRequestConfirm.Location = new System.Drawing.Point(42, 43);
            this.labelRequestConfirm.Name = "labelRequestConfirm";
            this.labelRequestConfirm.Size = new System.Drawing.Size(233, 25);
            this.labelRequestConfirm.TabIndex = 29;
            this.labelRequestConfirm.Text = "Confirm Payroll Update";
            // 
            // ConfirmPayrollUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(314, 215);
            this.Controls.Add(this.XpictureBox);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonConfirm);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.labelRequestConfirm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ConfirmPayrollUpdate";
            this.Text = "ConfirmPayrollUpdate";
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