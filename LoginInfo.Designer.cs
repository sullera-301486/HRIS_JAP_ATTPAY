namespace HRIS_JAP_ATTPAY
{
    partial class LoginInfo
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
            this.LoginLogo = new System.Windows.Forms.PictureBox();
            this.name1 = new System.Windows.Forms.Label();
            this.name2 = new System.Windows.Forms.Label();
            this.name3 = new System.Windows.Forms.Label();
            this.info1 = new System.Windows.Forms.Label();
            this.info2 = new System.Windows.Forms.Label();
            this.info3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.LoginLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // LoginLogo
            // 
            this.LoginLogo.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.LoginLogo;
            this.LoginLogo.Location = new System.Drawing.Point(10, 30);
            this.LoginLogo.Name = "LoginLogo";
            this.LoginLogo.Size = new System.Drawing.Size(450, 160);
            this.LoginLogo.TabIndex = 1;
            this.LoginLogo.TabStop = false;
            // 
            // name1
            // 
            this.name1.AutoSize = true;
            this.name1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.name1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.name1.Location = new System.Drawing.Point(10, 210);
            this.name1.Name = "name1";
            this.name1.Size = new System.Drawing.Size(28, 25);
            this.name1.TabIndex = 2;
            this.name1.Text = "J.";
            // 
            // name2
            // 
            this.name2.AutoSize = true;
            this.name2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.name2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(36)))), ((int)(((byte)(30)))));
            this.name2.Location = new System.Drawing.Point(48, 210);
            this.name2.Name = "name2";
            this.name2.Size = new System.Drawing.Size(31, 25);
            this.name2.TabIndex = 3;
            this.name2.Text = "A.";
            // 
            // name3
            // 
            this.name3.AutoSize = true;
            this.name3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.name3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.name3.Location = new System.Drawing.Point(91, 210);
            this.name3.Name = "name3";
            this.name3.Size = new System.Drawing.Size(195, 25);
            this.name3.TabIndex = 4;
            this.name3.Text = "Perfecto Builders Inc.";
            // 
            // info1
            // 
            this.info1.AutoSize = true;
            this.info1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.info1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.info1.Location = new System.Drawing.Point(10, 270);
            this.info1.Name = "info1";
            this.info1.Size = new System.Drawing.Size(349, 25);
            this.info1.TabIndex = 5;
            this.info1.Text = "An HRIS that manages employee data,";
            // 
            // info2
            // 
            this.info2.AutoSize = true;
            this.info2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.info2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.info2.Location = new System.Drawing.Point(10, 300);
            this.info2.Name = "info2";
            this.info2.Size = new System.Drawing.Size(311, 25);
            this.info2.TabIndex = 6;
            this.info2.Text = "attendance, payroll, and other core";
            // 
            // info3
            // 
            this.info3.AutoSize = true;
            this.info3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.info3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.info3.Location = new System.Drawing.Point(10, 330);
            this.info3.Name = "info3";
            this.info3.Size = new System.Drawing.Size(257, 25);
            this.info3.TabIndex = 7;
            this.info3.Text = "HR functions in one system. \r\n";
            // 
            // LoginInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.info3);
            this.Controls.Add(this.info2);
            this.Controls.Add(this.info1);
            this.Controls.Add(this.name3);
            this.Controls.Add(this.name2);
            this.Controls.Add(this.name1);
            this.Controls.Add(this.LoginLogo);
            this.Name = "LoginInfo";
            this.Size = new System.Drawing.Size(485, 385);
            this.Load += new System.EventHandler(this.LoginInfo_Load);
            ((System.ComponentModel.ISupportInitialize)(this.LoginLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox LoginLogo;
        private System.Windows.Forms.Label name1;
        private System.Windows.Forms.Label name2;
        private System.Windows.Forms.Label name3;
        private System.Windows.Forms.Label info1;
        private System.Windows.Forms.Label info2;
        private System.Windows.Forms.Label info3;
    }
}
