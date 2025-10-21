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
using System.Net.Http;
using System.Net.Http;

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminArchives : Form
    {
        // Firebase client
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        // Store archived employees data
        private List<ArchivedEmployeeData> allArchivedEmployees = new List<ArchivedEmployeeData>();

        public AdminArchives()
        {
            InitializeComponent();
            setFont();
            setDataGridViewAttributes();

            // Load archived data
            LoadArchivedEmployees();
        }

        // Helper class to store archived employee data
        private class ArchivedEmployeeData
        {
            public string EmployeeId { get; set; }
            public string FullName { get; set; }
            public string Department { get; set; }
            public string Position { get; set; }
            public string Contact { get; set; }
            public string Email { get; set; }
            public DateTime? DateArchived { get; set; }
            public string ArchivedBy { get; set; }
        }

        private async void LoadArchivedEmployees()
        {
            try
            {
                dataGridViewEmployee.Rows.Clear();
                allArchivedEmployees.Clear();

                using (var httpClient = new HttpClient())
                {
                    // Get ONLY ArchivedEmployees
                    string archivesUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/ArchivedEmployees.json";
                    var archivesResponse = await httpClient.GetAsync(archivesUrl);

                    if (!archivesResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Failed to load archived employees", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string archivesJson = await archivesResponse.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(archivesJson) || archivesJson == "null")
                    {
                        MessageBox.Show("No archived employees found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    JObject archivedEmployees = JObject.Parse(archivesJson);

                    // Process each archived employee - ONLY from ArchivedEmployees node
                    foreach (var archive in archivedEmployees)
                    {
                        try
                        {
                            string employeeId = archive.Key;
                            JObject archiveObj = (JObject)archive.Value;

                            // Extract employee data from archived record
                            var employeeData = archiveObj["employee_data"] as JObject;
                            if (employeeData == null) continue;

                            // Extract leave credits for department and position
                            var leaveCredits = archiveObj["leave_credits"] as JObject;

                            // Safe null handling for employee details
                            string firstName = employeeData["first_name"]?.ToString() ?? "";
                            string middleName = employeeData["middle_name"]?.ToString() ?? "";
                            string lastName = employeeData["last_name"]?.ToString() ?? "";
                            string contact = employeeData["contact"]?.ToString() ?? "";
                            string email = employeeData["email"]?.ToString() ?? "";

                            // Get department and position from leave_credits or use defaults
                            string department = "Not Assigned";
                            string position = "Not Assigned";

                            if (leaveCredits != null)
                            {
                                department = leaveCredits["department"]?.ToString() ?? "Not Assigned";
                                position = leaveCredits["position"]?.ToString() ?? "Not Assigned";
                            }

                            // Get archive details
                            string archivedBy = archiveObj["archived_by"]?.ToString() ?? "System";
                            string archiveDateStr = archiveObj["archived_date"]?.ToString() ?? "";

                            DateTime? dateArchived = null;
                            if (!string.IsNullOrEmpty(archiveDateStr) && DateTime.TryParse(archiveDateStr, out DateTime archivedDate))
                            {
                                dateArchived = archivedDate;
                            }

                            var archivedEmployee = new ArchivedEmployeeData
                            {
                                EmployeeId = employeeId,
                                FullName = FormatFullName(firstName, middleName, lastName),
                                Department = department,
                                Position = position,
                                Contact = contact,
                                Email = email,
                                DateArchived = dateArchived,
                                ArchivedBy = archivedBy
                            };

                            allArchivedEmployees.Add(archivedEmployee);

                            System.Diagnostics.Debug.WriteLine($"Loaded archived: {employeeId} - {archivedEmployee.FullName}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing archived employee {archive.Key}: {ex.Message}");
                            continue;
                        }
                    }
                }

                // Populate the grid
                PopulateArchivedGrid();

                System.Diagnostics.Debug.WriteLine($"Successfully loaded {allArchivedEmployees.Count} archived employees");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load archived employees: " + ex.Message,
                               "Data Load Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
        }

        private void PopulateArchivedGrid()
        {
            dataGridViewEmployee.Rows.Clear();

            int counter = 1;
            foreach (var emp in allArchivedEmployees)
            {
                string dateArchived = emp.DateArchived?.ToString("MM/dd/yyyy") ?? "Unknown";

                dataGridViewEmployee.Rows.Add(
                    false, // Checkbox
                    emp.EmployeeId,
                    emp.FullName,
                    emp.Department,
                    emp.Position,
                    dateArchived,
                    "Restore" // Button text
                );
                counter++;
            }
        }

        // Enhanced full name formatting with null safety
        private string FormatFullName(string firstName, string middleName, string lastName)
        {
            try
            {
                string first = firstName ?? "";
                string middle = middleName ?? "";
                string last = lastName ?? "";

                // Remove extra whitespace and format
                string fullName = $"{first} {middle} {last}".Trim();
                while (fullName.Contains("  "))
                    fullName = fullName.Replace("  ", " ");

                return string.IsNullOrWhiteSpace(fullName) ? "Unknown" : fullName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error formatting full name: {ex.Message}");
                return "Unknown";
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelArchivedEmployees.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                labelArchivedRecords.Font = AttributesClass.GetFont("Roboto-Light", 14f);
                labelSelectAll.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Underline);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setDataGridViewAttributes()
        {
            dataGridViewEmployee.ReadOnly = false;
            dataGridViewEmployee.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewEmployee.MultiSelect = false;
            dataGridViewEmployee.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewEmployee.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewEmployee.DefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewEmployee.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewEmployee.GridColor = Color.White;
            dataGridViewEmployee.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewEmployee.ColumnHeadersHeight = 40;
            dataGridViewEmployee.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewEmployee.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            // Ensure columns exist
            dataGridViewEmployee.Columns.Clear();

            // Leftmost: Checkbox column
            var counterCol = new DataGridViewCheckBoxColumn
            {
                Name = "CB",
                HeaderText = "",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 84,
                TrueValue = true,
                FalseValue = false,
            };
            dataGridViewEmployee.Columns.Add(counterCol);

            // Main Data Columns
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104, ReadOnly = true });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 148, ReadOnly = true });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Department", HeaderText = "Department", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 148, ReadOnly = true });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 108, ReadOnly = true });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date Archived", HeaderText = "Date Archived", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 148, ReadOnly = true });

            // Rightmost: Button column
            var actionCol = new DataGridViewButtonColumn
            {
                Name = "Restore",
                HeaderText = "",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 148,
                FlatStyle = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                ReadOnly = false
            };
            dataGridViewEmployee.Columns.Add(actionCol);

            // Cell formatting for restore button
            dataGridViewEmployee.CellFormatting += (s, e) =>
            {
                if (dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Restore" && e.RowIndex >= 0)
                {
                    DataGridViewButtonCell btnCell = (DataGridViewButtonCell)dataGridViewEmployee.Rows[e.RowIndex].Cells["Restore"];
                    btnCell.Style.BackColor = Color.FromArgb(95, 218, 71);
                    btnCell.Style.ForeColor = Color.White;
                    btnCell.FlatStyle = FlatStyle.Flat;
                    btnCell.Value = "Restore";
                }
            };

            dataGridViewEmployee.CellPainting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Restore" && e.RowIndex >= 0)
                {
                    e.CellStyle.SelectionBackColor = Color.FromArgb(95, 218, 71); // same green
                    e.CellStyle.SelectionForeColor = Color.White;
                }
            };

            // Handle restore button clicks
            dataGridViewEmployee.CellClick += dataGridViewEmployee_CellClick;

            // --- Make Checkbox Clickable Immediately ---
            dataGridViewEmployee.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dataGridViewEmployee.IsCurrentCellDirty)
                    dataGridViewEmployee.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // --- Hand Cursor for Buttons and Checkboxes ---
            dataGridViewEmployee.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var colName = dataGridViewEmployee.Columns[e.ColumnIndex].Name;
                    if (colName == "Restore" || colName == "CB")
                        dataGridViewEmployee.Cursor = Cursors.Hand;
                }
            };

            dataGridViewEmployee.CellMouseLeave += (s, e) =>
            {
                dataGridViewEmployee.Cursor = Cursors.Default;
            };
        }

        private void dataGridViewEmployee_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Handle restore button click
            if (e.ColumnIndex == dataGridViewEmployee.Columns["Restore"].Index)
            {
                string employeeId = dataGridViewEmployee.Rows[e.RowIndex].Cells["ID"].Value?.ToString();
                string employeeName = dataGridViewEmployee.Rows[e.RowIndex].Cells["Name"].Value?.ToString();

                if (!string.IsNullOrEmpty(employeeId))
                {
                    // Show confirmation dialog
                    Form parentForm = this.FindForm();
                    ConfirmRecoverEmployee confirmRecoverEmployee = new ConfirmRecoverEmployee(employeeId, employeeName);
                    confirmRecoverEmployee.EmployeeRestored += OnEmployeeRestored;
                    AttributesClass.ShowWithOverlay(parentForm, confirmRecoverEmployee);
                }
            }
        }

        private void OnEmployeeRestored(string employeeId)
        {
            // Refresh the archived list after restoration
            LoadArchivedEmployees();

            // Also refresh the main employee list if needed
            AdminEmployee.RefreshEmployeeData();
        }

        private void labelSelectAll_Click(object sender, EventArgs e)
        {
            dataGridViewEmployee.ClearSelection();
            dataGridViewEmployee.CurrentCell = null;

            if (labelSelectAll.Text == "Select All")
            {
                labelSelectAll.Text = "Deselect All";
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                {
                    // Only modify rows that have a checkbox column
                    if (row.Cells["CB"] is DataGridViewCheckBoxCell cbCell)
                    {
                        cbCell.Value = true;
                    }
                }
            }
            else
            {
                labelSelectAll.Text = "Select All";
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                {
                    // Only modify rows that have a checkbox column
                    if (row.Cells["CB"] is DataGridViewCheckBoxCell cbCell)
                    {
                        cbCell.Value = false;
                    }
                }
            }
        }

        private void buttonRestoreSelected_Click(object sender, EventArgs e)
        {
            // Get selected employees
            var selectedEmployees = new List<string>();

            foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
            {
                if (row.Cells["CB"] is DataGridViewCheckBoxCell cbCell &&
                    cbCell.Value is bool isSelected && isSelected)
                {
                    string employeeId = row.Cells["ID"].Value?.ToString();
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        selectedEmployees.Add(employeeId);
                    }
                }
            }

            if (selectedEmployees.Count == 0)
            {
                MessageBox.Show("Please select at least one employee to restore.", "No Selection",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show confirmation for multiple restoration
            Form parentForm = this.FindForm();
            ConfirmRecoverEmployee confirmRecoverEmployee = new ConfirmRecoverEmployee(selectedEmployees);
            confirmRecoverEmployee.EmployeeRestored += OnEmployeeRestored;
            AttributesClass.ShowWithOverlay(parentForm, confirmRecoverEmployee);
        }

        // Method to refresh data (can be called from other forms)
        public void RefreshArchivedData()
        {
            LoadArchivedEmployees();
        }
    }
    public class ArchivedEmployee
    {
        public string archived_by { get; set; }
        public string archived_date { get; set; }
        public JObject employee_data { get; set; }
        public Dictionary<string, dynamic> attendance_records { get; set; }
        public JObject leave_credits { get; set; }
        public bool is_archived { get; set; }
    }
}