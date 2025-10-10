using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

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
                    GenerateExcelFile(saveFileDialog.FileName, _payrollData);

                    // Log the export action to Firebase - this will automatically create the table
                    bool logSuccess = await LogPayrollExportToFirebase(_payrollData.EmployeeName, _payrollData.EmployeeId);

                    if (logSuccess)
                    {
                        MessageBox.Show("Payroll exported successfully and logged to system!", "Export Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Payroll exported successfully, but failed to log to system.", "Export Complete",
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

        private async Task<bool> LogPayrollExportToFirebase(string employeeName, string employeeId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Firebase database URL - CORRECTED to match AdminOverview
                    string firebaseUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/";

                    // Create log entry with the four required values
                    var logEntry = new
                    {
                        date = DateTime.Now.ToString("yyyy-MM-dd"),
                        time = DateTime.Now.ToString("hh:mm tt").ToUpper(),
                        action = "Export individual payroll",
                        details = $"Exported {employeeName}"
                    };

                    // Post to PayrollLogs table - Firebase automatically creates it if it doesn't exist
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

        private void GenerateExcelFile(string filePath, PayrollExportData data)
        {
            using (var spreadsheet = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                // Create workbook parts
                WorkbookPart workbookPart = spreadsheet.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

                // Create worksheet with sheet data
                SheetData sheetData = new SheetData();
                Worksheet worksheet = new Worksheet();
                worksheet.Append(sheetData);

                // Add column configurations
                Columns columns = new Columns();
                // Set widths for columns A-I (index 1-9)
                columns.Append(new Column() { Min = 1, Max = 1, Width = 25, CustomWidth = true }); // Column A
                columns.Append(new Column() { Min = 2, Max = 2, Width = 17, CustomWidth = true }); // Column B
                columns.Append(new Column() { Min = 3, Max = 3, Width = 35, CustomWidth = true }); // Column C
                columns.Append(new Column() { Min = 4, Max = 4, Width = 19, CustomWidth = true }); // Column D
                columns.Append(new Column() { Min = 5, Max = 5, Width = 27, CustomWidth = true }); // Column E
                columns.Append(new Column() { Min = 6, Max = 6, Width = 16, CustomWidth = true }); // Column F
                columns.Append(new Column() { Min = 7, Max = 7, Width = 16, CustomWidth = true }); // Column G
                columns.Append(new Column() { Min = 8, Max = 8, Width = 9, CustomWidth = true });  // Column H
                columns.Append(new Column() { Min = 9, Max = 9, Width = 10, CustomWidth = true }); // Column I

                worksheet.InsertBefore(columns, sheetData);
                worksheetPart.Worksheet = worksheet;

                // Add styles
                WorkbookStylesPart stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = CreateStylesheet();
                stylesPart.Stylesheet.Save();

                // Add sheets to workbook
                Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet()
                {
                    Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Payroll Summary"
                };
                sheets.Append(sheet);

                // Create the payroll summary
                CreatePayrollSummary(sheetData, data);

                worksheetPart.Worksheet.Save();
            }
        }

        private Stylesheet CreateStylesheet()
        {
            return new Stylesheet(
                new Fonts(
                    new Font( // Index 0 - default font
                        new FontSize() { Val = 11 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new Font( // Index 1 - bold font
                        new Bold(),
                        new FontSize() { Val = 11 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" })
                ),
                new Fills(
                    new Fill(new PatternFill() { PatternType = PatternValues.None }), // Index 0
                    new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }), // Index 1
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue { Value = "D3D3D3" } })
                    { PatternType = PatternValues.Solid }) // Index 2 - light gray
                ),
                new Borders(
                    new Border(), // Index 0 - no border
                    new Border( // Index 1 - all borders
                        new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder())
                ),
                new CellFormats(
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 }, // Index 0 - default
                    new CellFormat() { FontId = 1, FillId = 0, BorderId = 1, ApplyFont = true }, // Index 1 - bold with border
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 1, NumberFormatId = 4 }, // Index 2 - number with border
                    new CellFormat() { FontId = 1, FillId = 2, BorderId = 1, ApplyFont = true } // Index 3 - bold with gray background
                )
            );
        }

        private void CreatePayrollSummary(SheetData sheetData, PayrollExportData data)
        {
            // Title row
            sheetData.AppendChild(CreateRow(new string[] { "Payroll Summary" }, 1, 8));

            // Employee info section
            sheetData.AppendChild(CreateRow(new string[] { "", "Name/ID", $"{data.EmployeeName}/{data.EmployeeId}",
                "Department/Position", $"{data.Department}/{data.Position}", "Salary/Daily Rate", $"{data.Salary}/{data.DailyRate}" }, 1));

            // Date covered section
            sheetData.AppendChild(CreateRow(new string[] { $"Payroll for {DateTime.Now:MMMM d, yyyy}", "Date Covered/Days",
                $"{data.DateCovered}/{data.Days}", "Days Present", data.DaysPresent, "Overtime", data.Overtime }, 1));

            // Empty row
            sheetData.AppendChild(CreateRow(new string[] { "" }));

            // Table header
            sheetData.AppendChild(CreateRow(new string[] { "", "Pay & Allowances", "Amounts", "Credit", "Amounts",
                "Debit", "Amount", "Details", "" }, 3));

            // Basic Pay row
            sheetData.AppendChild(CreateRow(new string[] { "", "Basic Pay", $"P{data.BasicPay}",
                data.DaysPresent, $"P{data.BasicPay}", "W/Tax", $"P{data.WithholdingTax}", $"{data.TaxDetails}", "" }));

            // Overtime rows
            sheetData.AppendChild(CreateRow(new string[] { "", "Overtime/hr", $"P{data.OvertimePerHour}",
                data.Overtime, $"P{data.OvertimePerHour}", "SSS", $"P{data.SSS}", $"{data.SSSDetails}" }));

            sheetData.AppendChild(CreateRow(new string[] { "", "Overtime/min", $"P{data.OvertimePerMinute}",
                "", $"P{data.OvertimePerMinute}", "PAG-IBIG", $"P{data.PagIbig}", $"{data.PagIbigDetails}" }));

            // Incentives row
            sheetData.AppendChild(CreateRow(new string[] { "", "Incentives", $"P{data.Incentives}",
                "", $"P{data.Incentives}", "PHILHEALTH", $"P{data.Philhealth}", $"{data.PhilhealthDetails}" }));

            // Commission row
            sheetData.AppendChild(CreateRow(new string[] { "", "Commission", $"P{data.Commission}",
                "", $"P{data.Commission}", "SSS Loan", $"P{data.SSSLoan}", $"{data.SSSLoanDetails}" }));

            // Food Allowance row
            sheetData.AppendChild(CreateRow(new string[] { "", "Food Allowance", $"P{data.FoodAllowance}",
                "", $"P{data.FoodAllowance}", "PAG-IBIG Loan", $"P{data.PagIbigLoan}", $"{data.PagIbigLoanDetails}" }));

            // Communication row
            sheetData.AppendChild(CreateRow(new string[] { "" , "Communication", $"P{data.Communication}",
                "", $"P{data.Communication}", "Car Loan", $"P{data.CarLoan}", $"{data.CarLoanDetails}"}));

            // Gas Allowance row
            sheetData.AppendChild(CreateRow(new string[] { "", "Gas Allowance", $"P{data.GasAllowance}",
                "", $"P{data.GasAllowance}", "Housing Loan", $"P{data.HousingLoan}", $"{data.HousingLoanDetails}"}));

            // Gondola row
            sheetData.AppendChild(CreateRow(new string[] { "", "Gondola", $"P{data.Gondola}",
                "", $"P{data.Gondola}", "Cash Advance", $"P{data.CashAdvance}", $"{data.CashAdvanceDetails}" }));

            // Coop Loan row
            sheetData.AppendChild(CreateRow(new string[] { "", "", "",
                "", "", "COOP Loan", $"P{data.CoopLoan}", $"{data.CoopLoanDetails}" }));

            // Coop Contri row
            sheetData.AppendChild(CreateRow(new string[] { "", "", "",
                "", "", "COOP Contri", $"P{data.CoopContribution}", $"{data.CoopContributionDetails}" }));

            // Others row
            sheetData.AppendChild(CreateRow(new string[] { "", "", "",
                "", "", "Others", $"P{data.Others}", $"{data.OthersDetails}" }));

            // Empty row
            sheetData.AppendChild(CreateRow(new string[] { "" }));

            // Gross Pay and Deductions summary
            sheetData.AppendChild(CreateRow(new string[] { "", "", "", "Gross Pay", $"P{data.GrossPay}",
                "Deductions", $"P{data.TotalDeductions}", "Net Pay", $"P{data.NetPay}" }, 1));

            // Empty row
            sheetData.AppendChild(CreateRow(new string[] { "" }));

            // Leave section header
            sheetData.AppendChild(CreateRow(new string[] { "Leave", "Credit", "Debit", "Balance" }, 1));

            // Vacation Leave
            sheetData.AppendChild(CreateRow(new string[] { "Vacation Leave", data.VacationLeaveCredit,
                data.VacationLeaveDebit, data.VacationLeaveBalance }));

            // Sick Leave
            sheetData.AppendChild(CreateRow(new string[] { "Sick Leave", data.SickLeaveCredit,
                data.SickLeaveDebit, data.SickLeaveBalance }));
        }

        private Row CreateRow(string[] cellValues, uint styleIndex = 0, int mergeCount = 1)
        {
            Row row = new Row();

            for (int i = 0; i < cellValues.Length; i++)
            {
                Cell cell = new Cell()
                {
                    CellValue = new CellValue(cellValues[i]),
                    DataType = CellValues.String,
                    StyleIndex = styleIndex
                };

                // Merge cells for title if needed
                if (i == 0 && mergeCount > 1)
                {
                    // You can add cell merging logic here if needed
                }

                row.AppendChild(cell);
            }

            // Add empty cells if needed to complete the row structure
            for (int i = cellValues.Length; i < 9; i++)
            {
                row.AppendChild(new Cell() { CellValue = new CellValue(""), DataType = CellValues.String, StyleIndex = styleIndex });
            }

            return row;
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
    }
}