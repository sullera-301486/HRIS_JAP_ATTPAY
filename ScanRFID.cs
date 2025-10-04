using DocumentFormat.OpenXml.Drawing.Charts;
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
        private object parentForm; // store reference
        public static ScanRFID ActiveInstance { get; private set; }
        public ScanRFID(AddNewEmployee parent)
        {
            InitializeComponent();
            setFont();
            parentForm = parent;
        }
        public ScanRFID(EditEmployeeProfileHR parent)
        {
            InitializeComponent();
            setFont();
            parentForm = parent;
        }
        public ScanRFID(EditEmployeeProfile parent)
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

                // Handle different parent types with Invoke
                if (parentForm is AddNewEmployee addParent)
                {
                    addParent.Invoke(new Action(() => addParent.SetRFIDTag(data)));
                }
                else if (parentForm is EditEmployeeProfileHR hrParent)
                {
                    hrParent.Invoke(new Action(() => hrParent.SetRFIDTag(data)));
                }
                else if (parentForm is EditEmployeeProfile editParent)
                {
                    editParent.Invoke(new Action(() => editParent.SetRFIDTag(data)));
                }
            }
            else
            {
                labelScanRFID.Text = "RFID Detected";
                labelRFIDResult.Text = "Tag ID: " + data;

                // Handle different parent types without Invoke
                if (parentForm is AddNewEmployee addParent)
                {
                    addParent.SetRFIDTag(data);
                }
                else if (parentForm is EditEmployeeProfileHR hrParent)
                {
                    hrParent.SetRFIDTag(data);
                }
                else if (parentForm is EditEmployeeProfile editParent)
                {
                    editParent.SetRFIDTag(data);
                }

            }
        }
    }
}
