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
    public partial class Calendar : UserControl
    {
        private DateTime currentMonth = DateTime.Now;

        public Calendar()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.Font = AttributesClass.GetFont("Roboto-Light", 11f);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.Clear(Color.White);

            Rectangle bounds = this.ClientRectangle;
            int padding = 10;

            // Title
            string title = "Calendar";
            using (Font titleFont = AttributesClass.GetFont("Roboto-Regular", 16f))
            using (Brush brush = new SolidBrush(Color.FromArgb(43, 23, 112)))
            {
                SizeF size = g.MeasureString(title, titleFont);
                g.DrawString(title, titleFont, brush, (bounds.Width - size.Width) / 2, padding);
            }

            // Month-Year label
            string monthYear = currentMonth.ToString("MMMM yyyy");
            using (Font monthFont = AttributesClass.GetFont("Roboto-Light", 12f))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                SizeF size = g.MeasureString(monthYear, monthFont);
                g.DrawString(monthYear, monthFont, brush, (bounds.Width - size.Width) / 2, padding + 30);
            }

            // Calendar grid
            int startY = padding + 70;
            int cellWidth = bounds.Width / 7;
            int cellHeight = (bounds.Height - startY - padding) / 7;

            string[] days = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };

            // Draw day headers
            for (int i = 0; i < 7; i++)
            {
                g.DrawString(days[i], this.Font, Brushes.Gray, i * cellWidth + 15, startY + 5);
            }

            // Dates
            DateTime firstDay = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            int skip = (int)firstDay.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);

            int col = skip;
            int row = 1;


            for (int day = 1; day <= daysInMonth; day++)
            {
                string dayText = day.ToString();

                // Default font

                Font dayFont = this.Font;
                Brush brush = Brushes.Black;
                int cellX = col * cellWidth + 10;
                int cellY = startY + row * cellHeight;
                Rectangle cellRect = new Rectangle(cellX, cellY, (cellWidth - 20), cellHeight);
                // Highlight today if it's in the same month/year
                if (currentMonth.Year == DateTime.Today.Year &&
                    currentMonth.Month == DateTime.Today.Month &&
                    day == DateTime.Today.Day)
                {
                    using (Brush highlight = new SolidBrush(Color.FromArgb(96, 81, 148)))
                    {
                        g.FillRectangle(highlight, cellRect);
                    }
                    dayFont = new Font(this.Font, FontStyle.Bold);
                    brush = Brushes.White;
                }

                SizeF textSize = g.MeasureString(dayText, dayFont);
                float x = col * cellWidth + (cellWidth - textSize.Width) / 2;
                float y = startY + row * cellHeight + (cellHeight - textSize.Height) / 2;

                g.DrawString(dayText, dayFont, brush, x, y);

                col++;
                if (col == 7)
                {
                    col = 0;
                    row++;
                }

                // Dispose only if we created a bold font
                if (dayFont != this.Font) dayFont.Dispose();

            }
        }


        }
}

