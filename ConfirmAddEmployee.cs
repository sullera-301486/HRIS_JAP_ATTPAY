using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmAddEmployee : Form
    {
        public bool UserConfirmed { get; private set; } = false;
        private string employeeId;
        private string employeeName;

        // Updated constructor to accept employee info
        public ConfirmAddEmployee(string employeeId = null, string employeeName = null)
        {
            InitializeComponent();
            this.employeeId = employeeId;
            this.employeeName = employeeName;
            setFont();
            this.Load += ConfirmProfileUpdate_Load;
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.UserConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.UserConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            this.UserConfirmed = true;
            this.DialogResult = DialogResult.OK;

            // Log the action
            if (!string.IsNullOrEmpty(employeeId))
            {
                string description = $"Added new employee: {employeeName ?? employeeId}";
                await AdminLogService.LogAdminAction(
                    AdminLogService.Actions.ADD_EMPLOYEE,
                    description,
                    employeeId
                );
            }

            this.Close();
        }

        private async void ConfirmProfileUpdate_Load(object sender, EventArgs e)
        {
            try
            {
                await Task.Delay(200);
                // Add any initialization code here if needed
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during form load: " + ex.Message);
            }
        }
    }
}