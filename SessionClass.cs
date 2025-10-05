using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRIS_JAP_ATTPAY
{
    public static class SessionClass
    {
        public static string CurrentUserId { get; set; } // e.g. "101"
        public static string CurrentEmployeeId { get; set; } // e.g. "JAP-002"
        public static string CurrentEmployeeName { get; set; } // e.g. "Charles Andrei Atienza Macaraig"
        public static bool IsAdmin { get; set; }
    }
}
