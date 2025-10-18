using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Net.Http;
using Newtonsoft.Json;

namespace HRIS_JAP_ATTPAY
{
    public static class PayrollExportAllData
    {
        private static readonly FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );
        private static readonly Dictionary<string, PayrollPreset> payrollPresets = new Dictionary<string, PayrollPreset>
{
    {
        "JAP-001",
        new PayrollPreset
        {
            DailyRate = "500",
            Salary = "15000",
            Incentives = "1000",
            Commission = "500",
            FoodAllowance = "300",
            Communication = "200",
            GrossPay = "17000",
            WithholdingTax = "1000",
            SSS = "500",
            PagIbig = "200",
            Philhealth = "300",
            TotalDeductions = "2000",
            NetPay = "15000",
            SSSLoan = "0",
            PagIbigLoan = "0",
            CarLoan = "0",
            HousingLoan = "0",
            CashAdvance = "0",
            CoopLoan = "0",
            CoopContribution = "0",
            Others = "0",
            TaxDetails = "N/A",
            SSSDetails = "N/A",
            PagIbigDetails = "N/A",
            PhilhealthDetails = "N/A",
            SSSLoanDetails = "N/A",
            PagIbigLoanDetails = "N/A",
            CarLoanDetails = "N/A",
            HousingLoanDetails = "N/A",
            CashAdvanceDetails = "N/A",
            CoopLoanDetails = "N/A",
            CoopContributionDetails = "N/A",
            OthersDetails = "N/A",
            VacationLeaveCredit = "0",
            VacationLeaveDebit = "0",
            VacationLeaveBalance = "0",
            SickLeaveBalance = "6",
            SickLeaveCredit = "6",
            SickLeaveDebit = "0",
            Gondola = "400",
            GasAllowance = "300",
        }
    },
    {
        "JAP-002",
        new PayrollPreset
        {
            DailyRate = "600",
            Salary = "18000",
            Incentives = "1500",
            Commission = "600",
            FoodAllowance = "400",
            Communication = "250",
            GrossPay = "20000",
            WithholdingTax = "1200",
            SSS = "600",
            PagIbig = "250",
            Philhealth = "350",
            TotalDeductions = "2400",
            NetPay = "17600",
            // Optional details
            SSSLoan = "0",
            PagIbigLoan = "0",
            CarLoan = "0",
            HousingLoan = "0",
            CashAdvance = "0",
            CoopLoan = "0",
            CoopContribution = "0",
            Others = "0",
            TaxDetails = "N/A",
            SSSDetails = "N/A",
            PagIbigDetails = "N/A",
            PhilhealthDetails = "N/A",
            SSSLoanDetails = "N/A",
            PagIbigLoanDetails = "N/A",
            CarLoanDetails = "N/A",
            HousingLoanDetails = "N/A",
            CashAdvanceDetails = "N/A",
            CoopLoanDetails = "N/A",
            CoopContributionDetails = "N/A",
            OthersDetails = "N/A",
            VacationLeaveCredit = "0",
            VacationLeaveDebit = "0",
            VacationLeaveBalance = "0",
            SickLeaveBalance = "0",
            SickLeaveCredit = "0",
            SickLeaveDebit = "0",
            Gondola = "400",
            GasAllowance = "300",
        }
    },
    // Add more employees here in the same multi-line style
};

        // Main entry
        public static async Task GenerateAllPayrollsAsync(string savePath)
        {
            int employeeCount = 0;

            try
            {
                var payrollList = new List<PayrollExportData>();
                var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                int skippedArchived = 0, skippedNoId = 0, skippedDuplicate = 0, processedNoName = 0;

                //  Fetch EmploymentInfo
                string empJson = await firebase.Child("EmploymentInfo").OnceAsJsonAsync();
                if (string.IsNullOrWhiteSpace(empJson))
                {
                    MessageBox.Show("No employees found in EmploymentInfo.", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Fetch ArchivedEmployees (for skip)
                string archivedJson = await firebase.Child("ArchivedEmployees").OnceAsJsonAsync();
                HashSet<string> archivedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(archivedJson))
                {
                    try
                    {
                        JsonDocument aDoc = JsonDocument.Parse(archivedJson);
                        if (aDoc.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var prop in aDoc.RootElement.EnumerateObject())
                            {
                                if (!string.IsNullOrEmpty(prop.Name))
                                    archivedIds.Add(prop.Name);
                            }
                        }
                        else if (aDoc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in aDoc.RootElement.EnumerateArray())
                            {
                                if (item.TryGetProperty("employee_id", out JsonElement idEl))
                                    archivedIds.Add(idEl.GetString());
                            }
                        }
                    }
                    catch { }
                }

                //  Fetch EmployeeDetails map (id -> name parts)
                string empDetailsJson = await firebase.Child("EmployeeDetails").OnceAsJsonAsync();
                var employeeDetailsMap = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(empDetailsJson))
                {
                    try
                    {
                        JsonDocument d = JsonDocument.Parse(empDetailsJson);
                        if (d.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var prop in d.RootElement.EnumerateObject())
                            {
                                // prop.Name is the employee id
                                var inner = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                if (prop.Value.ValueKind == JsonValueKind.Object)
                                {
                                    foreach (var kv in prop.Value.EnumerateObject())
                                    {
                                        if (kv.Value.ValueKind == JsonValueKind.String || kv.Value.ValueKind == JsonValueKind.Number)
                                            inner[kv.Name] = kv.Value.ToString();
                                    }
                                }
                                if (!employeeDetailsMap.ContainsKey(prop.Name))
                                    employeeDetailsMap[prop.Name] = inner;
                            }
                        }
                    }
                    catch { }
                }

                //  Parse EmploymentInfo
                JsonDocument doc = JsonDocument.Parse(empJson);
                JsonElement root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var empProp in root.EnumerateObject())
                    {
                        PayrollExportData p = BuildPayrollFromJson(empProp.Value, empProp.Name, archivedIds,
                            employeeDetailsMap, ref skippedArchived, ref skippedNoId, ref processedNoName);
                        if (p != null)
                        {
                            if (seenIds.Add(p.EmployeeId))
                                payrollList.Add(p);
                            else
                                skippedDuplicate++;
                        }
                    }
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var empEl in root.EnumerateArray())
                    {
                        PayrollExportData p = BuildPayrollFromJson(empEl, null, archivedIds,
                            employeeDetailsMap, ref skippedArchived, ref skippedNoId, ref processedNoName);
                        if (p != null)
                        {
                            if (seenIds.Add(p.EmployeeId))
                                payrollList.Add(p);
                            else
                                skippedDuplicate++;
                        }
                    }
                }

                if (payrollList.Count == 0)
                {
                    MessageBox.Show("No valid employee records found.", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Store the count for logging
                employeeCount = payrollList.Count;

                // 🔹 Pull Attendance per employee
                foreach (var data in payrollList)
                {
                    try
                    {
                        var attendance = await firebase.Child("Attendance")
                            .OrderBy("employee_id").EqualTo(data.EmployeeId)
                            .OnceAsync<object>();

                        data.Days = attendance.Count.ToString();
                        data.DaysPresent = data.Days;

                        double overtime = 0;
                        foreach (var a in attendance)
                        {
                            var attObj = a.Object as IDictionary<string, object>;
                            if (attObj != null && attObj.ContainsKey("overtime_hours"))
                            {
                                if (double.TryParse(attObj["overtime_hours"].ToString(), out double val))
                                    overtime += val;
                            }
                        }
                        data.Overtime = overtime.ToString("0.00");
                    }
                    catch { }
                }

                // Generate Excel workbook
                CreateMultiSheetWorkbook(savePath, payrollList);

                // Log the export action to Firebase
                bool logSuccess = await LogPayrollExportAllToFirebase(employeeCount);

                string summary = $"Export complete.{Environment.NewLine}{Environment.NewLine}Exported: {payrollList.Count} employees";
                if (logSuccess)
                {
                    summary += "\nLogged to system successfully!";
                }
                else
                {
                    summary += "\nFailed to log to system.";
                }

                MessageBox.Show(summary, "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting payroll: " + ex.Message, "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task<bool> LogPayrollExportAllToFirebase(int employeeCount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Firebase database URL
                    string firebaseUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/";

                    // Create log entry with the four required values - TIME IN 12-HOUR FORMAT
                    var logEntry = new
                    {
                        date = DateTime.Now.ToString("yyyy-MM-dd"),
                        time = DateTime.Now.ToString("hh:mm tt").ToUpper(), // 12-hour format with AM/PM
                        action = "Export all payroll",
                        details = $"Exported payroll for {employeeCount} employees"
                    };

                    // Post to PayrollLogs table - Firebase automatically creates it if it doesn't exist
                    string payrollLogsUrl = $"{firebaseUrl}PayrollLogs.json";
                    string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(logEntry);
                    var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(payrollLogsUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine("Firebase log for all payroll export created successfully!");
                        return true;
                    }
                    else
                    {
                        // Log failure but don't interrupt the export process
                        string errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Firebase log failed: {response.StatusCode} - {errorContent}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't show error to user for logging failure, just debug
                System.Diagnostics.Debug.WriteLine($"Firebase logging error: {ex.Message}");
                return false;
            }
        }

        private static PayrollExportData BuildPayrollFromJson(
            JsonElement empEl,
            string idFallback,
            HashSet<string> archivedIds,
            Dictionary<string, Dictionary<string, string>> employeeDetailsMap,
            ref int skippedArchived,
            ref int skippedNoId,
            ref int processedNoName)
        {
            try
            {
                // Flatten properties for easy access
                Dictionary<string, string> props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                ExtractAllProperties(empEl, props);

                // Resolve employee ID (fallback is property name e.g., "JAP-001")
                string empId = idFallback;
                if (props.ContainsKey("employee_id")) empId = props["employee_id"];
                if (props.ContainsKey("employment_id") && string.IsNullOrEmpty(empId)) empId = props["employment_id"];
                if (string.IsNullOrWhiteSpace(empId))
                {
                    skippedNoId++;
                    return null;
                }

                // Skip archived by ID
                if (archivedIds.Contains(empId))
                {
                    skippedArchived++;
                    return null;
                }

                // ---------- NAME RESOLUTION ----------
                string name = null;

                // 1) Prefer local EmployeeDetails object inside this node (if present)
                if (empEl.ValueKind == JsonValueKind.Object)
                {
                    JsonElement detailsNode;
                    if (empEl.TryGetProperty("EmployeeDetails", out detailsNode) && detailsNode.ValueKind == JsonValueKind.Object)
                    {
                        name = ExtractNameFromDetailsNode(detailsNode);
                    }
                    else if (empEl.TryGetProperty("employee_data", out detailsNode) && detailsNode.ValueKind == JsonValueKind.Object)
                    {
                        name = ExtractNameFromDetailsNode(detailsNode);
                    }
                }

                // 2) If still null, check flattened props
                if (string.IsNullOrWhiteSpace(name))
                {
                    string full = props.ContainsKey("full_name") ? props["full_name"] : null;
                    if (!string.IsNullOrWhiteSpace(full))
                        name = full.Trim();
                    else
                    {
                        string first = props.ContainsKey("first_name") ? props["first_name"] : "";
                        string middle = props.ContainsKey("middle_name") ? props["middle_name"] : "";
                        string last = props.ContainsKey("last_name") ? props["last_name"] : "";
                        string assembled = (first + " " + middle + " " + last).Trim();
                        if (!string.IsNullOrWhiteSpace(assembled)) name = assembled;
                    }
                }

                // 3) Final fallback: look up in the top-level EmployeeDetails map
                if (string.IsNullOrWhiteSpace(name) && employeeDetailsMap != null && employeeDetailsMap.ContainsKey(empId))
                {
                    var map = employeeDetailsMap[empId];
                    string full = map.ContainsKey("full_name") ? map["full_name"] : null;
                    if (!string.IsNullOrWhiteSpace(full)) name = full.Trim();
                    else
                    {
                        string first = map.ContainsKey("first_name") ? map["first_name"] : "";
                        string middle = map.ContainsKey("middle_name") ? map["middle_name"] : "";
                        string last = map.ContainsKey("last_name") ? map["last_name"] : "";
                        string assembled = (first + " " + middle + " " + last).Trim();
                        if (!string.IsNullOrWhiteSpace(assembled)) name = assembled;
                    }
                }

                // 4) Fallback
                if (string.IsNullOrWhiteSpace(name))
                {
                    processedNoName++;
                    name = "(No Name)";
                }

                string dept = props.ContainsKey("department") ? props["department"] : "";
                string pos = props.ContainsKey("position") ? props["position"] : "";

                // ---------- LOAD PRESET ----------
                PayrollPreset preset = payrollPresets.ContainsKey(empId) ? payrollPresets[empId] : null;

                return new PayrollExportData
                {
                    EmployeeId = empId,
                    EmployeeName = name,
                    Department = dept,
                    Position = pos,
                    DateCovered = string.Format("{0:MMM 1} - {0:MMM 15, yyyy}", DateTime.Now),

                    // Payroll values from preset or defaults
                    DailyRate = preset?.DailyRate ?? "0",
                    Salary = preset?.Salary ?? "0",
                    Incentives = preset?.Incentives ?? "0",
                    Commission = preset?.Commission ?? "0",
                    FoodAllowance = preset?.FoodAllowance ?? "0",
                    Communication = preset?.Communication ?? "0",
                    GrossPay = preset?.GrossPay ?? "0",
                    WithholdingTax = preset?.WithholdingTax ?? "0",
                    SSS = preset?.SSS ?? "0",
                    PagIbig = preset?.PagIbig ?? "0",
                    Philhealth = preset?.Philhealth ?? "0",
                    TotalDeductions = preset?.TotalDeductions ?? "0",
                    NetPay = preset?.NetPay ?? "0",

                    Days = preset?.Days ?? "0",
                    DaysPresent = preset?.DaysPresent ?? "0",
                    BasicPay = preset?.BasicPay ?? "0",
                    Overtime = preset?.Overtime ?? "0",
                    OvertimePerHour = preset?.OvertimePerHour ?? "0",
                    OvertimePerMinute = preset?.OvertimePerMinute ?? "0",

                    SSSLoan = preset?.SSSLoan ?? "0",
                    PagIbigLoan = preset?.PagIbigLoan ?? "0",
                    CarLoan = preset?.CarLoan ?? "0",
                    HousingLoan = preset?.HousingLoan ?? "0",
                    CashAdvance = preset?.CashAdvance ?? "0",
                    CoopLoan = preset?.CoopLoan ?? "0",
                    CoopContribution = preset?.CoopContribution ?? "0",
                    Others = preset?.Others ?? "0",

                    TaxDetails = preset?.TaxDetails ?? "N/A",
                    SSSDetails = preset?.SSSDetails ?? "N/A",
                    PagIbigDetails = preset?.PagIbigDetails ?? "N/A",
                    PhilhealthDetails = preset?.PhilhealthDetails ?? "N/A",
                    SSSLoanDetails = preset?.SSSLoanDetails ?? "N/A",
                    PagIbigLoanDetails = preset?.PagIbigLoanDetails ?? "N/A",
                    CarLoanDetails = preset?.CarLoanDetails ?? "N/A",
                    HousingLoanDetails = preset?.HousingLoanDetails ?? "N/A",
                    CashAdvanceDetails = preset?.CashAdvanceDetails ?? "N/A",
                    CoopLoanDetails = preset?.CoopLoanDetails ?? "N/A",
                    CoopContributionDetails = preset?.CoopContributionDetails ?? "N/A",
                    OthersDetails = preset?.OthersDetails ?? "N/A",

                    VacationLeaveBalance = preset?.VacationLeaveBalance ?? "0",
                    VacationLeaveCredit = preset?.VacationLeaveCredit ?? "0",
                    VacationLeaveDebit = preset?.VacationLeaveDebit ?? "0",
                    SickLeaveBalance = preset?.SickLeaveBalance ?? "0",
                    SickLeaveCredit = preset?.SickLeaveCredit ?? "0",
                    SickLeaveDebit = preset?.SickLeaveDebit ?? "0",

                    Gondola = preset?.Gondola ?? "0",
                    GasAllowance = preset?.GasAllowance ?? "0"
                };
            }
            catch
            {
                return null;
            }
        }


        // helper to extract name from a details JsonElement node
        private static string ExtractNameFromDetailsNode(JsonElement detailsNode)
        {
            try
            {
                if (detailsNode.ValueKind != JsonValueKind.Object) return null;

                string full = null;
                if (detailsNode.TryGetProperty("full_name", out JsonElement fullEl))
                    full = fullEl.GetString();

                if (!string.IsNullOrWhiteSpace(full)) return full.Trim();

                string first = detailsNode.TryGetProperty("first_name", out JsonElement f) ? f.GetString() : "";
                string middle = detailsNode.TryGetProperty("middle_name", out JsonElement m) ? m.GetString() : "";
                string last = detailsNode.TryGetProperty("last_name", out JsonElement l) ? l.GetString() : "";

                string assembled = (first + " " + middle + " " + last).Trim();
                if (!string.IsNullOrWhiteSpace(assembled)) return assembled;

                return null;
            }
            catch { return null; }
        }

        // Recursively extract basic string/number properties into collector
        private static void ExtractAllProperties(JsonElement element, Dictionary<string, string> collector)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String || prop.Value.ValueKind == JsonValueKind.Number)
                        collector[prop.Name] = prop.Value.ToString();
                    else
                        ExtractAllProperties(prop.Value, collector);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                    ExtractAllProperties(item, collector);
            }
        }

        // Excel writer
        private static void CreateMultiSheetWorkbook(string filePath, List<PayrollExportData> payrollList)
        {
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = spreadsheet.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());

                WorkbookStylesPart stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = ConfirmPayrollExportIndividualShared.CreateStylesheet();
                stylesPart.Stylesheet.Save();

                uint sheetId = 1;
                foreach (var data in payrollList)
                {
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    SheetData sheetData = new SheetData();
                    Worksheet worksheet = new Worksheet();
                    Columns columns = ConfirmPayrollExportIndividualShared.CreateColumnStructure();
                    worksheet.Append(columns);
                    worksheet.Append(sheetData);
                    worksheetPart.Worksheet = worksheet;

                    string sheetName = $"{data.EmployeeId} - {data.EmployeeName}";
                    if (sheetName.Length > 31)
                        sheetName = sheetName.Substring(0, 31);

                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = sheetId++,
                        Name = sheetName
                    };
                    sheets.Append(sheet);

                    ConfirmPayrollExportIndividualShared.CreatePayrollSummary(sheetData, data);
                    worksheetPart.Worksheet.Save();
                }

                workbookPart.Workbook.Save();
            }
        }

        public class PayrollPreset
        {
            public string EmployeeId { get; set; }
            public string EmployeeName { get; set; }
            public string Department { get; set; }
            public string Position { get; set; }
            public string DateCovered { get; set; }
            public string Days { get; set; }
            public string DaysPresent { get; set; }
            public string DailyRate { get; set; }
            public string Salary { get; set; }
            public string Overtime { get; set; }

            // Earnings
            public string BasicPay { get; set; }
            public string OvertimePerHour { get; set; }
            public string OvertimePerMinute { get; set; }
            public string Incentives { get; set; }
            public string Commission { get; set; }
            public string FoodAllowance { get; set; }
            public string Communication { get; set; }
            public string GasAllowance { get; set; }
            public string Gondola { get; set; }
            public string GrossPay { get; set; }


            // Deductions
            public string WithholdingTax { get; set; }
            public string SSS { get; set; }
            public string PagIbig { get; set; }
            public string Philhealth { get; set; }
            public string SSSLoan { get; set; }
            public string PagIbigLoan { get; set; }
            public string CarLoan { get; set; }
            public string HousingLoan { get; set; }
            public string CashAdvance { get; set; }
            public string CoopLoan { get; set; }
            public string CoopContribution { get; set; }
            public string Others { get; set; }
            public string TotalDeductions { get; set; }

            // Details
            public string TaxDetails { get; set; }
            public string SSSDetails { get; set; }
            public string PagIbigDetails { get; set; }
            public string PhilhealthDetails { get; set; }
            public string SSSLoanDetails { get; set; }
            public string PagIbigLoanDetails { get; set; }
            public string CarLoanDetails { get; set; }
            public string HousingLoanDetails { get; set; }
            public string CashAdvanceDetails { get; set; }
            public string CoopLoanDetails { get; set; }
            public string CoopContributionDetails { get; set; }
            public string OthersDetails { get; set; }


            // Leave balances
            public string VacationLeaveCredit { get; set; }
            public string VacationLeaveDebit { get; set; }
            public string VacationLeaveBalance { get; set; }
            public string SickLeaveCredit { get; set; }
            public string SickLeaveDebit { get; set; }
            public string SickLeaveBalance { get; set; }

            public string NetPay { get; set; }
        }
    }
}