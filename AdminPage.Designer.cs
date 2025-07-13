namespace HRIS_JAP_ATTPAY
{
    partial class AdminForm
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
            this.AdminMenuPanel = new System.Windows.Forms.Panel();
            this.AdminViewPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // AdminMenuPanel
            // 
            this.AdminMenuPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(137)))), ((int)(((byte)(207)))));
            this.AdminMenuPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.AdminMenuPanel.Location = new System.Drawing.Point(0, 0);
            this.AdminMenuPanel.Name = "AdminMenuPanel";
            this.AdminMenuPanel.Size = new System.Drawing.Size(1422, 182);
            this.AdminMenuPanel.TabIndex = 1;
            // 
            // AdminViewPanel
            // 
            this.AdminViewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AdminViewPanel.AutoScroll = true;
            this.AdminViewPanel.BackColor = System.Drawing.Color.White;
            this.AdminViewPanel.Location = new System.Drawing.Point(0, 182);
            this.AdminViewPanel.Margin = new System.Windows.Forms.Padding(0);
            this.AdminViewPanel.Name = "AdminViewPanel";
            this.AdminViewPanel.Size = new System.Drawing.Size(1422, 793);
            this.AdminViewPanel.TabIndex = 2;
            // 
            // AdminForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1422, 977);
            this.Controls.Add(this.AdminViewPanel);
            this.Controls.Add(this.AdminMenuPanel);
            this.Name = "AdminForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AdminPage";
            this.Load += new System.EventHandler(this.AdminPage_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel AdminMenuPanel;
        private System.Windows.Forms.Panel AdminViewPanel;
    }
}