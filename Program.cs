using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    internal static class Program
    {
        private static Form currentForm;
        private static string currentUserId;
        private static string currentEmployeeId;
        private static string payrollPeriod;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FormHost hostForm = new FormHost();
            Application.Run(hostForm); // Run app using persistent host
        }

        public static void OpenNextForm(string formName, string userId = null)
        {
           

            if (!string.IsNullOrEmpty(userId))
            {
                currentUserId = userId;
            }
            Application.OpenForms
                  .OfType<Form>()
                  .Where(f => f != null && !f.IsDisposed && f != currentForm)
                  .ToList()
                  .ForEach(f => f.Hide());

            Form nextForm = null;

            switch (formName)
            {
                case "LoginForm":
                    nextForm = new LoginForm();
                    break;
                case "OpenNewForm1":
                    nextForm = new AdminForm(currentUserId, currentEmployeeId, payrollPeriod);
                    break;
                case "OpenNewForm2":
                    nextForm = new HRForm(currentUserId);
                    break;
                default:
                    MessageBox.Show("Invalid form requested, exiting.");
                    Application.Exit();
                    return;
            }

            currentForm = nextForm;

            nextForm.FormClosed += (s, e) =>
            {
                Form closedForm = s as Form;
                string tag = closedForm?.Tag as string;

                if (!string.IsNullOrEmpty(tag))
                {
                    OpenNextForm(tag);
                }
                else
                {
                    Application.Exit();
                }
            };

            nextForm.Show();
        }
    }
    public partial class FormHost : Form
    {
        public FormHost()
        {
            this.Load += (s, e) =>
            {
                Program.OpenNextForm("LoginForm");
            };
            // Make it completely invisible
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(-2000, -2000);
            this.Size = new System.Drawing.Size(1, 1);
        }
        public void TransitionToLogin()
        {
            Timer delay = new Timer();
            delay.Interval = 100; // 100ms gives enough time to let current calls complete
            delay.Tick += (s, e) =>
            {
                delay.Stop();
                delay.Dispose();

                // Close all open forms except FormHost
                foreach (var form in Application.OpenForms.OfType<Form>().ToList())
                {
                    if (form != this)
                        form.Close();
                }

                // Now open login form AFTER others have closed
                Timer openLogin = new Timer();
                openLogin.Interval = 100;
                openLogin.Tick += (s2, e2) =>
                {
                    openLogin.Stop();
                    openLogin.Dispose();
                    Program.OpenNextForm("LoginForm");
                };
                openLogin.Start();
            };
            delay.Start();
        }
    }
}