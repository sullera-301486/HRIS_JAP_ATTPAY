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
    public partial class HRForm : Form
    {
        private string currentUserId;
        private AttributesClassAlt panelLoaderView;
        private AttributesClassAlt panelLoaderMenu;
        private HROverview hrOverview; // Add reference to track the overview

        public HRForm(string userId)
        {
            InitializeComponent();
            currentUserId = userId;

            // Enhanced debugging
            Console.WriteLine($"=== HRForm Initialized ===");
            Console.WriteLine($"User ID: {currentUserId}");
            Console.WriteLine($"Session User ID: {SessionClass.CurrentUserId}");
            Console.WriteLine($"Session Employee ID: {SessionClass.CurrentEmployeeId}");
            Console.WriteLine($"Session Employee Name: {SessionClass.CurrentEmployeeName}");
            Console.WriteLine($"Is Admin: {SessionClass.IsAdmin}");
            Console.WriteLine($"==========================");

            AttributesClass.SetMinSize(this, 1440, 1024);
            this.Size = new Size(1240, 824);
            panelLoaderMenu = new AttributesClassAlt(HRMenuPanel);
            panelLoaderView = new AttributesClassAlt(HRViewPanel);
        }

        private void HRPage_Load(object sender, EventArgs e)
        {
            // Pass currentUserId to both menu and overview
            panelLoaderMenu.LoadUserControl(new HRMenu(HRViewPanel, currentUserId));

            // Create and store reference to overview
            hrOverview = new HROverview(currentUserId);
            panelLoaderView.LoadUserControl(hrOverview);

            if (AttributesScanner.IsScannerConnected())
            {
                AttributesScanner.OnScannerInput += AttributesScanner_OnScannerInput;
                AttributesScanner.StartScannerMonitor();
            }
        }

        private void HRForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AttributesScanner.OnScannerInput -= AttributesScanner_OnScannerInput;
            AttributesScanner.StopScannerMonitor();
        }

        private void AttributesScanner_OnScannerInput(object sender, string data)
        {
            Console.WriteLine("Scanned: " + data);
            // normal scanner behavior; attendance logging, add database logic here
        }

        // Add method to refresh todo list when needed
        public void RefreshTodoList()
        {
            hrOverview?.RefreshTodoList();
        }
    }
}