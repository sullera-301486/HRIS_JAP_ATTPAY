namespace HRIS_JAP_ATTPAY
{
    partial class AddNewTask
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
            this.labelAddNewTask = new System.Windows.Forms.Label();
            this.labelAddTaskDesc = new System.Windows.Forms.Label();
            this.textBoxTaskDesc = new System.Windows.Forms.TextBox();
            this.textBoxDueDate = new System.Windows.Forms.TextBox();
            this.labelTaskDesc = new System.Windows.Forms.Label();
            this.labelDueDate = new System.Windows.Forms.Label();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // XpictureBox
            // 
            this.XpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.XpictureBox.Image = global::HRIS_JAP_ATTPAY.Properties.Resources.XButton;
            this.XpictureBox.Location = new System.Drawing.Point(453, 8);
            this.XpictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.XpictureBox.Name = "XpictureBox";
            this.XpictureBox.Size = new System.Drawing.Size(51, 50);
            this.XpictureBox.TabIndex = 2;
            this.XpictureBox.TabStop = false;
            this.XpictureBox.Click += new System.EventHandler(this.XpictureBox_Click);
            // 
            // labelAddNewTask
            // 
            this.labelAddNewTask.AutoSize = true;
            this.labelAddNewTask.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAddNewTask.ForeColor = System.Drawing.Color.Red;
            this.labelAddNewTask.Location = new System.Drawing.Point(31, 45);
            this.labelAddNewTask.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAddNewTask.Name = "labelAddNewTask";
            this.labelAddNewTask.Size = new System.Drawing.Size(210, 36);
            this.labelAddNewTask.TabIndex = 3;
            this.labelAddNewTask.Text = "Add New Task";
            // 
            // labelAddTaskDesc
            // 
            this.labelAddTaskDesc.AutoSize = true;
            this.labelAddTaskDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAddTaskDesc.ForeColor = System.Drawing.Color.Black;
            this.labelAddTaskDesc.Location = new System.Drawing.Point(31, 98);
            this.labelAddTaskDesc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAddTaskDesc.Name = "labelAddTaskDesc";
            this.labelAddTaskDesc.Size = new System.Drawing.Size(232, 25);
            this.labelAddTaskDesc.TabIndex = 4;
            this.labelAddTaskDesc.Text = "Stay on top of your duties";
            // 
            // textBoxTaskDesc
            // 
            this.textBoxTaskDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxTaskDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxTaskDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTaskDesc.ForeColor = System.Drawing.Color.White;
            this.textBoxTaskDesc.Location = new System.Drawing.Point(209, 144);
            this.textBoxTaskDesc.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxTaskDesc.Name = "textBoxTaskDesc";
            this.textBoxTaskDesc.Size = new System.Drawing.Size(223, 29);
            this.textBoxTaskDesc.TabIndex = 161;
            // 
            // textBoxDueDate
            // 
            this.textBoxDueDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(174)))), ((int)(((byte)(189)))));
            this.textBoxDueDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxDueDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxDueDate.ForeColor = System.Drawing.Color.White;
            this.textBoxDueDate.Location = new System.Drawing.Point(209, 191);
            this.textBoxDueDate.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDueDate.Name = "textBoxDueDate";
            this.textBoxDueDate.Size = new System.Drawing.Size(223, 29);
            this.textBoxDueDate.TabIndex = 162;
            // 
            // labelTaskDesc
            // 
            this.labelTaskDesc.AutoSize = true;
            this.labelTaskDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTaskDesc.ForeColor = System.Drawing.Color.Black;
            this.labelTaskDesc.Location = new System.Drawing.Point(34, 145);
            this.labelTaskDesc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTaskDesc.Name = "labelTaskDesc";
            this.labelTaskDesc.Size = new System.Drawing.Size(155, 25);
            this.labelTaskDesc.TabIndex = 163;
            this.labelTaskDesc.Text = "Task description";
            // 
            // labelDueDate
            // 
            this.labelDueDate.AutoSize = true;
            this.labelDueDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDueDate.ForeColor = System.Drawing.Color.Black;
            this.labelDueDate.Location = new System.Drawing.Point(34, 192);
            this.labelDueDate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDueDate.Name = "labelDueDate";
            this.labelDueDate.Size = new System.Drawing.Size(91, 25);
            this.labelDueDate.TabIndex = 164;
            this.labelDueDate.Text = "Due date";
            // 
            // buttonAdd
            // 
            this.buttonAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(81)))), ((int)(((byte)(148)))));
            this.buttonAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAdd.ForeColor = System.Drawing.Color.White;
            this.buttonAdd.Location = new System.Drawing.Point(90, 246);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(135, 41);
            this.buttonAdd.TabIndex = 165;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = false;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(154)))), ((int)(((byte)(154)))));
            this.buttonCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(266, 246);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(135, 41);
            this.buttonCancel.TabIndex = 166;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // AddNewTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(507, 316);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.labelDueDate);
            this.Controls.Add(this.labelTaskDesc);
            this.Controls.Add(this.textBoxDueDate);
            this.Controls.Add(this.textBoxTaskDesc);
            this.Controls.Add(this.labelAddTaskDesc);
            this.Controls.Add(this.labelAddNewTask);
            this.Controls.Add(this.XpictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AddNewTask";
            this.ShowInTaskbar = false;
            this.Text = "AddNewTask";
            ((System.ComponentModel.ISupportInitialize)(this.XpictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox XpictureBox;
        private System.Windows.Forms.Label labelAddNewTask;
        private System.Windows.Forms.Label labelAddTaskDesc;
        private System.Windows.Forms.TextBox textBoxTaskDesc;
        private System.Windows.Forms.TextBox textBoxDueDate;
        private System.Windows.Forms.Label labelTaskDesc;
        private System.Windows.Forms.Label labelDueDate;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonCancel;
    }
}