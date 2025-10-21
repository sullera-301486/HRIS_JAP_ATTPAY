using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Net.Http;

namespace HRIS_JAP_ATTPAY
{
    public partial class ManageLeave : Form
    {
        // Firebase client
        private static FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        public ManageLeave()
        {
            InitializeComponent();
            SetFont();
        }

        private void SetFont()
        {
            try
            {
                labelLeaveManagement.Font = AttributesClass.GetFont("Roboto-Regular", 24f);
                labelAddLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                label1.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                label2.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelLeaveCredits.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void labelAddLeave_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            NewLeave newLeaveForm = new NewLeave();
            AttributesClass.ShowWithOverlay(parentForm, newLeaveForm);
        }

        private void AddLeaveItem(string submittedBy, string employee, string date, string leaveType, string period, Image photo)
        {
            var item = new LeaveList();
            item.SetData(submittedBy, employee, date, leaveType, period, photo);

            // Hook up revoke event
            item.RevokeClicked += async (s, e) =>
            {
                MessageBox.Show($"Revoked {employee}'s {leaveType} ({period})");
                flowLayoutPanel1.Controls.Remove(item);

                // After revoking, refresh ManageLeave list
                await LoadManageLeaveDataAsync();
            };

            flowLayoutPanel1.Controls.Add(item);
        }

        // Load ManageLeave data from Firebase with sorting by most recent
        public async Task LoadManageLeaveDataAsync()
        {
            try
            {
                flowLayoutPanel1.Controls.Clear();

                var manageLeaveList = await firebase
                    .Child("ManageLeave")
                    .OnceAsync<dynamic>();

                // 🟢 Create a list to hold leave items with their dates for sorting
                var leaveItems = new List<(string submittedBy, string employee, string date, string leaveType, string period, DateTime sortDate)>();

                // 🟢 Collect all leave items and parse their dates
                foreach (var leave in manageLeaveList)
                {
                    string submittedBy = leave.Object.submitted_by ?? "Unknown";
                    string employee = leave.Object.employee ?? "Unknown";
                    string date = leave.Object.created_at ?? "N/A";
                    string leaveType = leave.Object.leave_type ?? "N/A";
                    string period = leave.Object.period ?? "N/A";

                    // 🟢 Parse creation date for sorting
                    DateTime sortDate = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                    {
                        sortDate = parsedDate;
                    }
                    else
                    {
                        // Fallback if parsing fails
                        sortDate = DateTime.Now;
                    }

                    leaveItems.Add((submittedBy, employee, date, leaveType, period, sortDate));
                }

                //  Sort by most recent first (descending order)
                var sortedLeaves = leaveItems.OrderByDescending(x => x.sortDate).ToList();

                //  Add items to flowLayoutPanel in sorted order
                foreach (var leave in sortedLeaves)
                {
                    // Try to load employee image from EmployeeDetails
                    Image employeePhoto = await GetEmployeeImageAsync(leave.employee);
                    AddLeaveItem(leave.submittedBy, leave.employee, leave.date, leave.leaveType, leave.period, employeePhoto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading ManageLeave data: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper to get employee image from EmployeeDetails
        private async Task<Image> GetEmployeeImageAsync(string employeeName)
        {
            try
            {
                var employees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();

                foreach (var emp in employees)
                {
                    string fullName = $"{emp.Object.first_name} {emp.Object.middle_name} {emp.Object.last_name}"
                        .Replace("  ", " ")
                        .Trim();

                    if (string.Equals(fullName, employeeName, StringComparison.OrdinalIgnoreCase))
                    {
                        string imageUrl = emp.Object.image_url?.ToString();
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                var bytes = await client.GetByteArrayAsync(imageUrl);
                                using (var ms = new System.IO.MemoryStream(bytes))
                                {
                                    return Image.FromStream(ms);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore errors, fallback to default
            }

            return Properties.Resources.User1; // fallback default avatar
        }

        private async void ManageLeave_Load(object sender, EventArgs e)
        {
            await LoadManageLeaveDataAsync(); // Load real data from Firebase
        }

        private void labelLeaveCredits_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ManageLeaveCredits manageLeaveCredits = new ManageLeaveCredits();
            AttributesClass.ShowWithOverlay(parentForm, manageLeaveCredits);
        }
    }
}
