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
using System.Text.RegularExpressions;

namespace HRIS_JAP_ATTPAY
{
    public partial class HRAttendance : UserControl
    {
        // 🔹 Firebase client
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        private bool isLoading = false;

        public HRAttendance()
        {
            InitializeComponent();

            // Remove any duplicate event handler attachments
            comboBoxSelectDate.SelectedIndexChanged -= comboBoxSelectDate_SelectedIndexChanged;
            textBoxSearchEmployee.TextChanged -= textBoxSearchEmployee_TextChanged;

            comboBoxSelectDate.SelectedIndexChanged += comboBoxSelectDate_SelectedIndexChanged;
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;

            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();

            // Load data only once
            LoadFirebaseAttendanceData();
            PopulateDateComboBox();
        }

        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBoxSearchEmployee.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                foreach (DataGridViewRow row in dataGridViewAttendance.Rows)
                {
                    if (!row.IsNewRow)
                        row.Visible = true;
                }
                return;
            }

            foreach (DataGridViewRow row in dataGridViewAttendance.Rows)
            {
                if (!row.IsNewRow & searchText != "find employee")
                {
                    string employeeId = row.Cells["EmployeeId"].Value?.ToString()?.ToLower() ?? "";
                    string fullName = row.Cells["FullName"].Value?.ToString()?.ToLower() ?? "";
                    string status = row.Cells["Status"].Value?.ToString()?.ToLower() ?? "";

                    bool isMatch = employeeId.Contains(searchText) ||
                                  fullName.Contains(searchText) ||
                                  status.Contains(searchText);

                    row.Visible = isMatch;
                }
            }
        }

        private void labelAddLeave_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LeaveRequest leaveRequestForm = new LeaveRequest();
            AttributesClass.ShowWithOverlay(parentForm, leaveRequestForm);
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                switch (e.Value.ToString())
                {
                    case "On Time":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(95, 218, 71);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(95, 218, 71);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Late":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(255, 163, 74);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(255, 163, 74);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Early Out":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(255, 163, 74);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(255, 163, 74);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Absent":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(221, 60, 60);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(221, 60, 60);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Leave":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(71, 93, 218);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(71, 93, 218);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Day Off":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(180, 174, 189);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(180, 174, 189);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                }
            }
        }

        private void setDataGridViewAttributes()
        {
            dataGridViewAttendance.ReadOnly = true;
            dataGridViewAttendance.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewAttendance.MultiSelect = false;
            dataGridViewAttendance.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewAttendance.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewAttendance.DefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewAttendance.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewAttendance.GridColor = Color.White;
            dataGridViewAttendance.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAttendance.ColumnHeadersHeight = 40;
            dataGridViewAttendance.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAttendance.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            dataGridViewAttendance.CellFormatting += dataGridView1_CellFormatting;
            dataGridViewAttendance.CellMouseEnter += dataGridViewAttendance_CellMouseEnter;
            dataGridViewAttendance.CellMouseLeave += dataGridViewAttendance_CellMouseLeave;
            dataGridViewAttendance.CellClick += dataGridViewAttendance_CellClick;

            // 🔹 Define columns (HoursWorked + OvertimeHours → OvertimeIn + OvertimeOut)
            dataGridViewAttendance.Columns.Clear();
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "RowNumber", HeaderText = "", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 25 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeIn", HeaderText = "Time In", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeOut", HeaderText = "Time Out", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "OvertimeIn", HeaderText = "Overtime In", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "OvertimeOut", HeaderText = "Overtime Out", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "VerificationMethod", HeaderText = "Verification", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewImageColumn
            {
                Name = "Action",
                HeaderText = "",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25,
                Image = Properties.Resources.VerticalThreeDots
            });
        }

        private void setFont()
        {
            labelHREmployee.Font = AttributesClass.GetFont("Roboto-Light", 20f);
            labelAttendanceDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            labelAddLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
            textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
            comboBoxSelectDate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
        }

        private void pictureBoxFilters_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminAttendance filterAdminAttendanceForm = new FilterAdminAttendance();
            AttributesClass.ShowWithOverlay(parentForm, filterAdminAttendanceForm);
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxSearchEmployee, "Find Employee");
        }

        private void dataGridViewAttendance_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Action")
                dataGridViewAttendance.Cursor = Cursors.Hand;
        }

        private void dataGridViewAttendance_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewAttendance.Cursor = Cursors.Default;
        }

        private void dataGridViewAttendance_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Action")
            {
                Form parentForm = this.FindForm();
                EditAttendance editAttendanceForm = new EditAttendance();
                AttributesClass.ShowWithOverlay(parentForm, editAttendanceForm);
            }
        }

        private void comboBoxSelectDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = comboBoxSelectDate.SelectedItem?.ToString();
            DateTime? selectedDate = null;

            if (!string.IsNullOrEmpty(selectedText) && selectedText != "All Dates")
            {
                if (DateTime.TryParseExact(selectedText, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsedDate))
                {
                    selectedDate = parsedDate;
                }
            }

            LoadFirebaseAttendanceData(selectedDate);
        }

        private string FormatFirebaseTime(string dateTimeStr)
        {
            if (string.IsNullOrEmpty(dateTimeStr) || dateTimeStr == "N/A")
                return "N/A";

            try
            {
                if (DateTime.TryParse(dateTimeStr, out DateTime dt))
                    return dt.ToString("h:mm tt");

                return dateTimeStr;
            }
            catch
            {
                return dateTimeStr;
            }
        }

        private async void PopulateDateComboBox()
        {
            try
            {
                comboBoxSelectDate.Items.Clear();
                comboBoxSelectDate.Items.Add("All Dates");

                var attendanceArray = await firebase.Child("Attendance").OnceSingleAsync<JArray>();
                if (attendanceArray != null)
                {
                    var uniqueDates = new HashSet<string>();

                    foreach (var attendanceItem in attendanceArray)
                    {
                        if (attendanceItem != null && attendanceItem.Type != JTokenType.Null)
                        {
                            string dateStr = attendanceItem["attendance_date"]?.ToString();
                            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime date))
                            {
                                uniqueDates.Add(date.ToString("yyyy-MM-dd"));
                            }
                        }
                    }

                    var sortedDates = uniqueDates.Select(d => DateTime.Parse(d))
                                                .OrderByDescending(d => d)
                                                .Select(d => d.ToString("yyyy-MM-dd"))
                                                .ToList();

                    foreach (var date in sortedDates)
                    {
                        comboBoxSelectDate.Items.Add(date);
                    }
                }

                comboBoxSelectDate.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error populating date combo box: " + ex.Message);
            }
        }

        private async void LoadFirebaseAttendanceData(DateTime? selectedDate = null)
        {
            if (isLoading) return;

            isLoading = true;

            try
            {
                dataGridViewAttendance.Rows.Clear();
                dataGridViewAttendance.Refresh();

                Cursor.Current = Cursors.WaitCursor;

                var firebaseEmployees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                var employeeDict = new Dictionary<string, dynamic>();
                foreach (var emp in firebaseEmployees)
                    employeeDict[emp.Key] = emp.Object;

                try
                {
                    var attendanceData = await firebase.Child("Attendance").OnceSingleAsync<JArray>();

                    if (attendanceData == null || attendanceData.Count == 0)
                    {
                        MessageBox.Show("No attendance records found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    int counter = 1;
                    foreach (var attendanceItem in attendanceData)
                    {
                        if (attendanceItem != null && attendanceItem.Type != JTokenType.Null)
                        {
                            bool recordAdded = ProcessAttendanceRecord(attendanceItem, employeeDict, selectedDate, counter);
                            if (recordAdded)
                            {
                                counter++;
                            }
                        }
                    }

                    if (selectedDate.HasValue)
                    {
                        labelAttendanceDate.Text = $"Attendance for {selectedDate.Value:yyyy-MM-dd}";
                    }
                    else
                    {
                        labelAttendanceDate.Text = "All Attendance Records";
                    }
                }
                catch (FirebaseException fex)
                {
                    if (fex.ResponseData.Contains("404") || fex.ResponseData.Contains("null"))
                    {
                        MessageBox.Show("Attendance database is empty or doesn't exist yet.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Firebase error: {fex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading attendance data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private bool ProcessAttendanceRecord(JToken attendance, Dictionary<string, dynamic> employeeDict, DateTime? selectedDate, int counter)
        {
            try
            {
                string timeInStr = attendance["time_in"]?.ToString() ?? "";
                string timeOutStr = attendance["time_out"]?.ToString() ?? "";
                string overtimeInStr = attendance["overtime_in"]?.ToString() ?? "";
                string overtimeOutStr = attendance["overtime_out"]?.ToString() ?? "";
                string attendanceDateStr = attendance["attendance_date"]?.ToString() ?? "";

                if (selectedDate.HasValue)
                {
                    bool shouldInclude = false;

                    if (!string.IsNullOrEmpty(attendanceDateStr) && DateTime.TryParse(attendanceDateStr, out DateTime attendanceDate))
                    {
                        if (attendanceDate.Date == selectedDate.Value.Date)
                        {
                            shouldInclude = true;
                        }
                    }

                    if (!shouldInclude && !string.IsNullOrEmpty(timeInStr) && DateTime.TryParse(timeInStr, out DateTime timeInDate))
                    {
                        if (timeInDate.Date == selectedDate.Value.Date)
                        {
                            shouldInclude = true;
                        }
                    }

                    if (!shouldInclude && !string.IsNullOrEmpty(timeOutStr) && DateTime.TryParse(timeOutStr, out DateTime timeOutDate))
                    {
                        if (timeOutDate.Date == selectedDate.Value.Date)
                        {
                            shouldInclude = true;
                        }
                    }

                    if (!shouldInclude)
                    {
                        return false;
                    }
                }

                string employeeId = attendance["employee_id"]?.ToString() ?? "N/A";
                string timeIn = FormatFirebaseTime(timeInStr);
                string timeOut = FormatFirebaseTime(timeOutStr);
                string overtimeIn = FormatFirebaseTime(overtimeInStr);
                string overtimeOut = FormatFirebaseTime(overtimeOutStr);
                string status = attendance["status"]?.ToString() ?? "Absent";
                string verification = attendance["verification_method"]?.ToString() ?? "Manual";

                string fullName = "N/A";
                if (employeeDict.ContainsKey(employeeId))
                {
                    var employee = employeeDict[employeeId];
                    fullName = $"{employee.first_name} {employee.middle_name} {employee.last_name}".Trim();
                }

                if (dataGridViewAttendance.InvokeRequired)
                {
                    dataGridViewAttendance.Invoke((MethodInvoker)delegate
                    {
                        dataGridViewAttendance.Rows.Add(
                            counter,
                            employeeId,
                            fullName,
                            timeIn,
                            timeOut,
                            status,
                            overtimeIn,
                            overtimeOut,
                            verification,
                            Properties.Resources.VerticalThreeDots
                        );
                    });
                }
                else
                {
                    dataGridViewAttendance.Rows.Add(
                        counter,
                        employeeId,
                        fullName,
                        timeIn,
                        timeOut,
                        status,
                        overtimeIn,
                        overtimeOut,
                        verification,
                        Properties.Resources.VerticalThreeDots
                    );
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing attendance record: {ex.Message}");
                return false;
            }
        }
    }
}
