namespace HRIS_JAP_ATTPAY
{
    partial class LogOut
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
            this.labelLogOutConfirm = new System.Windows.Forms.Label();
            this.labelLogOutDetails1 = new System.Windows.Forms.Label();
            this.labelLogOutDetails2 = new System.Windows.Forms.Label();
            this.labelLogOutDetails3 = new System.Windows.Forms.Label();
            this.buttonLogOut = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(423, 12);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(50, 50);
            this.XpictureBox.TabIndex = 1;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelLogOutConfirm
            // 
            this.labelLogOutConfirm.AutoSize = true;
            this.labelLogOutConfirm.Font = new System.Drawing.Font("Microsoft Sans Serif", 21F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLogOutConfirm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(36)))), ((int)(((byte)(30)))));
            this.labelLogOutConfirm.Location = new System.Drawing.Point(73, 65);
            this.labelLogOutConfirm.Name = "labelLogOutConfirm";
            this.labelLogOutConfirm.Size = new System.Drawing.Size(329, 39);
            this.labelLogOutConfirm.TabIndex = 2;
            this.labelLogOutConfirm.Text = "Logout Confirmation";
            this.labelLogOutConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLogOutDetails1
            // 
            this.labelLogOutDetails1.AutoSize = true;
            this.labelLogOutDetails1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLogOutDetails1.ForeColor = System.Drawing.Color.Black;
            this.labelLogOutDetails1.Location = new System.Drawing.Point(101, 120);
            this.labelLogOutDetails1.Name = "labelLogOutDetails1";
            this.labelLogOutDetails1.Size = new System.Drawing.Size(281, 25);
            this.labelLogOutDetails1.TabIndex = 3;
            this.labelLogOutDetails1.Text = "Please confirm that you want to";
            this.labelLogOutDetails1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLogOutDetails2
            // 
            this.labelLogOutDetails2.AutoSize = true;
            this.labelLogOutDetails2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLogOutDetails2.ForeColor = System.Drawing.Color.Black;
            this.labelLogOutDetails2.Location = new System.Drawing.Point(94, 150);
            this.labelLogOutDetails2.Name = "labelLogOutDetails2";
            this.labelLogOutDetails2.Size = new System.Drawing.Size(289, 25);
            this.labelLogOutDetails2.TabIndex = 4;
            this.labelLogOutDetails2.Text = "log out of the system to securely";
            this.labelLogOutDetails2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLogOutDetails3
            // 
            this.labelLogOutDetails3.AutoSize = true;
            this.labelLogOutDetails3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLogOutDetails3.ForeColor = System.Drawing.Color.Black;
            this.labelLogOutDetails3.Location = new System.Drawing.Point(126, 181);
            this.labelLogOutDetails3.Name = "labelLogOutDetails3";
            this.labelLogOutDetails3.Size = new System.Drawing.Size(230, 25);
            this.labelLogOutDetails3.TabIndex = 5;
            this.labelLogOutDetails3.Text = "end your current session.";
            this.labelLogOutDetails3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonLogOut
            // 
            this.buttonLogOut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(81)))), ((int)(((byte)(148)))));
            this.buttonLogOut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonLogOut.FlatAppearance.BorderSize = 0;
            this.buttonLogOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLogOut.ForeColor = System.Drawing.Color.White;
            this.buttonLogOut.Location = new System.Drawing.Point(85, 240);
            this.buttonLogOut.Name = "buttonLogOut";
            this.buttonLogOut.Size = new System.Drawing.Size(142, 41);
            this.buttonLogOut.TabIndex = 6;
            this.buttonLogOut.Text = "Log Out";
            this.buttonLogOut.UseVisualStyleBackColor = false;
            this.buttonLogOut.Click += new System.EventHandler(this.buttonLogOut_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(154)))), ((int)(((byte)(154)))));
            this.buttonCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonCancel.FlatAppearance.BorderSize = 0;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(275, 240);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(142, 41);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // LogOut
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(485, 345);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonLogOut);
            this.Controls.Add(this.labelLogOutDetails3);
            this.Controls.Add(this.labelLogOutDetails2);
            this.Controls.Add(this.labelLogOutDetails1);
            this.Controls.Add(this.labelLogOutConfirm);
            this.Controls.Add(this.XpictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LogOut";
            this.ShowInTaskbar = false;
            this.Text = "LogOut";
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Label labelLogOutConfirm;
        private System.Windows.Forms.Label labelLogOutDetails1;
        private System.Windows.Forms.Label labelLogOutDetails2;
        private System.Windows.Forms.Label labelLogOutDetails3;
        private System.Windows.Forms.Button buttonLogOut;
        private System.Windows.Forms.Button buttonCancel;
    }
}