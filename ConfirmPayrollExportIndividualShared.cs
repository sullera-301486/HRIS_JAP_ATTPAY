using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Reflection;

namespace HRIS_JAP_ATTPAY
{
    public static class ConfirmPayrollExportIndividualShared
    {
        // Cache the form instance and method info to avoid repeated reflection
        private static ConfirmPayrollExportIndividual _cachedForm = null;
        private static MethodInfo _stylesheetMethod = null;
        private static MethodInfo _buildContentMethod = null;

        private static void EnsureFormInitialized()
        {
            if (_cachedForm == null)
            {
                _cachedForm = new ConfirmPayrollExportIndividual(null);
                var formType = _cachedForm.GetType();

                // Look for CreateSimpleStylesheet method
                _stylesheetMethod = formType.GetMethod("CreateSimpleStylesheet",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                // Look for BuildPayrollContent method (NOT CreatePayrollSummary!)
                _buildContentMethod = formType.GetMethod("BuildPayrollContent",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (_stylesheetMethod == null)
                {
                    throw new Exception("CreateSimpleStylesheet method not found in ConfirmPayrollExportIndividual");
                }

                if (_buildContentMethod == null)
                {
                    throw new Exception("BuildPayrollContent method not found in ConfirmPayrollExportIndividual");
                }
            }
        }

        /// <summary>
        /// Creates the stylesheet by calling CreateSimpleStylesheet from ConfirmPayrollExportIndividual
        /// </summary>
        public static Stylesheet CreateStylesheet()
        {
            try
            {
                EnsureFormInitialized();
                return _stylesheetMethod.Invoke(_cachedForm, null) as Stylesheet;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating stylesheet: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates the column structure with predefined widths
        /// </summary>
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

        /// <summary>
        /// Creates the payroll summary by calling BuildPayrollContent from ConfirmPayrollExportIndividual
        /// </summary>
        public static void CreatePayrollSummary(SheetData sheetData, PayrollExportData data)
        {
            try
            {
                EnsureFormInitialized();
                _buildContentMethod.Invoke(_cachedForm, new object[] { sheetData, data });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating payroll summary: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }
    }
}