using Firebase.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HRIS_JAP_ATTPAY
{
    public partial class HROverview : UserControl
    {
        private FirebaseClient firebase = new FirebaseClient(
        "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private string currentUserId;
        public HROverview(string userId)
        {
            currentUserId = userId;
            InitializeComponent();
            setFont();
            setActivityLogsDataGridViewAttributes();
            setTodoDataGridViewAttributes();
            setAlertAbsentDataGridViewAttributes();
            setAlertLateDataGridViewAttributes();
            setPanelAttributes();
            setCalendar();
            LoadAttendanceSummary(DateTime.Today);
            PopulateDateComboBox();
            LoadTodoList();
            LoadTodaysAbsentEmployees();
            LoadTodaysLateEmployees();
            comboBoxSelectDate.SelectedIndexChanged += comboBoxSelectDate_SelectedIndexChanged;
        }
        private void setFont()
        {
            labelHROverview.Font = AttributesClass.GetFont("Roboto-Light", 20f);
            labelOverviewDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            labelAttendanceSummary.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            comboBoxSelectDate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            labelTotalNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
            labelOnTimeNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
            labelAbsentNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
            labelLateNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
            labelTotalTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            labelOnTimeTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            labelLateTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            labelAbsentTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            labelTotalDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelTotalDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOnTimeDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOnTimeDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelLateDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelLateDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelAbsentDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelAbsentDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
        }
        private async void PopulateDateComboBox()
        {
            try
            {
                comboBoxSelectDate.Items.Clear();

                // Add special options
                comboBoxSelectDate.Items.Add("Today");
                comboBoxSelectDate.Items.Add("Yesterday");

                // Get ALL unique dates from Firebase attendance data
                var allDates = await GetAllAttendanceDatesAsync();

                if (allDates.Count > 0)
                {
                    // Add all unique dates to the combo box
                    foreach (var date in allDates)
                    {
                        comboBoxSelectDate.Items.Add(date);
                    }
                }
                else
                {
                    // Fallback: Add dates for the last 30 days if no data in Firebase
                    for (int i = 2; i < 30; i++)
                    {
                        comboBoxSelectDate.Items.Add(DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"));
                    }
                }

                // Set default selection
                comboBoxSelectDate.SelectedItem = "Today";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating date combo box: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async Task<List<string>> GetAllAttendanceDatesAsync()
        {
            var uniqueDates = new HashSet<string>();

            try
            {
                // FIX: Load attendance as dictionary instead of JArray
                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();

                if (attendanceRecords != null)
                {
                    foreach (var attendanceRecord in attendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            // Convert dynamic to dictionary for safe access
                            var attendanceDict = attendanceRecord.Object as IDictionary<string, object>;
                            if (attendanceDict != null)
                            {
                                // Extract date from both possible fields
                                string attendanceDateStr = attendanceDict.ContainsKey("attendance_date") ?
                                    attendanceDict["attendance_date"]?.ToString() ?? "" : "";
                                string timeInStr = attendanceDict.ContainsKey("time_in") ?
                                    attendanceDict["time_in"]?.ToString() ?? "" : "";

                                DateTime? recordDate = null;

                                // Try to get date from attendance_date field first
                                if (!string.IsNullOrEmpty(attendanceDateStr) &&
                                    DateTime.TryParse(attendanceDateStr, out DateTime parsedDate))
                                {
                                    recordDate = parsedDate;
                                }
                                // If not available, try to extract from time_in field
                                else if (!string.IsNullOrEmpty(timeInStr) &&
                                         DateTime.TryParse(timeInStr, out DateTime timeInDate))
                                {
                                    recordDate = timeInDate.Date;
                                }

                                if (recordDate.HasValue)
                                {
                                    // Add date in consistent format
                                    uniqueDates.Add(recordDate.Value.ToString("yyyy-MM-dd"));
                                }
                            }
                        }
                    }
                }

                // Sort dates in descending order (most recent first)
                return uniqueDates.OrderByDescending(d => DateTime.Parse(d)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving attendance dates: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<string>();
            }
        }
        private async void LoadAttendanceSummary(DateTime date)
        {
            try
            {
                // Show loading state
                labelTotalNumber.Text = "";
                labelOnTimeNumber.Text = "0";
                labelLateNumber.Text = "0";
                labelAbsentNumber.Text = "0";

                // Load ALL employee data and filter out archived employees
                var allEmployees = await GetAllActiveEmployeesAsync();
                int totalEmployees = allEmployees.Count;

                // If no active employees found, show zeros
                if (totalEmployees == 0)
                {
                    labelTotalNumber.Text = "0";
                    labelOnTimeNumber.Text = "0";
                    labelLateNumber.Text = "0";
                    labelAbsentNumber.Text = "0";
                    labelAlertDateRange.Text = $"Date: {date.ToString("yyyy-MM-dd")}";
                    return;
                }

                // Initialize counters
                int onTimeCount = 0;
                int lateCount = 0;
                int absentCount = 0;

                // FIX: Load attendance as dictionary instead of JArray
                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();

                // Create a set of active employees who attended on the selected date
                var attendedEmployees = new HashSet<string>();
                string targetDate = date.ToString("yyyy-MM-dd");

                if (attendanceRecords != null)
                {
                    foreach (var attendanceRecord in attendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            // Convert dynamic to dictionary for safe access
                            var attendanceDict = attendanceRecord.Object as IDictionary<string, object>;
                            if (attendanceDict != null)
                            {
                                // Extract values safely
                                string attendanceDateStr = attendanceDict.ContainsKey("attendance_date") ?
                                    attendanceDict["attendance_date"]?.ToString() ?? "" : "";
                                string timeInStr = attendanceDict.ContainsKey("time_in") ?
                                    attendanceDict["time_in"]?.ToString() ?? "" : "";
                                string status = attendanceDict.ContainsKey("status") ?
                                    attendanceDict["status"]?.ToString() ?? "Absent" : "Absent";
                                string employeeId = attendanceDict.ContainsKey("employee_id") ?
                                    attendanceDict["employee_id"]?.ToString() ?? "" : "";

                                // Check if this record matches the selected date
                                bool matchesDate = false;

                                // Check attendance_date first
                                if (!string.IsNullOrEmpty(attendanceDateStr) && attendanceDateStr == targetDate)
                                {
                                    matchesDate = true;
                                }

                                // Check time_in date if not already matched
                                if (!matchesDate && !string.IsNullOrEmpty(timeInStr) &&
                                    DateTime.TryParse(timeInStr, out DateTime timeInDate))
                                {
                                    matchesDate = timeInDate.ToString("yyyy-MM-dd") == targetDate;
                                }

                                // Only count if employee is active (not archived) and matches date
                                if (matchesDate && !string.IsNullOrEmpty(employeeId) && allEmployees.Contains(employeeId))
                                {
                                    attendedEmployees.Add(employeeId);

                                    switch (status.ToLower())
                                    {
                                        case "on time":
                                            onTimeCount++;
                                            break;
                                        case "late":
                                        case "early out":
                                        case "late & early out":
                                            lateCount++;
                                            break;
                                        default:
                                            // Employee attended but with different status
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                absentCount = totalEmployees - attendedEmployees.Count;

                // Update UI with counts
                labelTotalNumber.Text = totalEmployees.ToString();
                labelOnTimeNumber.Text = onTimeCount.ToString();
                labelLateNumber.Text = lateCount.ToString();
                labelAbsentNumber.Text = absentCount.ToString();

                // Update the date range label
                labelAlertDateRange.Text = $"Date: {targetDate}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance summary: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Reset UI on error
                labelTotalNumber.Text = "0";
                labelOnTimeNumber.Text = "0";
                labelLateNumber.Text = "0";
                labelAbsentNumber.Text = "0";
            }
        }

        // NEW METHOD TO GET ALL ACTIVE EMPLOYEES (EXCLUDING ARCHIVED ONES)
        private async Task<HashSet<string>> GetAllActiveEmployeesAsync()
        {
            var activeEmployees = new HashSet<string>();

            try
            {
                // Load current employees
                var currentEmployees = await firebase.Child("EmployeeDetails").OnceAsync<JObject>();
                if (currentEmployees != null)
                {
                    foreach (var employee in currentEmployees)
                    {
                        activeEmployees.Add(employee.Key);
                    }
                }

                // Load archived employees and remove them from active list
                var archivedEmployees = await firebase.Child("ArchivedEmployees").OnceAsync<JObject>();
                if (archivedEmployees != null)
                {
                    foreach (var archivedEmployee in archivedEmployees)
                    {
                        activeEmployees.Remove(archivedEmployee.Key);
                    }
                }

                return activeEmployees;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading active employees: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return activeEmployees;
            }
        }

        // UPDATED ALERT METHODS
        private async void LoadTodaysAbsentEmployees()
        {
            try
            {
                dataGridViewAlertAbsent.Rows.Clear();
                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var activeEmployees = await GetAllActiveEmployeesAsync();

                // FIX: Get attendance as dictionary
                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var attendedToday = new HashSet<string>();

                if (attendanceRecords != null)
                {
                    foreach (var attendanceRecord in attendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            var attendanceDict = attendanceRecord.Object as IDictionary<string, object>;
                            if (attendanceDict != null)
                            {
                                string attendanceDateStr = attendanceDict.ContainsKey("attendance_date") ?
                                    attendanceDict["attendance_date"]?.ToString() ?? "" : "";
                                string timeInStr = attendanceDict.ContainsKey("time_in") ?
                                    attendanceDict["time_in"]?.ToString() ?? "" : "";
                                string employeeId = attendanceDict.ContainsKey("employee_id") ?
                                    attendanceDict["employee_id"]?.ToString() ?? "" : "";

                                bool matchesDate = false;
                                if (!string.IsNullOrEmpty(attendanceDateStr) && attendanceDateStr == today)
                                {
                                    matchesDate = true;
                                }
                                else if (!matchesDate && !string.IsNullOrEmpty(timeInStr) &&
                                         DateTime.TryParse(timeInStr, out DateTime timeInDate))
                                {
                                    matchesDate = timeInDate.ToString("yyyy-MM-dd") == today;
                                }

                                if (matchesDate && !string.IsNullOrEmpty(employeeId))
                                {
                                    attendedToday.Add(employeeId);
                                }
                            }
                        }
                    }
                }

                // Find absent employees (active employees who didn't attend today)
                var absentEmployees = activeEmployees.Where(empId => !attendedToday.Contains(empId)).ToList();

                // Load employee names and count absences
                var employeeDetails = await firebase.Child("EmployeeDetails").OnceAsync<JObject>();
                var absenceCounts = new Dictionary<string, int>();

                foreach (var empId in absentEmployees)
                {
                    if (employeeDetails.Any(e => e.Key == empId))
                    {
                        var empData = employeeDetails.First(e => e.Key == empId).Object;
                        string firstName = empData["first_name"]?.ToString() ?? "";
                        string lastName = empData["last_name"]?.ToString() ?? "";
                        string fullName = $"{firstName} {lastName}".Trim();

                        if (absenceCounts.ContainsKey(fullName))
                        {
                            absenceCounts[fullName]++;
                        }
                        else
                        {
                            absenceCounts[fullName] = 1;
                        }
                    }
                }

                // Add to DataGridView
                foreach (var entry in absenceCounts)
                {
                    dataGridViewAlertAbsent.Rows.Add(entry.Key, entry.Value);
                }

                // If no absent employees, show message
                if (dataGridViewAlertAbsent.Rows.Count == 0)
                {
                    dataGridViewAlertAbsent.Rows.Add("No absent employees today", "0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading absent employees: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridViewAlertAbsent.Rows.Add("Error loading data", "0");
            }
        }

        private async void LoadTodaysLateEmployees()
        {
            try
            {
                dataGridViewAlertLate.Rows.Clear();
                var today = DateTime.Today.ToString("yyyy-MM-dd");

                // FIX: Get attendance as dictionary
                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var lateEmployees = new Dictionary<string, int>();

                if (attendanceRecords != null)
                {
                    foreach (var attendanceRecord in attendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            var attendanceDict = attendanceRecord.Object as IDictionary<string, object>;
                            if (attendanceDict != null)
                            {
                                string attendanceDateStr = attendanceDict.ContainsKey("attendance_date") ?
                                    attendanceDict["attendance_date"]?.ToString() ?? "" : "";
                                string timeInStr = attendanceDict.ContainsKey("time_in") ?
                                    attendanceDict["time_in"]?.ToString() ?? "" : "";
                                string status = attendanceDict.ContainsKey("status") ?
                                    attendanceDict["status"]?.ToString() ?? "" : "";
                                string employeeId = attendanceDict.ContainsKey("employee_id") ?
                                    attendanceDict["employee_id"]?.ToString() ?? "" : "";

                                bool matchesDate = false;
                                if (!string.IsNullOrEmpty(attendanceDateStr) && attendanceDateStr == today)
                                {
                                    matchesDate = true;
                                }
                                else if (!matchesDate && !string.IsNullOrEmpty(timeInStr) &&
                                         DateTime.TryParse(timeInStr, out DateTime timeInDate))
                                {
                                    matchesDate = timeInDate.ToString("yyyy-MM-dd") == today;
                                }

                                if (matchesDate && !string.IsNullOrEmpty(employeeId) &&
                                    (status.ToLower() == "late" || status.ToLower() == "early out" || status.ToLower() == "late & early out"))
                                {
                                    if (lateEmployees.ContainsKey(employeeId))
                                    {
                                        lateEmployees[employeeId]++;
                                    }
                                    else
                                    {
                                        lateEmployees[employeeId] = 1;
                                    }
                                }
                            }
                        }
                    }
                }

                // Load employee names
                var employeeDetails = await firebase.Child("EmployeeDetails").OnceAsync<JObject>();
                var lateEmployeeCounts = new Dictionary<string, int>();

                foreach (var entry in lateEmployees)
                {
                    string empId = entry.Key;
                    int count = entry.Value;

                    if (employeeDetails.Any(e => e.Key == empId))
                    {
                        var empData = employeeDetails.First(e => e.Key == empId).Object;
                        string firstName = empData["first_name"]?.ToString() ?? "";
                        string lastName = empData["last_name"]?.ToString() ?? "";
                        string fullName = $"{firstName} {lastName}".Trim();

                        lateEmployeeCounts[fullName] = count;
                    }
                }

                // Add to DataGridView
                foreach (var entry in lateEmployeeCounts)
                {
                    dataGridViewAlertLate.Rows.Add(entry.Key, entry.Value);
                }

                // If no late employees, show message
                if (dataGridViewAlertLate.Rows.Count == 0)
                {
                    dataGridViewAlertLate.Rows.Add("No late/early out employees today", "0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading late employees: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridViewAlertLate.Rows.Add("Error loading data", "0");
            }
        }


        private void setPanelAttributes()
        {
            panelCalendar.Paint += panelCalendar_Paint;
            panelAttendanceSummary.Paint += panelAttendanceSummary_Paint;
        }

        private void setCalendar()
        {
            Calendar myCalendar = new Calendar();
            int borderPadding = 1;

            myCalendar.Location = new Point(borderPadding, borderPadding);
            myCalendar.Size = new Size(
                panelCalendar.ClientSize.Width - borderPadding * 2,
                panelCalendar.ClientSize.Height - borderPadding * 2
            );

            // make it resize along with the panel
            myCalendar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            panelCalendar.Controls.Clear();
            panelCalendar.Controls.Add(myCalendar);
        }

        private void panelCalendar_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelCalendar.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelAttendanceSummary_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelAttendanceSummary.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelAttendanceSummary_Resize(object sender, EventArgs e)
        {
            panelAttendanceSummary.Invalidate();
        }

        private void setActivityLogsDataGridViewAttributes()
        {
            dataGridViewActivityLogs.GridColor = Color.White;
            dataGridViewActivityLogs.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewActivityLogs.ColumnHeadersHeight = 40;
            dataGridViewActivityLogs.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewActivityLogs.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            for (int i = 1; i < 30; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewActivityLogs.Rows.Add("6 - " + i + " - 25", "10:30 PM", "Manual Entry Added", "Added attendance for Franz Louies Deloritos.");
            }
        }

        private void panelActivityLog_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelActivityLog.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelActivityLog_Resize(object sender, EventArgs e)
        {
            panelActivityLog.Invalidate();
        }

        private void setTodoDataGridViewAttributes()
        {
            dataGridViewTodo.GridColor = Color.White;
            dataGridViewTodo.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewTodo.ColumnHeadersHeight = 40;
            dataGridViewTodo.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewTodo.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            dataGridViewTodo.CellMouseEnter += dataGridViewTodo_CellMouseEnter;
            dataGridViewTodo.CellMouseLeave += dataGridViewTodo_CellMouseLeave;
            dataGridViewTodo.CellClick += dataGridViewTodo_CellClick;

            // Clear existing columns and add the correct ones
            dataGridViewTodo.Columns.Clear();

            // Task column
            dataGridViewTodo.Columns.Add("ColumnTask", "Task");
            dataGridViewTodo.Columns["ColumnTask"].Width = 250;
            dataGridViewTodo.Columns["ColumnTask"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Due Date column
            dataGridViewTodo.Columns.Add("ColumnDueDate", "Due Date");
            dataGridViewTodo.Columns["ColumnDueDate"].Width = 150;

            // Delete button column
            DataGridViewImageColumn deleteColumn = new DataGridViewImageColumn();
            deleteColumn.Name = "ColumnTrash";
            deleteColumn.HeaderText = "";
            deleteColumn.Width = 40;
            deleteColumn.Image = Properties.Resources.TrashBin;
            dataGridViewTodo.Columns.Add(deleteColumn);
        }

        private void setAlertAbsentDataGridViewAttributes()
        {
            dataGridViewAlertAbsent.GridColor = Color.White;
            dataGridViewAlertAbsent.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAlertAbsent.ColumnHeadersHeight = 40;
            dataGridViewAlertAbsent.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAlertAbsent.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            dataGridViewAlertAbsent.Rows.Clear();
        }

        private void setAlertLateDataGridViewAttributes()
        {
            dataGridViewAlertLate.GridColor = Color.White;
            dataGridViewAlertLate.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAlertLate.ColumnHeadersHeight = 40;
            dataGridViewAlertLate.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAlertLate.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            // REMOVE TEST DATA - data will be loaded from Firebase
            dataGridViewAlertLate.Rows.Clear();
        }


        private void panelTodo_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelTodo.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelTodo_Resize(object sender, EventArgs e)
        {
            panelTodo.Invalidate();
        }

        private void panelAlert_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelAlert.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelAlert_Resize(object sender, EventArgs e)
        {
            panelAlert.Invalidate();
        }

        private void dataGridViewTodo_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewTodo.Columns[e.ColumnIndex].Name == "ColumnTrash")
                dataGridViewTodo.Cursor = Cursors.Hand;
        }

        private void dataGridViewTodo_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewTodo.Cursor = Cursors.Default;
        }

        private void dataGridViewTodo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewTodo.Columns[e.ColumnIndex].Name == "ColumnTrash")
            {
                if (todoItemKeys.ContainsKey(e.RowIndex))
                {
                    string taskKey = todoItemKeys[e.RowIndex];
                    string taskText = dataGridViewTodo.Rows[e.RowIndex].Cells["ColumnTask"].Value?.ToString() ?? "";

                    Form parentForm = this.FindForm();
                    ConfirmDeleteTask confirmDeleteTaskForm = new ConfirmDeleteTask(taskKey, taskText, LoadTodoList);
                    AttributesClass.ShowWithOverlay(parentForm, confirmDeleteTaskForm);
                }
            }
        }


        private void labelTodoAdd_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            AddNewTask addNewTaskForm = new AddNewTask(currentUserId);
            addNewTaskForm.TaskAdded += (s, args) =>
            {
                LoadTodoList();
            };
            AttributesClass.ShowWithOverlay(parentForm, addNewTaskForm);
        }

        public void RefreshTodoList()
        {
            LoadTodoList();
        }

        private Dictionary<int, string> todoItemKeys = new Dictionary<int, string>(); // Add this at class level

        private async void LoadTodoList()
        {
            try
            {
                dataGridViewTodo.Rows.Clear();
                todoItemKeys.Clear(); // Clear previous mappings

                // Load todo items from Firebase
                var todoItems = await firebase
                    .Child("Todos")
                    .OnceAsync<Dictionary<string, object>>();

                if (todoItems != null && todoItems.Any())
                {
                    int rowIndex = 0; // Track row index for mapping
                    foreach (var todoItem in todoItems)
                    {
                        var todoData = todoItem.Object;

                        if (todoData != null && todoData.ContainsKey("task"))
                        {
                            string task = todoData["task"]?.ToString() ?? "";
                            string dueDate = todoData.ContainsKey("dueDate") ? todoData["dueDate"].ToString() : "";
                            string assignedTo = todoData.ContainsKey("assignedTo") ? todoData["assignedTo"].ToString() : "";

                            // Only show tasks assigned to current user or general tasks
                            if (string.IsNullOrEmpty(assignedTo) || assignedTo == currentUserId || assignedTo == "all")
                            {
                                // Format date if possible
                                string formattedDate = dueDate;
                                if (DateTime.TryParse(dueDate, out DateTime parsedDate))
                                {
                                    formattedDate = parsedDate.ToString("MM-dd-yyyy");
                                }
                                else if (string.IsNullOrEmpty(dueDate))
                                {
                                    formattedDate = "No due date";
                                }

                                // Add to DataGridView
                                dataGridViewTodo.Rows.Add(task, formattedDate);

                                // Store the Firebase key for this row
                                todoItemKeys[rowIndex] = todoItem.Key;
                                rowIndex++;
                            }
                        }
                    }
                }

                // If no tasks found, show a message
                if (dataGridViewTodo.Rows.Count == 0)
                {
                    dataGridViewTodo.Rows.Add("No tasks found", "");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading todo list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridViewTodo.Rows.Add("Error loading tasks", "");
            }
        }
        private void comboBoxSelectDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSelectDate.SelectedItem == null)
                return;

            string selectedText = comboBoxSelectDate.SelectedItem.ToString();
            DateTime selectedDate;

            if (selectedText == "Today")
            {
                selectedDate = DateTime.Today;
            }
            else if (selectedText == "Yesterday")
            {
                selectedDate = DateTime.Today.AddDays(-1);
            }
            else if (DateTime.TryParseExact(selectedText, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime parsedDate))
            {
                selectedDate = parsedDate;
            }
            else
            {
                selectedDate = DateTime.Today;
            }

            // Load the attendance summary for the selected date
            LoadAttendanceSummary(selectedDate);
        }
    }
}