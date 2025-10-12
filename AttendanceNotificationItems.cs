using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class AttendanceNotificationItems : UserControl
    {
        private DateTime createdAt;
        private Timer refreshTimer;

        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly Dictionary<string, Image> ImageCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public event EventHandler ApproveClicked;
        public event EventHandler DeclineClicked;

        public AttendanceNotificationItems()
        {
            InitializeComponent();
            SetFont();

            this.Load -= AttendanceNotificationItems_Load;
            this.Load += AttendanceNotificationItems_Load;
        }

        private void AttendanceNotificationItems_Load(object sender, EventArgs e)
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 60_000;
            refreshTimer.Tick += (s, ev) => UpdateTimeAgo();
            refreshTimer.Start();
        }

        private void SetFont()
        {
            try
            {
                lblTimeAgo.Font = AttributesClass.GetFont("Roboto-Regular", 9f);
                lblTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                label2.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label3.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label8.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label1.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label11.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label9.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label10.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblSubmittedBy.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblEmployee.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblDate.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblTimeIn.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblTimeOut.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblOvertimeIn.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblOvertimeOut.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                btnApprove.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                btnDecline.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            }
            catch
            {
                // ignore font errors
            }
        }

        public async void SetData(
            string title,
            string submittedId,
            string employeeId,
            DateTime date,
            string timeIn,
            string timeOut,
            string overtimeIn,
            string overtimeOut,
            Image photo = null,
            DateTime? createdAtOverride = null)
        {
            lblTitle.Text = title ?? "Attendance Edit Request";
            lblDate.Text = date == DateTime.MinValue ? "N/A" : date.ToShortDateString();
            lblTimeIn.Text = FormatTimeOnly(timeIn);
            lblTimeOut.Text = FormatTimeOnly(timeOut);
            lblOvertimeIn.Text = FormatTimeOnly(overtimeIn);
            lblOvertimeOut.Text = FormatTimeOnly(overtimeOut);

            await LoadEmployeeDetailsAsync(submittedId, employeeId, photo);

            createdAt = createdAtOverride ?? DateTime.Now;
            UpdateTimeAgo();
        }

        private async Task LoadEmployeeDetailsAsync(string submittedId, string employeeId, Image fallbackPhoto)
        {
            try
            {
                var employees = await firebase.Child("EmployeeDetails").OnceAsync<JObject>();
                string submittedName = null;
                string employeeName = null;
                string submittedImageUrl = null;

                foreach (var emp in employees)
                {
                    var obj = emp.Object;
                    if (obj == null) continue;

                    string empId = obj["employee_id"]?.ToString()?.Trim();
                    string first = obj["first_name"]?.ToString()?.Trim() ?? "";
                    string middle = obj["middle_name"]?.ToString()?.Trim() ?? "";
                    string last = obj["last_name"]?.ToString()?.Trim() ?? "";
                    string imageUrl = obj["image_url"]?.ToString()?.Trim() ?? "";

                    string fullName = $"{first} {middle} {last}".Replace("  ", " ").Trim();
                    string noMiddle = $"{first} {last}".Trim();

                    // 🔹 Match for SubmittedBy (ID or full name)
                    if (empId.Equals(submittedId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(fullName, submittedId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(noMiddle, submittedId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(first, submittedId, StringComparison.OrdinalIgnoreCase))
                    {
                        submittedName = fullName;
                        submittedImageUrl = imageUrl;
                    }

                    // 🔹 Match for Employee
                    if (empId.Equals(employeeId, StringComparison.OrdinalIgnoreCase))
                    {
                        employeeName = fullName;
                    }
                }

                lblSubmittedBy.Text = submittedName ?? submittedId ?? "Unknown";
                lblEmployee.Text = employeeName ?? employeeId ?? "Unknown";

                // 🔹 Load photo if available
                if (!string.IsNullOrEmpty(submittedImageUrl))
                {
                    await LoadImageFromUrlAsync(submittedImageUrl, lblSubmittedBy.Text);
                }

                bool isCurrentUser =
                    !string.IsNullOrWhiteSpace(SessionClass.CurrentEmployeeId) &&
                    (submittedId ?? string.Empty).Equals(SessionClass.CurrentEmployeeId, StringComparison.OrdinalIgnoreCase);

                picEmployee.BackColor = isCurrentUser
                    ? Color.FromArgb(220, 245, 220)
                    : Color.Transparent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ LoadEmployeeDetailsAsync error: " + ex.Message);
                lblSubmittedBy.Text = submittedId ?? "Unknown";
                lblEmployee.Text = employeeId ?? "Unknown";
            }
        }

        private async Task LoadImageFromUrlAsync(string imageUrl, string name)
        {
            try
            {
                if (ImageCache.TryGetValue(imageUrl, out Image cached))
                {
                    picEmployee.Image = cached;
                    picEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                    return;
                }

                using (var resp = await httpClient.GetAsync(imageUrl))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var bytes = await resp.Content.ReadAsByteArrayAsync();
                        using (var ms = new System.IO.MemoryStream(bytes))
                        {
                            Image img = Image.FromStream(ms);
                            ImageCache[imageUrl] = (Image)img.Clone();
                            picEmployee.Image = img;
                            picEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Image load failed: {ex.Message}");
            }
        }

        private string FormatTimeOnly(string datetimeString)
        {
            if (string.IsNullOrWhiteSpace(datetimeString)) return "N/A";

            if (DateTime.TryParse(datetimeString, out DateTime dt))
                return dt.ToString("hh:mm tt").ToLower();

            return datetimeString;
        }

        private void UpdateTimeAgo()
        {
            try
            {
                var diff = DateTime.Now - createdAt;

                if (diff.TotalMinutes < 1)
                    lblTimeAgo.Text = "Just now";
                else if (diff.TotalMinutes < 60)
                    lblTimeAgo.Text = $"{(int)diff.TotalMinutes}m ago";
                else if (diff.TotalHours < 24)
                    lblTimeAgo.Text = $"{(int)diff.TotalHours}h ago";
                else
                    lblTimeAgo.Text = $"{(int)diff.TotalDays}d ago";
            }
            catch
            {
                lblTimeAgo.Text = "Just now";
            }
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            ApproveClicked?.Invoke(this, EventArgs.Empty);
        }

        private void btnDecline_Click(object sender, EventArgs e)
        {
            DeclineClicked?.Invoke(this, EventArgs.Empty);
        }

        // 🧩 Helpers: Avatar generator (you can add your initials image logic here later)
    }
}
