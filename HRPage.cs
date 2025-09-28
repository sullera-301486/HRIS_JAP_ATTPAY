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
        public HRForm(string userId)
        {
            currentUserId = userId;
            InitializeComponent();

            AttributesClass.SetMinSize(this, 1440, 1024);
            this.Size = new Size(1240, 824);
            panelLoaderMenu = new AttributesClassAlt(HRMenuPanel);
            panelLoaderView = new AttributesClassAlt(HRViewPanel);

        }
        private void HRPage_Load(object sender, EventArgs e)
        {
            panelLoaderMenu.LoadUserControl(new HRMenu(HRViewPanel, currentUserId));
            panelLoaderView.LoadUserControl(new HROverview(currentUserId));
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
    }
}