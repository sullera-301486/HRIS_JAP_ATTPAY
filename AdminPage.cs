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
        private AdminOverview adminOverview;
        private RFIDAttendanceHandler rfidHandler;

        // Add this flag to manage scanning mode
        private bool isScanRFIDMode = false;

        public AdminForm(string userId, string employeeId, string period = null)
        {
            InitializeComponent();
            currentUserId = userId;
            currentEmployeeId = employeeId;
            payrollPeriod = period;

            Console.WriteLine($"=== AdminForm Initialized ===");
            Console.WriteLine($"User ID: {currentUserId}");
            Console.WriteLine($"Employee ID: {currentEmployeeId}");
            Console.WriteLine($"Payroll Period: {payrollPeriod}");
            Console.WriteLine($"==============================");

            AttributesClass.SetMinSize(this, 1440, 1024);
            this.Size = new Size(1240, 824);

            panelLoaderMenu = new AttributesClassAlt(AdminMenuPanel);
            panelLoaderView = new AttributesClassAlt(AdminViewPanel);

            InitializeRFIDHandler();
        }

        private void InitializeRFIDHandler()
        {
            rfidHandler = new RFIDAttendanceHandler();

            // Subscribe to events for real-time updates
            rfidHandler.OnScanHistoryUpdate += (message) =>
            {
                // Only update if not in ScanRFID mode
                if (!isScanRFIDMode)
                {
                    Console.WriteLine($"RFID: {message}");
                    UpdateScanHistoryUI(message);
                }
            };

            rfidHandler.OnEmployeeInfoUpdate += (name, id, department, status, scanTime, overtime) =>
            {
                // Only update if not in ScanRFID mode
                if (!isScanRFIDMode)
                {
                    Console.WriteLine($"Employee Scan: {name} - {status} at {scanTime}");
                    // This will trigger the event in RFIDAttendanceHandler but won't update UI in ScanRFID mode
                }
            };
        }

        private async void AdminPage_Load(object sender, EventArgs e)
        {
            panelLoaderMenu.LoadUserControl(new AdminMenu(AdminViewPanel, currentUserId, currentEmployeeId));
            adminOverview = new AdminOverview(currentUserId);
            panelLoaderView.LoadUserControl(adminOverview);

            // Initialize RFID handler data
            await rfidHandler.InitializeAsync();

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

        private async void AttributesScanner_OnScannerInput(object sender, string data)
        {
            Console.WriteLine($"Scanner Input - ScanRFID Mode: {isScanRFIDMode}, ActiveInstance: {ScanRFID.ActiveInstance != null}");

            // Priority 1: If ScanRFID form is active, let it handle the input exclusively
            if (ScanRFID.ActiveInstance != null && !ScanRFID.ActiveInstance.IsDisposed)
            {
                Console.WriteLine("Delegating to ScanRFID ActiveInstance: " + data);
                ScanRFID.ActiveInstance.HandleScannedDataInstance(data);
                return; // CRITICAL: Exit after delegating to prevent double processing
            }

            // Priority 2: Normal RFID attendance processing (only when no ScanRFID form is active)
            else
            {
                Console.WriteLine("Processing via RFID Handler: " + data);
                await rfidHandler.ProcessRFIDScan(data);
            }
        }

        // UI update methods (implement these based on your UI)
        private void UpdateScanHistoryUI(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateScanHistoryUI), message);
                return;
            }

            // If you have a scan history listbox or log display, update it here
            // Example: listBoxScanHistory.Items.Add(message);
        }

        public void RefreshTodoList()
        {
            adminOverview?.RefreshTodoList();
        }

        public async Task RefreshRFIDData()
        {
            await rfidHandler.RefreshData();
        }

        // Public methods to control the scanning mode
        public void EnterScanRFIDMode()
        {
            isScanRFIDMode = true;
            Console.WriteLine("=== SCAN RFID MODE ACTIVATED ===");
        }

        public void ExitScanRFIDMode()
        {
            isScanRFIDMode = false;
            Console.WriteLine("=== SCAN RFID MODE DEACTIVATED ===");
        }
    }
}