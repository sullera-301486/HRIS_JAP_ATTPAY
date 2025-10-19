using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    //UPDATED
    public partial class AdminLoan : UserControl
    {
        private LoanFilterCriteria currentLoanFilters = new LoanFilterCriteria();
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
                string selectedEmployeeId = dataGridViewEmployee.Rows[e.RowIndex].Cells["EmployeeId"].Value?.ToString();
                string selectedLoanType = dataGridViewEmployee.Rows[e.RowIndex].Cells["LoanType"].Value?.ToString();

                if (!string.IsNullOrEmpty(selectedEmployeeId))
                {
                    Console.WriteLine($"DEBUG: Opening LoanDetails for employee: '{selectedEmployeeId}' | LoanType: '{selectedLoanType}'");

                    Form parentForm = this.FindForm();

                    // ✅ Pass both employee ID and loan type
                    LoanDetails loanDetails = new LoanDetails(selectedEmployeeId, selectedLoanType);

                    AttributesClass.ShowWithOverlay(parentForm, loanDetails);
                }
                else
                {
                    MessageBox.Show("Could not find employee ID for selected row.");
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
        // Load Firebase Data - IMPROVED VERSION
        // Replace the LoadFirebaseData method in AdminLoan.cs with this:

        private async void LoadFirebaseData()
        {
            try
            {
                dataGridViewEmployee.Rows.Clear();
                Console.WriteLine("=== STARTING LOAN DATA LOAD ===");

                using (var httpClient = new HttpClient())
                {
                    // 1. Get EmployeeDetails
                    string empDetailsUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/EmployeeDetails.json";
                    var empResponse = await httpClient.GetAsync(empDetailsUrl);

                    if (!empResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Failed to load employee details", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string empJson = await empResponse.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(empJson) || empJson == "null")
                    {
                        MessageBox.Show("No employee data found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    JObject employeeDetails = JObject.Parse(empJson);
                    Console.WriteLine($"Loaded {employeeDetails.Count} employees");

                    // 2. Get EmployeeLoans array
                    string loansUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/EmployeeLoans.json";
                    var loansResponse = await httpClient.GetAsync(loansUrl);

                    if (!loansResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Failed to load loan data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string loansJson = await loansResponse.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(loansJson) || loansJson == "null")
                    {
                        MessageBox.Show("No loan data found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Parse the loans array
                    List<JObject> loansList = new List<JObject>();
                    var loansToken = JToken.Parse(loansJson);

                    if (loansToken is JArray loansArray)
                    {
                        foreach (var item in loansArray)
                        {
                            if (item != null && item.Type != JTokenType.Null && item is JObject loanObj)
                            {
                                loansList.Add(loanObj);
                            }
                        }
                    }
                    else if (loansToken is JObject loansObj)
                    {
                        // Handle if it's stored as object instead of array
                        foreach (var prop in loansObj.Properties())
                        {
                            if (prop.Value != null && prop.Value.Type != JTokenType.Null && prop.Value is JObject loanObj)
                            {
                                loansList.Add(loanObj);
                            }
                        }
                    }

                    Console.WriteLine($"Loaded {loansList.Count} loan records");

                    // 3. Populate DataGridView
                    int counter = 1;
                    foreach (var loan in loansList)
                    {
                        try
                        {
                            string employeeId = loan["employee_id"]?.ToString() ?? "";

                            if (string.IsNullOrEmpty(employeeId))
                            {
                                Console.WriteLine("Skipping loan with empty employee ID");
                                continue;
                            }

                            Console.WriteLine($"Processing loan for employee: {employeeId}");

                            // Get employee details
                            string fullName = "Unknown Employee";
                            if (employeeDetails[employeeId] != null)
                            {
                                var empObj = (JObject)employeeDetails[employeeId];
                                string firstName = empObj["first_name"]?.ToString() ?? "";
                                string middleName = empObj["middle_name"]?.ToString() ?? "";
                                string lastName = empObj["last_name"]?.ToString() ?? "";

                                // Format full name
                                fullName = $"{firstName} {middleName} {lastName}".Trim();
                                while (fullName.Contains("  "))
                                    fullName = fullName.Replace("  ", " ");

                                if (string.IsNullOrWhiteSpace(fullName))
                                    fullName = "Unknown Employee";

                                Console.WriteLine($"  Employee found: {fullName}");
                            }
                            else
                            {
                                Console.WriteLine($"  ❌ Employee NOT FOUND: {employeeId}");
                            }

                            // Get loan details
                            string loanType = loan["loan_type"]?.ToString() ?? "Unknown";
                            string loanAmount = "₱ 0.00";
                            string balance = "₱ 0.00";

                            // Parse amounts
                            if (decimal.TryParse(loan["loan_amount"]?.ToString(), out decimal amount))
                                loanAmount = $"₱ {amount:N2}";

                            if (decimal.TryParse(loan["balance"]?.ToString(), out decimal bal))
                                balance = $"₱ {bal:N2}";

                            // Calculate payment progress for remarks
                            string remarks = "N/A";
                            if (int.TryParse(loan["total_payment_done"]?.ToString(), out int paymentDone) &&
                                int.TryParse(loan["total_payment_terms"]?.ToString(), out int paymentTerms) &&
                                paymentTerms > 0)
                            {
                                remarks = $"{paymentDone}/{paymentTerms}";
                            }

                            // Add row to DataGridView
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
                            Console.WriteLine($"  ✓ Added row for {employeeId} - {loanType}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing loan record: {ex.Message}");
                            continue;
                        }
                    }

                    Console.WriteLine($"=== DATA LOAD COMPLETED: {counter - 1} loans displayed ===");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load Firebase data: {ex.Message}",
                               "Data Load Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
                Console.WriteLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
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

            // Subscribe to filter events
            filterAdminLoan.FiltersApplied += ApplyLoanFilters;
            filterAdminLoan.FiltersReset += ResetLoanFilters;

            AttributesClass.ShowWithOverlay(parentForm, filterAdminLoan);
        }
        private void ApplyLoanFilters(LoanFilterCriteria filters)
        {
            System.Diagnostics.Debug.WriteLine($"ApplyLoanFilters called with SortBy: '{filters?.SortBy}'");
            currentLoanFilters = filters ?? new LoanFilterCriteria();
            ApplyLoanFilterAndSort();
        }

        private void ResetLoanFilters()
        {
            System.Diagnostics.Debug.WriteLine("=== RESETTING LOAN FILTERS ===");

            // Reset the filter criteria
            currentLoanFilters = new LoanFilterCriteria();

            // Clear the search text
            if (textBoxSearchEmployee.InvokeRequired)
            {
                textBoxSearchEmployee.Invoke((MethodInvoker)delegate
                {
                    textBoxSearchEmployee.Text = "";
                });
            }
            else
            {
                textBoxSearchEmployee.Text = "";
            }

            // Reload the data
            LoadFirebaseData();

            System.Diagnostics.Debug.WriteLine("=== LOAN FILTERS RESET COMPLETE ===");
        }

        private void ApplyLoanFilterAndSort()
        {
            dataGridViewEmployee.SuspendLayout();

            try
            {
                // Get all visible rows
                var visibleRows = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                {
                    if (!row.IsNewRow && row.Visible)
                    {
                        visibleRows.Add(row);
                    }
                }

                // Apply filters
                foreach (DataGridViewRow row in visibleRows)
                {
                    bool shouldShow = MatchesLoanFilters(row);
                    row.Visible = shouldShow;
                }

                // Apply sorting if specified
                if (!string.IsNullOrEmpty(currentLoanFilters?.SortBy))
                {
                    ApplySortingToLoanGrid();
                }

                // Update row numbers
                UpdateLoanRowNumbers();
            }
            finally
            {
                dataGridViewEmployee.ResumeLayout();
                dataGridViewEmployee.Refresh();
            }
        }

        private bool MatchesLoanFilters(DataGridViewRow row)
        {
            string employeeId = row.Cells["EmployeeId"].Value?.ToString() ?? "";
            string fullName = row.Cells["FullName"].Value?.ToString() ?? "";
            string loanType = row.Cells["LoanType"].Value?.ToString() ?? "";
            string loanAmountStr = row.Cells["LoanAmount"].Value?.ToString() ?? "₱ 0.00";
            string balanceStr = row.Cells["Balance"].Value?.ToString() ?? "₱ 0.00";

            // Remove currency symbol and parse
            decimal loanAmount = ParseCurrency(loanAmountStr);
            decimal balance = ParseCurrency(balanceStr);

            // Employee ID filter
            if (!string.IsNullOrEmpty(currentLoanFilters.EmployeeId) &&
                currentLoanFilters.EmployeeId.Trim().ToLower() != "search id")
            {
                if (!employeeId.ToLower().Contains(currentLoanFilters.EmployeeId.ToLower()))
                    return false;
            }

            // Name filter
            if (!string.IsNullOrEmpty(currentLoanFilters.Name) &&
                currentLoanFilters.Name.Trim().ToLower() != "search name")
            {
                if (!fullName.ToLower().Contains(currentLoanFilters.Name.ToLower()))
                    return false;
            }

            // Loan Type filter
            if (!string.IsNullOrEmpty(currentLoanFilters.LoanType) &&
                !currentLoanFilters.LoanType.Equals("Select loan type", StringComparison.OrdinalIgnoreCase))
            {
                if (!loanType.Equals(currentLoanFilters.LoanType, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // Note: Department and Position filters would require additional employee data
            // For now, skipping those since they're not directly in the loan grid

            // Amount range filter
            if (!string.IsNullOrEmpty(currentLoanFilters.AmountMinimum) &&
                currentLoanFilters.AmountMinimum.Trim().ToLower() != "minimum")
            {
                if (decimal.TryParse(currentLoanFilters.AmountMinimum, out decimal minAmount))
                {
                    if (loanAmount < minAmount)
                        return false;
                }
            }

            if (!string.IsNullOrEmpty(currentLoanFilters.AmountMaximum) &&
                currentLoanFilters.AmountMaximum.Trim().ToLower() != "maximum")
            {
                if (decimal.TryParse(currentLoanFilters.AmountMaximum, out decimal maxAmount))
                {
                    if (loanAmount > maxAmount)
                        return false;
                }
            }

            // Balance range filter
            if (!string.IsNullOrEmpty(currentLoanFilters.BalanceMinimum) &&
                currentLoanFilters.BalanceMinimum.Trim().ToLower() != "minimum")
            {
                if (decimal.TryParse(currentLoanFilters.BalanceMinimum, out decimal minBalance))
                {
                    if (balance < minBalance)
                        return false;
                }
            }

            if (!string.IsNullOrEmpty(currentLoanFilters.BalanceMaximum) &&
                currentLoanFilters.BalanceMaximum.Trim().ToLower() != "maximum")
            {
                if (decimal.TryParse(currentLoanFilters.BalanceMaximum, out decimal maxBalance))
                {
                    if (balance > maxBalance)
                        return false;
                }
            }

            return true;
        }

        private void ApplySortingToLoanGrid()
        {
            string sort = currentLoanFilters.SortBy.ToLower().Trim();
            System.Diagnostics.Debug.WriteLine($"Sorting by: '{sort}'");

            // Get visible rows with their data
            var rowDataList = new List<LoanRowData>();

            foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
            {
                if (!row.IsNewRow && row.Visible)
                {
                    rowDataList.Add(new LoanRowData
                    {
                        EmployeeId = row.Cells["EmployeeId"].Value?.ToString() ?? "",
                        FullName = row.Cells["FullName"].Value?.ToString() ?? "",
                        LoanType = row.Cells["LoanType"].Value?.ToString() ?? "",
                        LoanAmount = row.Cells["LoanAmount"].Value?.ToString() ?? "",
                        Balance = row.Cells["Balance"].Value?.ToString() ?? "",
                        Remarks = row.Cells["Remarks"].Value?.ToString() ?? ""
                    });
                }
            }

            // Sort the data
            List<LoanRowData> sortedData = null;

            switch (sort)
            {
                case "a-z":
                    sortedData = rowDataList.OrderBy(d => d.FullName, StringComparer.OrdinalIgnoreCase).ToList();
                    break;

                case "z-a":
                    sortedData = rowDataList.OrderByDescending(d => d.FullName, StringComparer.OrdinalIgnoreCase).ToList();
                    break;

                default:
                    return; // No sorting
            }

            if (sortedData == null) return;

            // Clear and re-add rows in sorted order
            dataGridViewEmployee.SuspendLayout();
            try
            {
                dataGridViewEmployee.Rows.Clear();

                int counter = 1;
                foreach (var data in sortedData)
                {
                    dataGridViewEmployee.Rows.Add(
                        counter,
                        data.EmployeeId,
                        data.FullName,
                        data.LoanType,
                        data.LoanAmount,
                        data.Balance,
                        data.Remarks,
                        Properties.Resources.ExpandRight
                    );
                    counter++;
                }
            }
            finally
            {
                dataGridViewEmployee.ResumeLayout();
            }
        }

        private decimal ParseCurrency(string currencyStr)
        {
            // Remove currency symbol (₱), spaces, and commas
            string cleaned = currencyStr.Replace("₱", "").Replace(",", "").Trim();

            if (decimal.TryParse(cleaned, out decimal value))
            {
                return value;
            }

            return 0m;
        }

        private void UpdateLoanRowNumbers()
        {
            int counter = 1;
            foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
            {
                if (!row.IsNewRow && row.Visible)
                {
                    if (row.Cells["RowNumber"] != null)
                    {
                        row.Cells["RowNumber"].Value = counter;
                    }
                    counter++;
                }
                else if (!row.IsNewRow)
                {
                    if (row.Cells["RowNumber"] != null)
                    {
                        row.Cells["RowNumber"].Value = "";
                    }
                }
            }
        }
    }
}