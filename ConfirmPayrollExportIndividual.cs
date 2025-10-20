using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmPayrollExportIndividual : Form
    {
        private PayrollExportData _payrollData;

        public ConfirmPayrollExportIndividual(PayrollExportData payrollData)
        {
            InitializeComponent();
            _payrollData = payrollData;
            SetFont();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"Payroll_{_payrollData.EmployeeId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    bool fileCreated = GenerateProtectedExcelFile(saveFileDialog.FileName, _payrollData);

                    if (!fileCreated)
                    {
                        MessageBox.Show("Failed to create the payroll file. Please check if the file is open in another program.",
                            "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Set file attributes to read-only
                    try
                    {
                        File.SetAttributes(saveFileDialog.FileName, FileAttributes.ReadOnly);
                    }
                    catch (Exception attrEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Could not set read-only attribute: {attrEx.Message}");
                    }

                    // Log the export action to Firebase
                    bool logSuccess = await LogPayrollExportToFirebase(_payrollData.EmployeeName, _payrollData.EmployeeId);

                    if (logSuccess)
                    {
                        MessageBox.Show("Payroll exported successfully as read-only and logged to system!", "Export Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Payroll exported successfully as read-only, but failed to log to system.", "Export Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting payroll: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool GenerateProtectedExcelFile(string filePath, PayrollExportData data)
        {
            try
            {
                // Delete existing file if it exists
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (IOException ioEx)
                    {
                        MessageBox.Show($"Cannot overwrite file. It may be open in another program.\n\nDetails: {ioEx.Message}",
                            "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                // Create a new Excel package
                using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    // Add workbook part
                    WorkbookPart workbookPart = spreadsheet.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    // Add stylesheet
                    WorkbookStylesPart stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                    stylesPart.Stylesheet = CreateSimpleStylesheet();
                    stylesPart.Stylesheet.Save();

                    // Add worksheet part
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    SheetData sheetData = new SheetData();

                    // Create worksheet with proper structure
                    Worksheet worksheet = new Worksheet();

                    // Add columns first
                    Columns columns = new Columns(
                        new Column() { Min = 1, Max = 1, Width = 25, CustomWidth = true },
                        new Column() { Min = 2, Max = 2, Width = 17, CustomWidth = true },
                        new Column() { Min = 3, Max = 3, Width = 35, CustomWidth = true },
                        new Column() { Min = 4, Max = 4, Width = 19, CustomWidth = true },
                        new Column() { Min = 5, Max = 5, Width = 27, CustomWidth = true },
                        new Column() { Min = 6, Max = 6, Width = 16, CustomWidth = true },
                        new Column() { Min = 7, Max = 7, Width = 16, CustomWidth = true },
                        new Column() { Min = 8, Max = 8, Width = 9, CustomWidth = true },
                        new Column() { Min = 9, Max = 9, Width = 10, CustomWidth = true }
                    );

                    // Append elements in correct order
                    worksheet.Append(columns);
                    worksheet.Append(sheetData);

                    // Add worksheet protection (this goes after the main content)
                    SheetProtection sheetProtection = new SheetProtection()
                    {
                        Sheet = true,
                        Objects = true,
                        Scenarios = true
                    };
                    worksheet.Append(sheetProtection);

                    worksheetPart.Worksheet = worksheet;

                    // Add sheets to workbook
                    Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Payroll Summary"
                    };
                    sheets.Append(sheet);

                    // Add workbook protection
                    WorkbookProtection workbookProtection = new WorkbookProtection()
                    {
                        LockStructure = true,
                        LockWindows = true
                    };
                    spreadsheet.WorkbookPart.Workbook.WorkbookProtection = workbookProtection;

                    // Build the payroll content
                    BuildPayrollContent(sheetData, data);

                    // Save everything
                    worksheetPart.Worksheet.Save();
                    workbookPart.Workbook.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Excel file: {ex.Message}", "File Creation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private Stylesheet CreateSimpleStylesheet()
        {
            Stylesheet stylesheet = new Stylesheet();

            // Fonts
            Fonts fonts = new Fonts();
            fonts.Append(new Font()); // Default font (index 0)
            fonts.Append(new Font(new Bold())); // Bold font (index 1)

            // Fills
            Fills fills = new Fills();
            fills.Append(new Fill(new PatternFill() { PatternType = PatternValues.None }));
            fills.Append(new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }));
            fills.Append(new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "D3D3D3" } })
            { PatternType = PatternValues.Solid }));

            // Borders
            Borders borders = new Borders();
            borders.Append(new Border()); // No border (index 0)
            borders.Append(new Border( // All borders (index 1)
                new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                new DiagonalBorder()));

            // Cell formats
            CellFormats cellFormats = new CellFormats();
            cellFormats.Append(new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 }); // Default (index 0)
            cellFormats.Append(new CellFormat() { FontId = 1, FillId = 0, BorderId = 0 }); // Bold (index 1)
            cellFormats.Append(new CellFormat() { FontId = 1, FillId = 2, BorderId = 1 }); // Bold with gray background and border (index 2)
            cellFormats.Append(new CellFormat() { FontId = 0, FillId = 0, BorderId = 1 }); // Normal with border (index 3)

            stylesheet.Append(fonts);
            stylesheet.Append(fills);
            stylesheet.Append(borders);
            stylesheet.Append(cellFormats);

            return stylesheet;
        }

        private void BuildPayrollContent(SheetData sheetData, PayrollExportData data)
        {
            uint rowIndex = 1;

            // Title row
            AddMergedRow(sheetData, rowIndex++, new string[] { "Payroll Summary" }, 1, 9, 1);

            // Employee info
            AddRow(sheetData, rowIndex++, new string[] {
                "", "Name/ID", $"{data.EmployeeName}/{data.EmployeeId}",
                "Department/Position", $"{data.Department}/{data.Position}",
                "Salary/Daily Rate", $"{data.Salary}/{data.DailyRate}", "", ""
            }, 1);

            // Date covered
            AddRow(sheetData, rowIndex++, new string[] {
                $"Payroll for {DateTime.Now:MMMM d, yyyy}", "Date Covered/Days", $"{data.DateCovered}/{data.Days}",
                "Days Present", data.DaysPresent, "Overtime", data.Overtime, "", ""
            }, 1);

            rowIndex++; // Empty row

            // Table header
            AddRow(sheetData, rowIndex++, new string[] {
                "", "Pay & Allowances", "Amounts", "Credit", "Amounts",
                "Debit", "Amount", "Details", ""
            }, 2);

            // Data rows
            AddRow(sheetData, rowIndex++, new string[] {
                "", "Basic Pay", FormatCurrency(data.BasicPay),
                data.DaysPresent, FormatCurrency(data.BasicPay),
                "W/Tax", FormatCurrency(data.WithholdingTax), data.TaxDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "Overtime/hr", FormatCurrency(data.OvertimePerHour),
                data.Overtime, FormatCurrency(data.OvertimePerHour),
                "SSS", FormatCurrency(data.SSS), data.SSSDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "Overtime/min", FormatCurrency(data.OvertimePerMinute),
                "", FormatCurrency(data.OvertimePerMinute),
                "PAG-IBIG", FormatCurrency(data.PagIbig), data.PagIbigDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "Incentives", FormatCurrency(data.Incentives),
                "", FormatCurrency(data.Incentives),
                "PHILHEALTH", FormatCurrency(data.Philhealth), data.PhilhealthDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "Commission", FormatCurrency(data.Commission),
                "", FormatCurrency(data.Commission),
                "SSS Loan", FormatCurrency(data.SSSLoan), data.SSSLoanDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "Food Allowance", FormatCurrency(data.FoodAllowance),
                "", FormatCurrency(data.FoodAllowance),
                "PAG-IBIG Loan", FormatCurrency(data.PagIbigLoan), data.PagIbigLoanDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "Communication", FormatCurrency(data.Communication),
                "", FormatCurrency(data.Communication),
                "Car Loan", FormatCurrency(data.CarLoan), data.CarLoanDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "Gas Allowance", FormatCurrency(data.GasAllowance),
                "", FormatCurrency(data.GasAllowance),
                "Housing Loan", FormatCurrency(data.HousingLoan), data.HousingLoanDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "Gondola", FormatCurrency(data.Gondola),
                "", FormatCurrency(data.Gondola),
                "Cash Advance", FormatCurrency(data.CashAdvance), data.CashAdvanceDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "", "", "", "",
                "COOP Loan", FormatCurrency(data.CoopLoan), data.CoopLoanDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "", "", "", "",
                "COOP Contri", FormatCurrency(data.CoopContribution), data.CoopContributionDetails, ""
            }, 0);

            AddRow(sheetData, rowIndex++, new string[] {
                "", "", "", "", "",
                "Others", FormatCurrency(data.Others), data.OthersDetails, ""
            }, 0);

            rowIndex++; // Empty row

            // Summary row
            AddRow(sheetData, rowIndex++, new string[] {
                "", "", "", "Gross Pay", FormatCurrency(data.GrossPay),
                "Deductions", FormatCurrency(data.TotalDeductions),
                "Net Pay", FormatCurrency(data.NetPay)
            }, 1);

            rowIndex++; // Empty row

            // Leave section
            AddRow(sheetData, rowIndex++, new string[] { "Leave", "Credit", "Debit", "Balance", "", "", "", "", "" }, 1);
            AddRow(sheetData, rowIndex++, new string[] { "Vacation Leave", data.VacationLeaveCredit, data.VacationLeaveDebit, data.VacationLeaveBalance, "", "", "", "", "" }, 0);
            AddRow(sheetData, rowIndex++, new string[] { "Sick Leave", data.SickLeaveCredit, data.SickLeaveDebit, data.SickLeaveBalance, "", "", "", "", "" }, 0);
        }

        private void AddRow(SheetData sheetData, uint rowIndex, string[] values, uint styleIndex)
        {
            Row row = new Row() { RowIndex = rowIndex };

            for (int i = 0; i < values.Length; i++)
            {
                Cell cell = new Cell()
                {
                    CellReference = GetColumnName(i) + rowIndex,
                    DataType = CellValues.String,
                    CellValue = new CellValue(values[i] ?? ""),
                    StyleIndex = styleIndex
                };
                row.Append(cell);
            }

            sheetData.Append(row);
        }

        private void AddMergedRow(SheetData sheetData, uint rowIndex, string[] values, int startCol, int endCol, uint styleIndex)
        {
            Row row = new Row() { RowIndex = rowIndex };

            for (int i = 0; i < values.Length; i++)
            {
                Cell cell = new Cell()
                {
                    CellReference = GetColumnName(i) + rowIndex,
                    DataType = CellValues.String,
                    CellValue = new CellValue(values[i] ?? ""),
                    StyleIndex = styleIndex
                };
                row.Append(cell);
            }

            sheetData.Append(row);
        }

        private string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (index < letters.Length)
                return letters[index].ToString();
            return "A";
        }

        private string FormatCurrency(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "0")
                return "P0.00";

            if (value.StartsWith("P"))
                return value;

            return $"P{value}";
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetFont()
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

        private async Task<bool> LogPayrollExportToFirebase(string employeeName, string employeeId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string firebaseUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/";

                    var logEntry = new
                    {
                        date = DateTime.Now.ToString("yyyy-MM-dd"),
                        time = DateTime.Now.ToString("hh:mm tt").ToUpper(),
                        action = "Export individual payroll",
                        details = $"Exported {employeeName}"
                    };

                    string payrollLogsUrl = $"{firebaseUrl}PayrollLogs.json";
                    string jsonData = JsonConvert.SerializeObject(logEntry);
                    var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(payrollLogsUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine("Firebase log created successfully!");
                        return true;
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Firebase log failed: {response.StatusCode} - {errorContent}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase logging error: {ex.Message}");
                return false;
            }
        }
    }
}