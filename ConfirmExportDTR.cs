using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmExportDTR : Form
    {
        private DataGridView dataGridView;
        private string exportDateRange;

        public ConfirmExportDTR()
        {
            InitializeComponent();
            setFont();
        }

        // Method to pass the DataGridView reference and date range from AdminAttendance
        public void SetExportData(DataGridView dgv, string dateRange)
        {
            this.dataGridView = dgv;
            this.exportDateRange = dateRange;
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView == null || dataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("No attendance data to export.", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show save file dialog
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                    saveFileDialog.FileName = $"DTR_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    saveFileDialog.Title = "Export DTR to Excel";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Create the Excel file
                        CreateExcelFile(saveFileDialog.FileName);
                        
                        MessageBox.Show("DTR exported successfully!", "Export Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting DTR: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void CreateExcelFile(string filePath)
        {
            // Create the Excel document
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                // Add WorkbookPart
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // Add WorksheetPart
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet();

                // Add Sheets to the Workbook
                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());

                // Append a new worksheet
                Sheet sheet = new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "DTR"
                };
                sheets.Append(sheet);

                // Add StyleSheet for formatting
                WorkbookStylesPart stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = CreateStylesheet();
                stylesPart.Stylesheet.Save();

                // Get the worksheet's data
                SheetData sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                // Add title row
                Row titleRow = new Row() { RowIndex = 1 };
                sheetData.Append(titleRow);
                AppendTextCell("A1", "Daily Time Record (DTR)", titleRow, 2); // Style index 2 for title

                // Add date range row
                Row dateRow = new Row() { RowIndex = 2 };
                sheetData.Append(dateRow);
                AppendTextCell("A2", exportDateRange ?? "All Records", dateRow, 1);

                // Add empty row
                Row emptyRow = new Row() { RowIndex = 3 };
                sheetData.Append(emptyRow);

                // Add header row
                Row headerRow = new Row() { RowIndex = 4 };
                sheetData.Append(headerRow);

                // Create headers
                AppendTextCell("A4", "Employee ID", headerRow, 1); // Style index 1 for header
                AppendTextCell("B4", "Name", headerRow, 1);
                AppendTextCell("C4", "Date", headerRow, 1);
                AppendTextCell("D4", "Time In", headerRow, 1);
                AppendTextCell("E4", "Time Out", headerRow, 1);
                AppendTextCell("F4", "Hours Worked", headerRow, 1);
                AppendTextCell("G4", "Status", headerRow, 1);
                AppendTextCell("H4", "Overtime Hours", headerRow, 1);
                AppendTextCell("I4", "Verification", headerRow, 1);

                // Add data rows
                uint rowIndex = 5;
                foreach (DataGridViewRow dgvRow in dataGridView.Rows)
                {
                    if (dgvRow.IsNewRow) continue;

                    Row dataRow = new Row() { RowIndex = rowIndex };
                    sheetData.Append(dataRow);

                    // Get cell values
                    string employeeId = dgvRow.Cells["EmployeeId"]?.Value?.ToString() ?? "";
                    string fullName = dgvRow.Cells["FullName"]?.Value?.ToString() ?? "";
                    string attendanceDate = dgvRow.Cells["AttendanceDate"]?.Value?.ToString() ?? "";
                    string timeIn = dgvRow.Cells["TimeIn"]?.Value?.ToString() ?? "";
                    string timeOut = dgvRow.Cells["TimeOut"]?.Value?.ToString() ?? "";
                    string hoursWorked = dgvRow.Cells["HoursWorked"]?.Value?.ToString() ?? "";
                    string status = dgvRow.Cells["Status"]?.Value?.ToString() ?? "";
                    string overtimeHours = dgvRow.Cells["OvertimeHours"]?.Value?.ToString() ?? "";
                    string verification = dgvRow.Cells["VerificationMethod"]?.Value?.ToString() ?? "";

                    // Format date
                    if (DateTime.TryParse(attendanceDate, out DateTime parsedDate))
                    {
                        attendanceDate = parsedDate.ToString("yyyy-MM-dd");
                    }

                    // Append cells
                    string columnA = GetColumnName(1) + rowIndex;
                    string columnB = GetColumnName(2) + rowIndex;
                    string columnC = GetColumnName(3) + rowIndex;
                    string columnD = GetColumnName(4) + rowIndex;
                    string columnE = GetColumnName(5) + rowIndex;
                    string columnF = GetColumnName(6) + rowIndex;
                    string columnG = GetColumnName(7) + rowIndex;
                    string columnH = GetColumnName(8) + rowIndex;
                    string columnI = GetColumnName(9) + rowIndex;

                    AppendTextCell(columnA, employeeId, dataRow, 0);
                    AppendTextCell(columnB, fullName, dataRow, 0);
                    AppendTextCell(columnC, attendanceDate, dataRow, 0);
                    AppendTextCell(columnD, timeIn, dataRow, 0);
                    AppendTextCell(columnE, timeOut, dataRow, 0);
                    AppendTextCell(columnF, hoursWorked, dataRow, 0);
                    AppendTextCell(columnG, status, dataRow, 0);
                    AppendTextCell(columnH, overtimeHours, dataRow, 0);
                    AppendTextCell(columnI, verification, dataRow, 0);

                    rowIndex++;
                }

                // Add column width settings
                Columns columns = new Columns();
                columns.Append(new Column() { Min = 1, Max = 1, Width = 30, CustomWidth = true }); // Employee ID
                columns.Append(new Column() { Min = 2, Max = 2, Width = 30, CustomWidth = true }); // Name
                columns.Append(new Column() { Min = 3, Max = 3, Width = 15, CustomWidth = true }); // Date
                columns.Append(new Column() { Min = 4, Max = 4, Width = 15, CustomWidth = true }); // Time In
                columns.Append(new Column() { Min = 5, Max = 5, Width = 15, CustomWidth = true }); // Time Out
                columns.Append(new Column() { Min = 6, Max = 6, Width = 15, CustomWidth = true }); // Hours Worked
                columns.Append(new Column() { Min = 7, Max = 7, Width = 15, CustomWidth = true }); // Status
                columns.Append(new Column() { Min = 8, Max = 8, Width = 15, CustomWidth = true }); // Overtime Hours
                columns.Append(new Column() { Min = 9, Max = 9, Width = 30, CustomWidth = true }); // Verification

                worksheetPart.Worksheet.InsertAt(columns, 0);

                // Mark worksheet as read-only
                SheetProtection sheetProtection = new SheetProtection()
                {
                    Sheet = true,
                    Objects = true,
                    Scenarios = true
                };
                worksheetPart.Worksheet.InsertAfter(sheetProtection, worksheetPart.Worksheet.Elements<SheetData>().First());

                worksheetPart.Worksheet.Save();
                workbookPart.Workbook.Save();
            }
        }

        private Stylesheet CreateStylesheet()
        {
            Stylesheet stylesheet = new Stylesheet();

            // Fonts
            Fonts fonts = new Fonts() { Count = 2 };

            // Index 0: Normal font
            fonts.Append(new DocumentFormat.OpenXml.Spreadsheet.Font()
            {
                FontSize = new FontSize() { Val = 11 },
                FontName = new FontName() { Val = "Calibri" }
            });

            // Index 1: Bold font for headers
            fonts.Append(new DocumentFormat.OpenXml.Spreadsheet.Font()
            {
                Bold = new Bold(),
                FontSize = new FontSize() { Val = 11 },
                FontName = new FontName() { Val = "Calibri" }
            });

            stylesheet.Append(fonts);

            // Fills
            Fills fills = new Fills() { Count = 2 };

            // Index 0: No fill
            fills.Append(new Fill()
            {
                PatternFill = new PatternFill() { PatternType = PatternValues.None }
            });

            // Index 1: Gray fill
            fills.Append(new Fill()
            {
                PatternFill = new PatternFill()
                {
                    PatternType = PatternValues.Solid,
                    ForegroundColor = new ForegroundColor() { Rgb = "FFD3D3D3" }
                }
            });

            stylesheet.Append(fills);

            // Borders
            Borders borders = new Borders() { Count = 1 };
            borders.Append(new Border()
            {
                LeftBorder = new LeftBorder(),
                RightBorder = new RightBorder(),
                TopBorder = new TopBorder(),
                BottomBorder = new BottomBorder(),
                DiagonalBorder = new DiagonalBorder()
            });

            stylesheet.Append(borders);

            // Cell formats
            CellFormats cellFormats = new CellFormats() { Count = 3 };

            // Index 0: Normal style
            cellFormats.Append(new CellFormat()
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0
            });

            // Index 1: Header style
            cellFormats.Append(new CellFormat()
            {
                FontId = 1,
                FillId = 1,
                BorderId = 0
            });

            // Index 2: Title style
            cellFormats.Append(new CellFormat()
            {
                FontId = 1,
                FillId = 0,
                BorderId = 0
            });

            stylesheet.Append(cellFormats);

            return stylesheet;
        }

        private void AppendTextCell(string cellReference, string cellValue, Row row, uint styleIndex)
        {
            Cell cell = new Cell()
            {
                CellReference = cellReference,
                DataType = CellValues.InlineString,
                StyleIndex = styleIndex
            };

            InlineString inlineString = new InlineString();
            Text text = new Text() { Text = cellValue };
            inlineString.AppendChild(text);
            cell.AppendChild(inlineString);

            row.AppendChild(cell);
        }

        private string GetColumnName(int columnNumber)
        {
            string columnName = "";

            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }
    }
}