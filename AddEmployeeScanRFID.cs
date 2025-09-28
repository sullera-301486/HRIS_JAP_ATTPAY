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
    public partial class AddEmployeeScanRFID : Form
    {
        public string rfidnum { get; set; }
        public AddEmployeeScanRFID()
        {
            InitializeComponent();
            setFont();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelRFIDScan.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                labelRFIDResult.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        public void HandleScannerInput(string scannedData)
        {
            // Keep only numbers
            string numericOnly = new string(scannedData.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrEmpty(numericOnly))
            {
                rfidnum = numericOnly;
                labelRFIDResult.Text = numericOnly;
                Console.WriteLine("Child form received RFID: " + numericOnly);
            }
        }
    }
}
