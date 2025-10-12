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
    public partial class adminUpdateAttendanceConfirm : Form
    {
        public bool UserConfirmed { get; private set; } = false;
        private string employeeId;
        private string employeeName;
        private string attendanceDate;

        // Updated constructor to accept employee info and date
        public adminUpdateAttendanceConfirm(string employeeId = null, string employeeName = null, string attendanceDate = null)
        {
            InitializeComponent();
            this.employeeId = employeeId;
            this.employeeName = employeeName;
            this.attendanceDate = attendanceDate;
            setFont();
            this.Load += adminUpdateAttendanceConfirm_Load;

            Console.WriteLine($"adminUpdateAttendanceConfirm created with: ID={employeeId}, Name={employeeName}, Date={attendanceDate}");
        }

        private async void adminUpdateAttendanceConfirm_Load(object sender, EventArgs e)
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

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            UserConfirmed = true;

            // Log the action BEFORE setting DialogResult
            if (!string.IsNullOrEmpty(employeeId))
            {
                string description = $"Attendance updated for {employeeName ?? employeeId}";
                if (!string.IsNullOrEmpty(attendanceDate))
                {
                    description += $" on {attendanceDate}";
                }

                try
                {
                    Console.WriteLine($"Attempting to log admin action: {description}");

                    await AdminLogService.LogAdminAction(
                        AdminLogService.Actions.UPDATE_ATTENDANCE,
                        description,
                        employeeId
                    );

                    Console.WriteLine($"SUCCESS: Admin log created for attendance update");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to log admin action - {ex.Message}");
                    MessageBox.Show($"Warning: Attendance updated but logging failed: {ex.Message}", "Logging Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                Console.WriteLine("WARNING: No employeeId provided, admin log not created");
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            UserConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            UserConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void setFont()
        {
            try
            {
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}