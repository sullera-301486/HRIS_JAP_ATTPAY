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
    public partial class DeductionMatrix : Form
    {
        private AttributesClassAlt panelLoaderView;
        private DeductionMatrixDetails deductionMatrixDetails;
        public DeductionMatrix()
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
                labelDeductionMatrix.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                labelUpdateLeaveData.Font = AttributesClass.GetFont("Roboto-Light", 14f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void DeductionMatrix_Load(object sender, EventArgs e)
        {
            panelLoaderView = new AttributesClassAlt(panelDeductionMatrix);
            deductionMatrixDetails = new DeductionMatrixDetails();
            panelLoaderView.LoadUserControl(deductionMatrixDetails);
        }
    }
}
