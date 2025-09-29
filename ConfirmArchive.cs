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

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmArchive : Form
    {
        private readonly string employeeId;
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        // Add this property
        public bool UserConfirmed { get; private set; }

        // Add this event to notify when archiving is complete
        public event Action EmployeeArchived;

        public ConfirmArchive(string employeeId)
        {
            InitializeComponent();
            this.employeeId = employeeId;
            this.StartPosition = FormStartPosition.CenterParent;
            setFont();

            // Ensure the form is fully loaded before any operations
            this.Load += ConfirmArchive_Load;
            this.Shown += ConfirmArchive_Shown;
        }

        private void ConfirmArchive_Load(object sender, EventArgs e)
        {
            // Initialization code here
        }

        private void ConfirmArchive_Shown(object sender, EventArgs e)
        {
            // Any operations that require the form to be fully shown can go here
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
                if (IsHandleCreated)
                {
                    if (labelMessage.InvokeRequired)
                    {
                        labelMessage.Invoke(new Action(() =>
                            labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f)));
                    }
                    else
                    {
                        labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                    }

                    // Apply fonts safely for all controls
                    SafeSetFont(labelRequestConfirm, AttributesClass.GetFont("Roboto-Regular", 16f));
                    SafeSetFont(buttonCancel, AttributesClass.GetFont("Roboto-Light", 12f));
                    SafeSetFont(buttonConfirm, AttributesClass.GetFont("Roboto-Regular", 12f));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void SafeSetFont(Control control, Font font)
        {
            if (control != null && !control.IsDisposed)
            {
                if (control.InvokeRequired)
                {
                    if (IsHandleCreated)
                    {
                        control.Invoke(new Action(() => control.Font = font));
                    }
                    else
                    {
                        // If handle not created, set font directly (will be applied when handle is created)
                        control.Font = font;
                    }
                }
                else
                {
                    control.Font = font;
                }
            }
        }

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable buttons during operation
                buttonConfirm.Enabled = false;
                buttonCancel.Enabled = false;

                await ArchiveEmployee();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error archiving employee: " + ex.Message);
            }
            finally
            {
                buttonConfirm.Enabled = true;
                buttonCancel.Enabled = true;
            }
        }

        private async Task ArchiveEmployee()
        {
            try
            {
                if (string.IsNullOrEmpty(employeeId))
                {
                    MessageBox.Show("Employee ID is missing.");
                    return;
                }

                // Check if form handle is created before any UI operations
                if (!IsHandleCreated)
                {
                    // Wait for handle to be created
                    await Task.Run(() =>
                    {
                        while (!IsHandleCreated)
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                    });
                }

                // Show loading state
                SafeSetButtonText(buttonConfirm, "Archiving...");

                // 1. Get employee data from EmployeeDetails
                var employeeData = await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                if (employeeData == null)
                {
                    MessageBox.Show("Employee not found in database.");
                    return;
                }

                // 2. Get employment info
                var employmentData = await GetEmploymentInfo();

                // 3. Create archived employee record
                var archivedEmployee = new
                {
                    employee_data = employeeData,
                    employment_info = employmentData,
                    archived_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    archived_by = "System", // You can change this to current user
                    is_archived = true
                };

                // 4. Save to ArchivedEmployees
                await firebase
                    .Child("ArchivedEmployees")
                    .Child(employeeId)
                    .PutAsync(archivedEmployee);

                // 5. Remove from active tables
                await RemoveFromActiveTables();

                // Set UserConfirmed to true on success
                this.UserConfirmed = true;
                this.DialogResult = DialogResult.OK;

                // Trigger the event to notify that employee was archived
                EmployeeArchived?.Invoke();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error archiving employee: " + ex.Message);
                this.UserConfirmed = false;
                // Don't set DialogResult here so the form stays open
            }
        }

        private async Task<dynamic> GetEmploymentInfo()
        {
            try
            {
                // EmploymentInfo is stored as an object, not an array
                var employmentData = await firebase
                    .Child("EmploymentInfo")
                    .OnceSingleAsync<Dictionary<string, dynamic>>();

                if (employmentData != null)
                {
                    // Find the employment record by employee_id
                    foreach (var kvp in employmentData)
                    {
                        if (kvp.Value != null)
                        {
                            // Check if this is the employee we're looking for
                            var empId = kvp.Value.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                return kvp.Value;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employment info: {ex.Message}");
                return null;
            }
        }

        private async Task RemoveFromActiveTables()
        {
            try
            {
                // Remove from EmployeeDetails
                await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .DeleteAsync();

                // Remove from EmploymentInfo
                await RemoveFromEmploymentInfo();

                // Remove from Work_Schedule
                await RemoveFromWorkSchedule();

                // Remove user access
                await RemoveUserAccess();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from active tables: {ex.Message}");
                // Continue even if some deletions fail
            }
        }

        private async Task RemoveFromEmploymentInfo()
        {
            try
            {
                var employmentData = await firebase
                    .Child("EmploymentInfo")
                    .OnceSingleAsync<Dictionary<string, dynamic>>();

                if (employmentData != null)
                {
                    // Find the employment key for this employee
                    string employmentKeyToRemove = null;
                    foreach (var kvp in employmentData)
                    {
                        if (kvp.Value != null)
                        {
                            var empId = kvp.Value.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                employmentKeyToRemove = kvp.Key;
                                break;
                            }
                        }
                    }

                    // Remove the employment record
                    if (!string.IsNullOrEmpty(employmentKeyToRemove))
                    {
                        await firebase
                            .Child("EmploymentInfo")
                            .Child(employmentKeyToRemove)
                            .DeleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from employment info: {ex.Message}");
            }
        }

        private async Task RemoveFromWorkSchedule()
        {
            try
            {
                // Work_Schedule is stored as an array
                var scheduleData = await firebase
                    .Child("Work_Schedule")
                    .OnceSingleAsync<List<dynamic>>();

                if (scheduleData != null)
                {
                    var newScheduleList = new List<dynamic>();

                    foreach (var item in scheduleData)
                    {
                        if (item != null)
                        {
                            var empId = item.employee_id?.ToString();
                            if (empId != employeeId)
                            {
                                newScheduleList.Add(item);
                            }
                        }
                    }

                    // Update the entire array
                    await firebase
                        .Child("Work_Schedule")
                        .PutAsync(newScheduleList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from work schedule: {ex.Message}");
            }
        }

        private async Task RemoveUserAccess()
        {
            try
            {
                var usersData = await firebase
                    .Child("Users")
                    .OnceSingleAsync<Dictionary<string, dynamic>>();

                if (usersData != null)
                {
                    // Find the user key for this employee
                    string userKeyToRemove = null;
                    foreach (var kvp in usersData)
                    {
                        if (kvp.Value != null)
                        {
                            var empId = kvp.Value.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                userKeyToRemove = kvp.Key;
                                break;
                            }
                        }
                    }

                    // Remove the user
                    if (!string.IsNullOrEmpty(userKeyToRemove))
                    {
                        await firebase
                            .Child("Users")
                            .Child(userKeyToRemove)
                            .DeleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing user access: {ex.Message}");
                throw; // Re-throw to handle in the main method
            }
        }

        private void SafeSetButtonText(Button button, string text)
        {
            if (button != null && !button.IsDisposed)
            {
                if (button.InvokeRequired)
                {
                    if (IsHandleCreated)
                    {
                        button.Invoke(new Action(() => button.Text = text));
                    }
                    else
                    {
                        button.Text = text;
                    }
                }
                else
                {
                    button.Text = text;
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Now it's safe to perform UI operations
        }
    }
}