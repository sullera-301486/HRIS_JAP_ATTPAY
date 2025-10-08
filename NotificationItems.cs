using System;
using System.Collections.Generic;
using System.Drawing;

namespace HRIS_JAP_ATTPAY
{
    public static class NotificationStore
    {
        public static List<LeaveRequestData> LeaveRequests { get; } = new List<LeaveRequestData>();
        public static List<AttendanceRequestData> AttendanceRequests { get; } = new List<AttendanceRequestData>();
    }

    public class LeaveRequestData
    {
        public string Title { get; set; }
        public string SubmittedBy { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Notes { get; set; }
        public Image Photo { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AttendanceRequestData
    {
        public string Title { get; set; }
        public string SubmittedBy { get; set; }
        public string Employee { get; set; }
        public DateTime Date { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string OTStart { get; set; }
        public string OTEnd { get; set; }
        public Image Photo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
