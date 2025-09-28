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
    public partial class ScanRFID : Form
    {
        private AddNewEmployee parentForm; // store reference
        public static ScanRFID ActiveInstance { get; private set; }
        public ScanRFID(AddNewEmployee parent)
        {
            InitializeComponent();
            setFont();
            parentForm = parent;
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            labelScanRFID.Font = AttributesClass.GetFont("Roboto-Regular", 20);
            labelRFIDResult.Font = AttributesClass.GetFont("Roboto-Light", 12f);
        }

        private void ScanRFID_Load(object sender, EventArgs e)
        {
            ActiveInstance = this;
        }

        private void ScanRFID_FormClosing(object sender, FormClosingEventArgs e)
        {
            ActiveInstance = null;
        }

        public void HandleScannedDataInstance(string data)
        {
            //if this gets updated for database purposes, follow the invoke logic for if, and regular logic for else
            if (labelRFIDResult.InvokeRequired)
            {
                labelRFIDResult.Invoke(new Action(() => labelRFIDResult.Text = "Tag ID: " + data));
                labelScanRFID.Invoke(new Action(() => labelScanRFID.Text = "RFID Detected"));
                parentForm.Invoke(new Action(() => parentForm.SetRFIDTag(data)));
            }
            else
            {
                labelScanRFID.Text = "RFID Detected";
                labelRFIDResult.Text = "Tag ID: " + data;
                parentForm.SetRFIDTag(data);
            }
        }
    }
}
