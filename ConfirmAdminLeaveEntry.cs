using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmAdminLeaveEntry : Form
    {
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private readonly string _employeeName;
        private readonly string _leaveType;
        private readonly string _startDate;
        private readonly string _endDate;

        public bool UserConfirmed { get; private set; }
        public event Action LeaveEntryConfirmed;

        public ConfirmAdminLeaveEntry(string employeeName, string leaveType, string startDate, string endDate)
        {
            InitializeComponent();
            _employeeName = employeeName;
            _leaveType = leaveType;
            _startDate = startDate;
            _endDate = endDate;

            this.StartPosition = FormStartPosition.CenterParent;
            setFont();

        }

        private async void buttonConfirm_Click_1(object sender, EventArgs e)
        {
            try
            {
                buttonConfirm.Enabled = false;
                buttonCancel.Enabled = false;
                buttonConfirm.Text = "Processing...";

                // ✅ Log the admin action to AdminLogs
                await LogAdminLeaveAction();

                this.UserConfirmed = true;
                this.DialogResult = DialogResult.OK;

                // Trigger the event
                LeaveEntryConfirmed?.Invoke();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error confirming leave entry: " + ex.Message);
                this.UserConfirmed = false;
                buttonConfirm.Enabled = true;
                buttonCancel.Enabled = true;
                buttonConfirm.Text = "Confirm";
            }
        }

        private void XpictureBox_Click_1(object sender, EventArgs e)
        {
            this.UserConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonCancel_Click_1(object sender, EventArgs e)
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


        private async Task LogAdminLeaveAction()
        {
            try
            {
                // Get employee ID for logging
                string employeeId = await GetEmployeeIdByName(_employeeName);

                // Create detailed description for admin log
                string description = $"Added {_leaveType} leave for {_employeeName} (Period: {_startDate} to {_endDate})";

                // Log to AdminLogs - this will appear in the admin logs data grid
                await AdminLogService.LogAdminAction(
                    AdminLogService.Actions.APPROVE_LEAVE, // Or create a new action like "ADD_LEAVE_ENTRY"
                    description,
                    employeeId
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging admin leave action: {ex.Message}");
                // Don't throw - we don't want logging failure to block the main action
            }
        }

        private async Task<string> GetEmployeeIdByName(string employeeName)
        {
            try
            {
                // Search in Leave Credits table
                var leaveCredits = await firebase
                    .Child("Leave Credits")
                    .OnceAsync<dynamic>();

                foreach (var item in leaveCredits)
                {
                    if (item.Object != null)
                    {
                        string fullName = item.Object.full_name?.ToString();
                        if (!string.IsNullOrEmpty(fullName) &&
                            fullName.Equals(employeeName, StringComparison.OrdinalIgnoreCase))
                        {
                            return item.Key; // Return employee ID
                        }
                    }
                }
                return "Unknown";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee ID: {ex.Message}");
                return "Error";
            }
        }


    }
}