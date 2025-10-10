using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Reflection;

namespace HRIS_JAP_ATTPAY
{
    public static class ConfirmPayrollExportIndividualShared
    {
        // Reuse CreateStylesheet() from ConfirmPayrollExportIndividual
        public static Stylesheet CreateStylesheet()
        {
            var form = new ConfirmPayrollExportIndividual(null);
            return form.GetType()
                .GetMethod("CreateStylesheet", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(form, null) as Stylesheet;
        }

        // Same column widths as your individual export
        public static Columns CreateColumnStructure()
        {
            Columns columns = new Columns();
            columns.Append(new Column() { Min = 1, Max = 1, Width = 25, CustomWidth = true }); // Column A
            columns.Append(new Column() { Min = 2, Max = 2, Width = 17, CustomWidth = true }); // Column B
            columns.Append(new Column() { Min = 3, Max = 3, Width = 35, CustomWidth = true }); // Column C
            columns.Append(new Column() { Min = 4, Max = 4, Width = 19, CustomWidth = true }); // Column D
            columns.Append(new Column() { Min = 5, Max = 5, Width = 27, CustomWidth = true }); // Column E
            columns.Append(new Column() { Min = 6, Max = 6, Width = 16, CustomWidth = true }); // Column F
            columns.Append(new Column() { Min = 7, Max = 7, Width = 16, CustomWidth = true }); // Column G
            columns.Append(new Column() { Min = 8, Max = 8, Width = 9, CustomWidth = true });  // Column H
            columns.Append(new Column() { Min = 9, Max = 9, Width = 10, CustomWidth = true }); // Column I
            return columns;
        }

        // Reuse CreatePayrollSummary() for each employee sheet
        public static void CreatePayrollSummary(SheetData sheetData, PayrollExportData data)
        {
            var form = new ConfirmPayrollExportIndividual(null);
            form.GetType()
                .GetMethod("CreatePayrollSummary", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(form, new object[] { sheetData, data });
        }
    }
}
