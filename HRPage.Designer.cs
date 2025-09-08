using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    partial class HRForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HRForm));
            this.HRMenuPanel = new System.Windows.Forms.Panel();
            this.HRViewPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // HRMenuPanel
            // 
            this.HRMenuPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(137)))), ((int)(((byte)(207)))));
            this.HRMenuPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HRMenuPanel.Location = new System.Drawing.Point(0, 0);
            this.HRMenuPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.HRMenuPanel.Name = "HRMenuPanel";
            this.HRMenuPanel.Size = new System.Drawing.Size(1371, 182);
            this.HRMenuPanel.TabIndex = 1;
            // 
            // HRViewPanel
            // 
            this.HRViewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HRViewPanel.AutoScroll = true;
            this.HRViewPanel.Location = new System.Drawing.Point(0, 182);
            this.HRViewPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.HRViewPanel.Name = "HRViewPanel";
            this.HRViewPanel.Size = new System.Drawing.Size(1371, 793);
            this.HRViewPanel.TabIndex = 2;
            // 
            // HRForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1371, 750);
            this.Controls.Add(this.HRViewPanel);
            this.Controls.Add(this.HRMenuPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "HRForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JAP HRIS HR";
            this.Load += new System.EventHandler(this.HRPage_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel HRMenuPanel;
        private System.Windows.Forms.Panel HRViewPanel;
    }
}