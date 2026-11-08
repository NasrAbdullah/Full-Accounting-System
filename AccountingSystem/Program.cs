using System;
using System.Windows.Forms;
using AccountingSystem.Data;
using AccountingSystem.Forms;

namespace AccountingSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                DatabaseInitializer.EnsureDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "تعذر الاتصال بقاعدة البيانات:\n" + ex.Message,
                    "خطأ في الاتصال",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return;
            }

            using (var login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new MainForm());
                }
            }
        }
    }
}