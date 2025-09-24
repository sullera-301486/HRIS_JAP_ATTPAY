namespace HRIS_JAP_ATTPAY
{
    partial class TermsAndConditions
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
            this.LoginBackground = new System.Windows.Forms.PictureBox();
            this.OuterLoginRectanglePanel = new System.Windows.Forms.Panel();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.buttonDecline = new System.Windows.Forms.Button();
            this.LoginRectanglePanel = new System.Windows.Forms.Panel();
            this.LoginLogo = new System.Windows.Forms.PictureBox();
            this.name3 = new System.Windows.Forms.Label();
            this.name2 = new System.Windows.Forms.Label();
            this.name1 = new System.Windows.Forms.Label();
            this.labelTermsAndConditions = new System.Windows.Forms.Label();
            this.labelLastUpdate = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.LoginBackground)).BeginInit();
            this.OuterLoginRectanglePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LoginLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // LoginBackground
            // 
            this.LoginBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(137)))), ((int)(((byte)(207)))));
            this.LoginBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LoginBackground.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.LoginBackground;
            this.LoginBackground.Location = new System.Drawing.Point(0, 0);
            this.LoginBackground.Margin = new System.Windows.Forms.Padding(4);
            this.LoginBackground.Name = "LoginBackground";
            this.LoginBackground.Size = new System.Drawing.Size(1900, 1100);
            this.LoginBackground.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LoginBackground.TabIndex = 1;
            this.LoginBackground.TabStop = false;
            // 
            // OuterLoginRectanglePanel
            // 
            this.OuterLoginRectanglePanel.BackColor = System.Drawing.Color.White;
            this.OuterLoginRectanglePanel.Controls.Add(this.labelLastUpdate);
            this.OuterLoginRectanglePanel.Controls.Add(this.labelTermsAndConditions);
            this.OuterLoginRectanglePanel.Controls.Add(this.name3);
            this.OuterLoginRectanglePanel.Controls.Add(this.LoginLogo);
            this.OuterLoginRectanglePanel.Controls.Add(this.name2);
            this.OuterLoginRectanglePanel.Controls.Add(this.name1);
            this.OuterLoginRectanglePanel.Controls.Add(this.buttonDecline);
            this.OuterLoginRectanglePanel.Controls.Add(this.buttonAccept);
            this.OuterLoginRectanglePanel.Location = new System.Drawing.Point(718, 120);
            this.OuterLoginRectanglePanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.OuterLoginRectanglePanel.Name = "OuterLoginRectanglePanel";
            this.OuterLoginRectanglePanel.Size = new System.Drawing.Size(665, 665);
            this.OuterLoginRectanglePanel.TabIndex = 2;
            // 
            // buttonAccept
            // 
            this.buttonAccept.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(81)))), ((int)(((byte)(148)))));
            this.buttonAccept.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonAccept.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAccept.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAccept.ForeColor = System.Drawing.Color.White;
            this.buttonAccept.Location = new System.Drawing.Point(406, 604);
            this.buttonAccept.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(180, 37);
            this.buttonAccept.TabIndex = 13;
            this.buttonAccept.Text = "Accept";
            this.buttonAccept.UseVisualStyleBackColor = false;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // buttonDecline
            // 
            this.buttonDecline.BackColor = System.Drawing.Color.White;
            this.buttonDecline.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonDecline.FlatAppearance.BorderSize = 2;
            this.buttonDecline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDecline.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDecline.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(81)))), ((int)(((byte)(148)))));
            this.buttonDecline.Location = new System.Drawing.Point(101, 604);
            this.buttonDecline.Margin = new System.Windows.Forms.Padding(4);
            this.buttonDecline.Name = "buttonDecline";
            this.buttonDecline.Size = new System.Drawing.Size(180, 37);
            this.buttonDecline.TabIndex = 14;
            this.buttonDecline.Text = "Decline";
            this.buttonDecline.UseVisualStyleBackColor = false;
            this.buttonDecline.Click += new System.EventHandler(this.buttonDecline_Click);
            // 
            // LoginRectanglePanel
            // 
            this.LoginRectanglePanel.Location = new System.Drawing.Point(748, 387);
            this.LoginRectanglePanel.Name = "LoginRectanglePanel";
            this.LoginRectanglePanel.Size = new System.Drawing.Size(596, 323);
            this.LoginRectanglePanel.TabIndex = 0;
            // 
            // LoginLogo
            // 
            this.LoginLogo.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.LoginLogo;
            this.LoginLogo.Location = new System.Drawing.Point(159, 5);
            this.LoginLogo.Name = "LoginLogo";
            this.LoginLogo.Size = new System.Drawing.Size(350, 139);
            this.LoginLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LoginLogo.TabIndex = 3;
            this.LoginLogo.TabStop = false;
            // 
            // name3
            // 
            this.name3.AutoSize = true;
            this.name3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.name3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.name3.Location = new System.Drawing.Point(232, 147);
            this.name3.Name = "name3";
            this.name3.Size = new System.Drawing.Size(195, 25);
            this.name3.TabIndex = 7;
            this.name3.Text = "Perfecto Builders Inc.";
            // 
            // name2
            // 
            this.name2.AutoSize = true;
            this.name2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.name2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(36)))), ((int)(((byte)(30)))));
            this.name2.Location = new System.Drawing.Point(202, 147);
            this.name2.Name = "name2";
            this.name2.Size = new System.Drawing.Size(31, 25);
            this.name2.TabIndex = 6;
            this.name2.Text = "A.";
            // 
            // name1
            // 
            this.name1.AutoSize = true;
            this.name1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.name1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(23)))), ((int)(((byte)(112)))));
            this.name1.Location = new System.Drawing.Point(177, 147);
            this.name1.Name = "name1";
            this.name1.Size = new System.Drawing.Size(28, 25);
            this.name1.TabIndex = 5;
            this.name1.Text = "J.";
            // 
            // labelTermsAndConditions
            // 
            this.labelTermsAndConditions.AutoSize = true;
            this.labelTermsAndConditions.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            this.labelTermsAndConditions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(36)))), ((int)(((byte)(30)))));
            this.labelTermsAndConditions.Location = new System.Drawing.Point(166, 190);
            this.labelTermsAndConditions.Name = "labelTermsAndConditions";
            this.labelTermsAndConditions.Size = new System.Drawing.Size(316, 39);
            this.labelTermsAndConditions.TabIndex = 15;
            this.labelTermsAndConditions.Text = "Terms && Conditions";
            // 
            // labelLastUpdate
            // 
            this.labelLastUpdate.AutoSize = true;
            this.labelLastUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.labelLastUpdate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
            this.labelLastUpdate.Location = new System.Drawing.Point(185, 239);
            this.labelLastUpdate.Name = "labelLastUpdate";
            this.labelLastUpdate.Size = new System.Drawing.Size(260, 20);
            this.labelLastUpdate.TabIndex = 8;
            this.labelLastUpdate.Text = "Last updated September 21, 2025";
            // 
            // TermsAndConditions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1900, 1100);
            this.Controls.Add(this.LoginRectanglePanel);
            this.Controls.Add(this.OuterLoginRectanglePanel);
            this.Controls.Add(this.LoginBackground);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TermsAndConditions";
            this.Text = "TermsAndConditions";
            this.Load += new System.EventHandler(this.TermsAndConditions_Load);
            ((System.ComponentModel.ISupportInitialize)(this.LoginBackground)).EndInit();
            this.OuterLoginRectanglePanel.ResumeLayout(false);
            this.OuterLoginRectanglePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LoginLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox LoginBackground;
        private System.Windows.Forms.Panel OuterLoginRectanglePanel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Button buttonDecline;
        private System.Windows.Forms.Panel LoginRectanglePanel;
        private System.Windows.Forms.PictureBox LoginLogo;
        private System.Windows.Forms.Label name3;
        private System.Windows.Forms.Label name2;
        private System.Windows.Forms.Label name1;
        private System.Windows.Forms.Label labelTermsAndConditions;
        private System.Windows.Forms.Label labelLastUpdate;
    }
}