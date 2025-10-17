using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public class EmployeeSchedule
    {
        public string schedule_id { get; set; }
        public string employee_id { get; set; }
        public string day_of_week { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string schedule_type { get; set; }
    }

    public class Employee
    {
        public string FullName { get; set; }
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmploymentStatus { get; set; }
        public string RFIDTag { get; set; }
        public string employee_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string middle_name { get; set; }
        public string full_name { get; set; }
        public string rfid_tag { get; set; }
        public string email { get; set; }
        public string contact { get; set; }
        public string department { get; set; }
        public string address { get; set; }
        public string date_of_birth { get; set; }
        public string gender { get; set; }
        public string marital_status { get; set; }
        public string nationality { get; set; }
        public string image_url { get; set; }
        public string created_at { get; set; }
    }
}