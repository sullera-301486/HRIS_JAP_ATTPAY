using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminForm : Form
    {
        private AttributesClassAlt panelLoaderView;
        private AttributesClassAlt panelLoaderMenu;
        private string currentUserId;
        private string currentEmployeeId;
        private string payrollPeriod;
        private AdminOverview adminOverview; // Add reference to track the overview

        public AdminForm(string userId, string employeeId, string period = null)
        {
            InitializeComponent();
            currentUserId = userId;
            currentEmployeeId = employeeId;
            payrollPeriod = period;

            // Debug output to verify user context
            Console.WriteLine($"=== AdminForm Initialized ===");
            Console.WriteLine($"User ID: {currentUserId}");
            Console.WriteLine($"Employee ID: {currentEmployeeId}");
            Console.WriteLine($"Payroll Period: {payrollPeriod}");
            Console.WriteLine($"Session User ID: {SessionClass.CurrentUserId}");
            Console.WriteLine($"Session Employee ID: {SessionClass.CurrentEmployeeId}");
            Console.WriteLine($"==============================");

            AttributesClass.SetMinSize(this, 1440, 1024);
            this.Size = new Size(1240, 824);

            panelLoaderMenu = new AttributesClassAlt(AdminMenuPanel);
            panelLoaderView = new AttributesClassAlt(AdminViewPanel);
        }

        private void AdminPage_Load(object sender, EventArgs e)
        {
            // Pass currentUserId to both menu and overview
            panelLoaderMenu.LoadUserControl(new AdminMenu(AdminViewPanel, currentUserId, currentEmployeeId));

            // Create and store reference to overview
            adminOverview = new AdminOverview(currentUserId);
            panelLoaderView.LoadUserControl(adminOverview);

            if (AttributesScanner.IsScannerConnected())
            {
                AttributesScanner.OnScannerInput += AttributesScanner_OnScannerInput;
                AttributesScanner.StartScannerMonitor();
            }
        }

        private void AdminForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AttributesScanner.OnScannerInput -= AttributesScanner_OnScannerInput;
            AttributesScanner.StopScannerMonitor();
        }

        private void AttributesScanner_OnScannerInput(object sender, string data)
        {
            if (ScanRFID.ActiveInstance != null)
            {
                // special handling for ScanRFID
                ScanRFID.ActiveInstance.HandleScannedDataInstance(data);
            }
            else
            {
                Console.WriteLine("Scanned: " + data);
                // normal scanner behavior; attendance logging, add database logic here
            }
        }

        // Add method to refresh todo list when needed
        public void RefreshTodoList()
        {
            adminOverview?.RefreshTodoList();
        }
    }
}