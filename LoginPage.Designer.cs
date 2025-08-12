namespace HRIS_JAP_ATTPAY
{
    partial class LoginForm
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
            this.LoginRectanglePanel = new System.Windows.Forms.Panel();
            this.LoginInfoPanel = new System.Windows.Forms.Panel();
            this.LoginBackground = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.LoginBackground)).BeginInit();
            this.SuspendLayout();
            // 
            // LoginRectanglePanel
            // 
            this.LoginRectanglePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoginRectanglePanel.Location = new System.Drawing.Point(528, 34);
            this.LoginRectanglePanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.LoginRectanglePanel.Name = "LoginRectanglePanel";
            this.LoginRectanglePanel.Size = new System.Drawing.Size(450, 488);
            this.LoginRectanglePanel.TabIndex = 1;
            // 
            // LoginInfoPanel
            // 
            this.LoginInfoPanel.Location = new System.Drawing.Point(202, 262);
            this.LoginInfoPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.LoginInfoPanel.Name = "LoginInfoPanel";
            this.LoginInfoPanel.Size = new System.Drawing.Size(364, 313);
            this.LoginInfoPanel.TabIndex = 2;
            // 
            // LoginBackground
            // 
            this.LoginBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(137)))), ((int)(((byte)(207)))));
            this.LoginBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LoginBackground.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.LoginBackground;
            this.LoginBackground.Location = new System.Drawing.Point(0, 0);
            this.LoginBackground.Name = "LoginBackground";
            this.LoginBackground.Size = new System.Drawing.Size(1028, 609);
            this.LoginBackground.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LoginBackground.TabIndex = 0;
            this.LoginBackground.TabStop = false;
            this.LoginBackground.Click += new System.EventHandler(this.LoginBackground_Click);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 609);
            this.Controls.Add(this.LoginInfoPanel);
            this.Controls.Add(this.LoginRectanglePanel);
            this.Controls.Add(this.LoginBackground);
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Log In";
            this.Load += new System.EventHandler(this.LoginPage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.LoginBackground)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox LoginBackground;
        private System.Windows.Forms.Panel LoginRectanglePanel;
        private System.Windows.Forms.Panel LoginInfoPanel;
    }
}

