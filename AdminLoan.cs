using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminLoan : UserControl
    {
        private AttributesClassAlt panelLoaderAdminPayroll;
        public Panel AdminViewPanel;
        private string currentEmployeeId;
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        // Search timer for delayed filtering
        private System.Threading.Timer searchTimer;

        public AdminLoan(Panel targetPanel, string employeeId)
        {
            currentEmployeeId = employeeId;
            InitializeComponent();
            setFont();
            setDataGridViewAttributes();
            setTextBoxAttributes();

            // Add event handler for search textbox
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;

            AdminViewPanel = targetPanel;
            panelLoaderAdminPayroll = new AttributesClassAlt(AdminViewPanel);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Load data from Firebase
            LoadFirebaseData();
        }

        // Search functionality implementation
        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            // Dispose of existing timer if any
            searchTimer?.Dispose();

            // If search is empty, show all rows immediately
            if (string.IsNullOrEmpty(textBoxSearchEmployee.Text) ||
                textBoxSearchEmployee.Text == "Find Employee")
            {
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                {
                    if (!row.IsNewRow)
                        row.Visible = true;
                }
                return;
            }

            // Create a new timer that will trigger after 300ms of inactivity
            searchTimer = new System.Threading.Timer(_ =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    PerformSearch();
                });
            }, null, 300, System.Threading.Timeout.Infinite);
        }

        private void PerformSearch()
        {
            string searchText = textBoxSearchEmployee.Text.Trim();

            // If search is empty, show all rows
            if (string.IsNullOrEmpty(searchText) || searchText == "Find Employee")
            {
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                {
                    if (!row.IsNewRow)
                        row.Visible = true;
                }
                return;
            }

            // Filter rows based on search text
            foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
            {
                if (!row.IsNewRow)
                {
                    // Get values from relevant columns
                    string employeeId = row.Cells["EmployeeId"].Value?.ToString() ?? "";
                    string fullName = row.Cells["FullName"].Value?.ToString() ?? "";
                    string loanType = row.Cells["LoanType"].Value?.ToString() ?? "";

                    // Check if any of the columns contain the search text (case-insensitive)
                    bool isMatch = employeeId.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  fullName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  loanType.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                    row.Visible = isMatch;
                }
            }
        }

        private void setDataGridViewAttributes()
        {
            dataGridViewEmployee.ReadOnly = true;
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
            dataGridViewEmployee.CellMouseEnter += dataGridViewEmployee_CellMouseEnter;
            dataGridViewEmployee.CellMouseLeave += dataGridViewEmployee_CellMouseLeave;
            dataGridViewEmployee.CellClick += dataGridViewEmployee_CellClick;

            // Setup columns
            dataGridViewEmployee.Columns.Clear();

            // Numbering column (without header text)
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "RowNumber", HeaderText = "", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 25 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "LoanType", HeaderText = "Type", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "LoanAmount", HeaderText = "Amount", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Balance", HeaderText = "Balance", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remarks", HeaderText = "Remarks", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });

            var actionCol = new DataGridViewImageColumn
            {
                Name = "Action",
                HeaderText = "",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25
            };
            actionCol.Image = Properties.Resources.ExpandRight;
            dataGridViewEmployee.Columns.Add(actionCol);
        }

        private void setFont()
        {
            try
            {
                labelAdminPayroll.Font = AttributesClass.GetFont("Roboto-Light", 20f);
                labelPayrollDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMovePayroll.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Underline);
                labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelAddLoan.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxSearchEmployee, "Find Employee");
        }

        private void dataGridViewEmployee_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Action")
                dataGridViewEmployee.Cursor = Cursors.Hand;
        }

        private void dataGridViewEmployee_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewEmployee.Cursor = Cursors.Default;
        }

        private void dataGridViewEmployee_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Action")
            {
                // Get the selected employee ID from the grid
                string selectedEmployeeId = dataGridViewEmployee.Rows[e.RowIndex].Cells["EmployeeId"].Value?.ToString();

                if (!string.IsNullOrEmpty(selectedEmployeeId))
                {
                    Form parentForm = this.FindForm();
                    LoanDetails loanDetails = new LoanDetails(currentEmployeeId);

                    // Pass the selected employee ID to the form if needed
                    // loanDetails.SetEmployeeId(selectedEmployeeId);

                    AttributesClass.ShowWithOverlay(parentForm, loanDetails);
                }
            }
        }

        // Handle malformed JSON from Firebase
        private List<Dictionary<string, string>> ParseMalformedJson(string rawJson)
        {
            var records = new List<Dictionary<string, string>>();
            try
            {
                string cleanedJson = rawJson.Replace("\n", "").Replace("\r", "").Replace("\t", "")
                    .Replace("'", "\"").Replace("(", "[").Replace(")", "]")
                    .Replace("[null,", "[").Replace("], [", ",").Replace("}, {", "},{")
                    .Replace("},(", "},{").Replace("),{", "},{");

                var matches = Regex.Matches(cleanedJson, @"\{[^{}]*\}");
                foreach (Match match in matches)
                {
                    try
                    {
                        var record = new Dictionary<string, string>();
                        string objectStr = match.Value;
                        var kvpMatches = Regex.Matches(objectStr, @"""([^""]+)""\s*:\s*(""[^""]*""|\d+\.?\d*|true|false|null)");
                        foreach (Match kvpMatch in kvpMatches)
                        {
                            if (kvpMatch.Groups.Count >= 3)
                            {
                                string key = kvpMatch.Groups[1].Value;
                                string value = kvpMatch.Groups[2].Value.Trim('"');
                                record[key] = value;
                            }
                        }
                        if (record.Count > 0)
                        {
                            records.Add(record);
                        }
                    }
                    catch { continue; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("JSON parsing error: " + ex.Message);
            }

            return records;
        }

        // Load Firebase Data - FIXED FOR LOAN DATA
        private async void LoadFirebaseData()
        {
            try
            {
                dataGridViewEmployee.Rows.Clear();
                Console.WriteLine("Starting Firebase data load for loans...");

                // Load all necessary data
                var employeeDetails = new Dictionary<string, dynamic>();
                var loanRecords = new List<Dictionary<string, string>>();

                // EmployeeDetails - use OnceAsync<dynamic> for proper handling
                var empDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                foreach (var emp in empDetails)
                {
                    if (emp?.Object != null)
                    {
                        employeeDetails[emp.Key] = emp.Object;
                    }
                }

                // EmployeeLoans
                await LoadArrayBasedData("EmployeeLoans", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"]?.ToString() : null;
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        loanRecords.Add(item);
                    }
                });

                Console.WriteLine($"Loaded: {employeeDetails.Count} employees, {loanRecords.Count} loan records");

                // Populate DataGrid with loan data
                int counter = 1;
                foreach (var loan in loanRecords)
                {
                    string employeeId = loan.ContainsKey("employee_id") ? loan["employee_id"]?.ToString() : "";

                    if (string.IsNullOrEmpty(employeeId)) continue;

                    // Get employee details
                    string fullName = "Unknown";
                    if (employeeDetails.ContainsKey(employeeId))
                    {
                        dynamic empData = employeeDetails[employeeId];
                        string firstName = empData?.first_name?.ToString() ?? "";
                        string middleName = empData?.middle_name?.ToString() ?? "";
                        string lastName = empData?.last_name?.ToString() ?? "";

                        fullName = $"{firstName} {middleName} {lastName}".Trim();
                        if (string.IsNullOrEmpty(fullName)) fullName = "Unknown";
                    }

                    // Get loan details
                    string loanType = loan.ContainsKey("loan_type") ? loan["loan_type"]?.ToString() ?? "Unknown" : "Unknown";
                    string loanAmount = "0.00";
                    string balance = "0.00";

                    // Calculate remarks (total_payment_done / total_payment_terms)
                    string remarks = "N/A";
                    int totalPaymentDone = 0;
                    int totalPaymentTerms = 0;

                    // Parse amounts
                    if (loan.ContainsKey("loan_amount") && !string.IsNullOrEmpty(loan["loan_amount"]?.ToString()))
                    {
                        if (decimal.TryParse(loan["loan_amount"]?.ToString(), out decimal amount))
                            loanAmount = $"₱ {amount:N2}";
                    }

                    if (loan.ContainsKey("balance") && !string.IsNullOrEmpty(loan["balance"]?.ToString()))
                    {
                        if (decimal.TryParse(loan["balance"]?.ToString(), out decimal bal))
                            balance = $"₱ {bal:N2}";
                    }

                    // Calculate payment progress for remarks
                    if (loan.ContainsKey("total_payment_done") && !string.IsNullOrEmpty(loan["total_payment_done"]?.ToString()))
                    {
                        int.TryParse(loan["total_payment_done"]?.ToString(), out totalPaymentDone);
                    }

                    if (loan.ContainsKey("total_payment_terms") && !string.IsNullOrEmpty(loan["total_payment_terms"]?.ToString()))
                    {
                        int.TryParse(loan["total_payment_terms"]?.ToString(), out totalPaymentTerms);
                    }

                    if (totalPaymentTerms > 0)
                    {
                        remarks = $"{totalPaymentDone}/{totalPaymentTerms}";
                    }

                    // Add row to grid
                    dataGridViewEmployee.Rows.Add(
                        counter,
                        employeeId,
                        fullName,
                        loanType,
                        loanAmount,
                        balance,
                        remarks,
                        Properties.Resources.ExpandRight
                    );

                    counter++;
                }

                // Refresh the data grid view
                dataGridViewEmployee.Refresh();
                Console.WriteLine("Loan data load completed successfully.");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Firebase data: " + ex.Message);
                Console.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");

                // Load test data as fallback
                LoadTestData();
            }
        }

        private async Task LoadArrayBasedData(string childPath, Action<Dictionary<string, string>> processItem)
        {
            try
            {
                var jsonResponse = await firebase.Child(childPath).OnceAsJsonAsync();
                string rawJson = jsonResponse.ToString();

                // Check if the response is empty or null
                if (string.IsNullOrEmpty(rawJson) || rawJson == "null")
                {
                    Console.WriteLine($"No data found for {childPath}");
                    return;
                }

                var records = ParseMalformedJson(rawJson);
                Console.WriteLine($"Found {records.Count} records in {childPath}");

                foreach (var record in records)
                {
                    processItem(record);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading {childPath}: {ex.Message}");
            }
        }

        private void LoadTestData()
        {
            // Clear existing rows first
            dataGridViewEmployee.Rows.Clear();

            // Test data that matches the actual column structure
            for (int i = 1; i <= 5; i++)
            {
                dataGridViewEmployee.Rows.Add(
                    i,                          // RowNumber
                    $"JAP-00{i}",               // EmployeeId
                    $"Test Employee {i}",       // FullName
                    $"Loan Type {i}",           // LoanType
                    $"₱ {10000 + i * 5000:N2}", // LoanAmount
                    $"₱ {5000 + i * 2500:N2}",  // Balance
                    $"({i * 3}/48)",            // Remarks (payment progress)
                    Properties.Resources.ExpandRight // Action
                );
            }
        }

        private void labelMovePayroll_Click(object sender, EventArgs e)
        {
            panelLoaderAdminPayroll.LoadUserControl(new AdminPayroll(AdminViewPanel, currentEmployeeId));
        }

        private void labelAddLoan_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            AddLoan addLoan = new AddLoan(currentEmployeeId);
            AttributesClass.ShowWithOverlay(parentForm, addLoan);
        }

        private void pictureBoxFilters_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminLoan filterAdminLoan = new FilterAdminLoan();
            AttributesClass.ShowWithOverlay(parentForm, filterAdminLoan);
        }
    }
}
