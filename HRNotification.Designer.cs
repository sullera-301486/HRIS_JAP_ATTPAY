namespace HRIS_JAP_ATTPAY
{
    partial class HRNotification
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
            this.labelNotification = new System.Windows.Forms.Label();
            this.XpictureBox = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.flowSummary = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonClearNotif = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelNotification
            // 
            this.labelNotification.AutoSize = true;
            this.labelNotification.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNotification.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(34)))), ((int)(((byte)(31)))));
            this.labelNotification.Location = new System.Drawing.Point(10, 21);
            this.labelNotification.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelNotification.Name = "labelNotification";
            this.labelNotification.Size = new System.Drawing.Size(145, 29);
            this.labelNotification.TabIndex = 0;
            this.labelNotification.Text = "Notifications";
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(497, -1);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(38, 41);
            this.XpictureBox.TabIndex = 1;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.buttonClearNotif);
            this.panel1.Controls.Add(this.labelNotification);
            this.panel1.Controls.Add(this.XpictureBox);
            this.panel1.Location = new System.Drawing.Point(0, -2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(541, 94);
            this.panel1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.flowSummary);
            this.panel2.Location = new System.Drawing.Point(0, 94);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(541, 365);
            this.panel2.TabIndex = 3;
            // 
            // flowSummary
            // 
            this.flowSummary.AutoScroll = true;
            this.flowSummary.Location = new System.Drawing.Point(0, 0);
            this.flowSummary.Name = "flowSummary";
            this.flowSummary.Size = new System.Drawing.Size(541, 365);
            this.flowSummary.TabIndex = 0;
            // 
            // buttonClearNotif
            // 
            this.buttonClearNotif.BackColor = System.Drawing.Color.Red;
            this.buttonClearNotif.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClearNotif.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearNotif.ForeColor = System.Drawing.Color.White;
            this.buttonClearNotif.Location = new System.Drawing.Point(450, 60);
            this.buttonClearNotif.Name = "buttonClearNotif";
            this.buttonClearNotif.Size = new System.Drawing.Size(82, 27);
            this.buttonClearNotif.TabIndex = 2;
            this.buttonClearNotif.Text = "Clear";
            this.buttonClearNotif.UseVisualStyleBackColor = false;
            this.buttonClearNotif.Click += new System.EventHandler(this.buttonClearNotif_Click);
            // 
            // HRNotification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(540, 459);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "HRNotification";
            this.ShowInTaskbar = false;
            this.Text = "HRNotification";
            this.Load += new System.EventHandler(this.HRNotification_Load);
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelNotification;
        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.FlowLayoutPanel flowSummary;
        private System.Windows.Forms.Button buttonClearNotif;
    }
}