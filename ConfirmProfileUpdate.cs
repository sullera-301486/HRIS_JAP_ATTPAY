using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmProfileUpdate : Form
    {
        public bool UserConfirmed { get; private set; } = false;
        private string employeeId;
        private string employeeName;

        // Updated constructor to accept employee info
        public ConfirmProfileUpdate(string employeeId = null, string employeeName = null)
        {
            InitializeComponent();
            this.employeeId = employeeId;
            this.employeeName = employeeName;
            setFont();
            this.Load += ConfirmProfileUpdate_Load;
        }

        private async void ConfirmProfileUpdate_Load(object sender, EventArgs e)
        {
            try
            {
                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during form load: " + ex.Message);
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async void buttonConfirm_Click_1(object sender, EventArgs e)
        {
            this.UserConfirmed = true;
            this.DialogResult = DialogResult.OK;

            // Log the action
            if (!string.IsNullOrEmpty(employeeId))
            {
                string description = $"Employee profile updated for {employeeName ?? employeeId}";
                await AdminLogService.LogAdminAction(
                    AdminLogService.Actions.UPDATE_EMPLOYEE,
                    description,
                    employeeId
                );
            }

            this.Close();
        }
    }
}