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

namespace HRIS_JAP_ATTPAY
{
    public static class PayrollExportAllData
    {
        private static readonly FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        // Main entry
        public static async Task GenerateAllPayrollsAsync(string savePath)
        {
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

                string summary = $"Export complete.{Environment.NewLine}{Environment.NewLine}Exported: {payrollList.Count} employees";
                MessageBox.Show(summary, "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting payroll: " + ex.Message, "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Build one employee payroll record; uses global EmployeeDetails map as final lookup
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
                // flatten properties for easy access
                Dictionary<string, string> props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                ExtractAllProperties(empEl, props);

                // resolve employee id (fallback is property name e.g., "JAP-001")
                string empId = idFallback;
                if (props.ContainsKey("employee_id")) empId = props["employee_id"];
                if (props.ContainsKey("employment_id") && string.IsNullOrEmpty(empId)) empId = props["employment_id"];
                if (string.IsNullOrWhiteSpace(empId))
                {
                    skippedNoId++;
                    return null;
                }

                // skip archived by id
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

                // 2) If still null, check flattened props (maybe first_name directly present)
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

                // 3) FINAL FIX: look up in the top-level EmployeeDetails map by empId
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

                // 4) fallback
                if (string.IsNullOrWhiteSpace(name))
                {
                    processedNoName++;
                    name = "(No Name)";
                }

                string dept = props.ContainsKey("department") ? props["department"] : "";
                string pos = props.ContainsKey("position") ? props["position"] : "";

                return new PayrollExportData
                {
                    EmployeeId = empId,
                    EmployeeName = name,
                    Department = dept,
                    Position = pos,
                    DateCovered = string.Format("{0:MMM 1} - {0:MMM dd, yyyy}", DateTime.Now),
                    Days = "0",
                    DaysPresent = "0",
                    DailyRate = "500.00",
                    Salary = "15000.00",
                    Overtime = "0.00",
                    BasicPay = "15000",
                    OvertimePerHour = "0.00",
                    OvertimePerMinute = "0.00",
                    Incentives = "1000",
                    Commission = "500",
                    FoodAllowance = "300",
                    Communication = "200",
                    GasAllowance = "0",
                    Gondola = "0",
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
                    OthersDetails = "N/A"
                }; //not all is final data; fix this by calling actual values from database
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
    }
}
