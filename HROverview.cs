using System;
using System.Drawing;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class HROverview : UserControl
    {
        public HROverview()
        {
            InitializeComponent();
            setFont();
            setActivityLogsDataGridViewAttributes();
            setTodoDataGridViewAttributes();
            setAlertAbsentDataGridViewAttributes();
            setAlertLateDataGridViewAttributes();
            setPanelAttributes();
            setCalendar();

        }
        private void setFont()
        {
            labelHROverview.Font = AttributesClass.GetFont("Roboto-Light", 20f);
            labelOverviewDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            labelAttendanceSummary.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            comboBoxSelectDate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            labelTotalNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
            labelOnTimeNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
            labelAbsentNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
            labelLateNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
            labelTotalTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            labelOnTimeTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            labelLateTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            labelAbsentTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            labelTotalDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelTotalDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOnTimeDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOnTimeDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelLateDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelLateDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelAbsentDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelAbsentDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
        }

        private void setPanelAttributes()
        {
            panelCalendar.Paint += panelCalendar_Paint;
            panelAttendanceSummary.Paint += panelAttendanceSummary_Paint;
        }

        private void setCalendar()
        {
            Calendar myCalendar = new Calendar();
            int borderPadding = 1;

            myCalendar.Location = new Point(borderPadding, borderPadding);
            myCalendar.Size = new Size(
                panelCalendar.ClientSize.Width - borderPadding * 2,
                panelCalendar.ClientSize.Height - borderPadding * 2
            );

            // make it resize along with the panel
            myCalendar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            panelCalendar.Controls.Clear();
            panelCalendar.Controls.Add(myCalendar);
        }

        private void panelCalendar_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelCalendar.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelAttendanceSummary_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelAttendanceSummary.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelAttendanceSummary_Resize(object sender, EventArgs e)
        {
            panelAttendanceSummary.Invalidate();
        }

        private void setActivityLogsDataGridViewAttributes()
        {
            dataGridViewActivityLogs.GridColor = Color.White;
            dataGridViewActivityLogs.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewActivityLogs.ColumnHeadersHeight = 40;
            dataGridViewActivityLogs.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewActivityLogs.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            for (int i = 1; i < 30; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewActivityLogs.Rows.Add("6 - " + i + " - 25", "10:30 PM", "Manual Entry Added", "Added attendance for Franz Louies Deloritos.");
            }
        }

        private void panelActivityLog_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelActivityLog.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelActivityLog_Resize(object sender, EventArgs e)
        {
            panelActivityLog.Invalidate();
        }

        private void setTodoDataGridViewAttributes()
        {
            dataGridViewTodo.GridColor = Color.White;
            dataGridViewTodo.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewTodo.ColumnHeadersHeight = 40;
            dataGridViewTodo.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewTodo.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            dataGridViewTodo.CellMouseEnter += dataGridViewTodo_CellMouseEnter;
            dataGridViewTodo.CellMouseLeave += dataGridViewTodo_CellMouseLeave;
            dataGridViewTodo.CellClick += dataGridViewTodo_CellClick;
            for (int i = 1; i < 10; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewTodo.Rows.Add("Sample task " + i + ".", "10-24-2025");
            }
        }

        private void setAlertAbsentDataGridViewAttributes()
        {
            dataGridViewAlertAbsent.GridColor = Color.White;
            dataGridViewAlertAbsent.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAlertAbsent.ColumnHeadersHeight = 40;
            dataGridViewAlertAbsent.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAlertAbsent.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            for (int i = 1; i < 10; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewAlertAbsent.Rows.Add("Franz Louies Deloritos", i);
            }
        }

        private void setAlertLateDataGridViewAttributes()
        {
            dataGridViewAlertLate.GridColor = Color.White;
            dataGridViewAlertLate.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAlertLate.ColumnHeadersHeight = 40;
            dataGridViewAlertLate.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAlertLate.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            for (int i = 1; i < 10; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewAlertLate.Rows.Add("Franz Louies Deloritos", i);
            }
        }

        private void panelTodo_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelTodo.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelTodo_Resize(object sender, EventArgs e)
        {
            panelTodo.Invalidate();
        }

        private void panelAlert_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelAlert.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelAlert_Resize(object sender, EventArgs e)
        {
            panelAlert.Invalidate();
        }

        private void dataGridViewTodo_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewTodo.Columns[e.ColumnIndex].Name == "ColumnTrash")
                dataGridViewTodo.Cursor = Cursors.Hand;
        }

        private void dataGridViewTodo_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewTodo.Cursor = Cursors.Default;
        }

        private void dataGridViewTodo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewTodo.Columns[e.ColumnIndex].Name == "ColumnTrash")
            {
                Form parentForm = this.FindForm();
                ConfirmDeleteTask confirmDeleteTaskForm = new ConfirmDeleteTask();
                AttributesClass.ShowWithOverlay(parentForm, confirmDeleteTaskForm); //should depend on database in the future
            }
        }

        private void labelTodoAdd_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            AddNewTask addNewTaskForm = new AddNewTask();
            AttributesClass.ShowWithOverlay(parentForm, addNewTaskForm);
        }
    }
}
