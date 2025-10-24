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
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmEditSalary : Form
    {
        private readonly string employeeId;
        private readonly string effectivityDate;
        private readonly string department;
        private readonly string position;
        private readonly decimal dailyRate;
        private readonly decimal salary;
        private readonly EmployeeSalaryDetailsEdit parentEditForm;
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public ConfirmEditSalary(string employeeId, string effectivityDate, string department,
                               string position, decimal dailyRate, decimal salary,
                               EmployeeSalaryDetailsEdit parentEditForm)
        {
            InitializeComponent();
            this.employeeId = employeeId;
            this.effectivityDate = effectivityDate;
            this.department = department;
            this.position = position;
            this.dailyRate = dailyRate;
            this.salary = salary;
            this.parentEditForm = parentEditForm;
            setFont();
           
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

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {

        }

        private async Task StoreCurrentDataAsHistory()
        {
            try
            {
                // Get current employment info
                var currentEmployment = await GetCurrentEmploymentInfo();
                var currentEarnings = await GetCurrentPayrollEarnings();

                if (currentEmployment != null && currentEarnings != null)
                {
                    var historyRecord = new Dictionary<string, object>
                    {
                        ["employee_id"] = employeeId,
                        ["effectivity_date"] = effectivityDate,
                        ["previous_department"] = currentEmployment.GetValueOrDefault("department")?.ToString() ?? "N/A",
                        ["previous_position"] = currentEmployment.GetValueOrDefault("position")?.ToString() ?? "N/A",
                        ["previous_daily_rate"] = currentEarnings.GetValueOrDefault("daily_rate")?.ToString() ?? "0.00",
                        ["previous_salary"] = currentEarnings.GetValueOrDefault("basic_pay")?.ToString() ?? "0.00",
                        ["new_department"] = department,
                        ["new_position"] = position,
                        ["new_daily_rate"] = dailyRate.ToString("0.00"),
                        ["new_salary"] = salary.ToString("0.00"),
                        ["changed_by"] = "System Administrator",
                        ["change_date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    // Generate a unique key for the history record
                    var historyKey = Guid.NewGuid().ToString();

                    await firebase
                        .Child("SalaryHistory")
                        .Child(employeeId)
                        .Child(historyKey)
                        .PutAsync(historyRecord);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to store current data as history: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, object>> GetCurrentEmploymentInfo()
        {
            try
            {
                var employmentInfo = await firebase
                    .Child("EmploymentInfo")
                    .Child(employeeId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                return employmentInfo ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get current employment info: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, object>> GetCurrentPayrollEarnings()
        {
            try
            {
                var payrollId = await GetCurrentPayrollId();
                if (string.IsNullOrEmpty(payrollId))
                    return new Dictionary<string, object>();

                var earnings = await firebase
                    .Child("PayrollEarnings")
                    .Child(payrollId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                return earnings ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get current payroll earnings: {ex.Message}");
            }
        }

        private async Task<string> GetCurrentPayrollId()
        {
            try
            {
                // Try to find existing payroll data for this employee
                var allPayrollData = await firebase
                    .Child("PayrollData")
                    .OnceAsync<Dictionary<string, object>>();

                foreach (var payroll in allPayrollData)
                {
                    if (payroll.Object?.GetValueOrDefault("employee_id")?.ToString() == employeeId)
                    {
                        return payroll.Key;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get current payroll ID: {ex.Message}");
            }
        }

        private async Task UpdateEmployeeInformation()
        {
            try
            {
                // Update EmploymentInfo
                await UpdateEmploymentInfo();

                // Update PayrollEarnings
                await UpdatePayrollEarnings();


            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update employee information: {ex.Message}");
            }
        }

        private async Task UpdateEmploymentInfo()
        {
            try
            {
                // Get current employment info to preserve other fields
                var employmentInfo = await GetCurrentEmploymentInfo();

                // Update department and position
                employmentInfo["department"] = department;
                employmentInfo["position"] = position;
                employmentInfo["last_updated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Save back to Firebase
                await firebase
                    .Child("EmploymentInfo")
                    .Child(employeeId)
                    .PutAsync(employmentInfo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update employment info: {ex.Message}");
            }
        }

        private async Task UpdatePayrollEarnings()
        {
            try
            {
                var payrollId = await GetOrCreatePayrollId();

                var earningsData = new Dictionary<string, object>
                {
                    ["daily_rate"] = dailyRate.ToString("0.00"),
                    ["basic_pay"] = salary.ToString("0.00"),
                    ["commission"] = "0.00",
                    ["incentives"] = "0.00",
                    ["overtime_pay"] = "0.00",
                    ["food_allowance"] = "0.00",
                    ["gas_allowance"] = "0.00",
                    ["communication"] = "0.00",
                    ["gondola"] = "0.00",
                    ["days_present"] = "0",
                    ["total_earnings"] = salary.ToString("0.00"),
                    ["last_updated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase
                    .Child("PayrollEarnings")
                    .Child(payrollId)
                    .PutAsync(earningsData);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update payroll earnings: {ex.Message}");
            }
        }

        private async Task<string> GetOrCreatePayrollId()
        {
            try
            {
                // Try to find existing payroll data for this employee
                var allPayrollData = await firebase
                    .Child("PayrollData")
                    .OnceAsync<Dictionary<string, object>>();

                foreach (var payroll in allPayrollData)
                {
                    if (payroll.Object?.GetValueOrDefault("employee_id")?.ToString() == employeeId)
                    {
                        return payroll.Key;
                    }
                }

                // Create new payroll data if not found
                var newPayrollId = Guid.NewGuid().ToString();
                var payrollData = new Dictionary<string, object>
                {
                    ["employee_id"] = employeeId,
                    ["payroll_id"] = newPayrollId,
                    ["cutoff_start"] = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd"),
                    ["cutoff_end"] = DateTime.Now.ToString("yyyy-MM-dd"),
                    ["net_pay"] = salary.ToString("0.00"),
                    ["created_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["last_updated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase
                    .Child("PayrollData")
                    .Child(newPayrollId)
                    .PutAsync(payrollData);

                return newPayrollId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get or create payroll ID: {ex.Message}");
            }
        }

        private async void buttonConfirm_Click_1(object sender, EventArgs e)
        {
            try
            {
                buttonConfirm.Enabled = false;
                buttonConfirm.Text = "Updating...";

                // Store current data as history before updating
                await StoreCurrentDataAsHistory();

                // Update employee information in Firebase
                await UpdateEmployeeInformation();

                // Refresh the parent form's history
                parentEditForm?.RefreshSalaryHistory();

                MessageBox.Show("Salary information updated successfully!", "Success",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating salary information: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonConfirm.Enabled = true;
                buttonConfirm.Text = "Confirm";
            }
        }

        private void buttonCancel_Click_1(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}