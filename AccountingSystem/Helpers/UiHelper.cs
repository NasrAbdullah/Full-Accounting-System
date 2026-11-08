using System.Windows.Forms;
using AccountingSystem.Theme;

namespace AccountingSystem.Helpers
{
    public static class UiHelper
    {
        private const MessageBoxOptions RtlOpts =
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;

        // ============ Toast الإشعارات الحديثة ============
        public static void ToastSuccess(string msg) { Toast.Success(msg); }
        public static void ToastWarning(string msg) { Toast.Warning(msg); }
        public static void ToastError(string msg) { Toast.Error(msg); }
        public static void ToastInfo(string msg) { Toast.Info(msg); }

        // ============ MessageBox التقليدية ============
        public static void Info(string msg, string title = "معلومة")
        {
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, RtlOpts);
        }

        public static void Warn(string msg, string title = "تنبيه")
        {
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1, RtlOpts);
        }

        public static void Error(string msg, string title = "خطأ")
        {
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, RtlOpts);
        }

        public static bool Confirm(string msg, string title = "تأكيد")
        {
            return MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2, RtlOpts) == DialogResult.Yes;
        }

        public static Label MakeLabel(string text, bool muted = false)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.AutoSize = true;
            if (muted)
            {
                lbl.ForeColor = DesignSystem.Colors.TextMuted;
                lbl.Font = DesignSystem.Fonts.Small;
            }
            else
            {
                lbl.ForeColor = DesignSystem.Colors.TextPrimary;
                lbl.Font = DesignSystem.Fonts.Body;
            }
            lbl.BackColor = System.Drawing.Color.Transparent;
            return lbl;
        }
    }
}