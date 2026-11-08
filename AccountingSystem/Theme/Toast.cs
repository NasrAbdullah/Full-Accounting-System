using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AccountingSystem.Theme
{
    public enum ToastType
    {
        Success,
        Warning,
        Error,
        Info
    }

    /// <summary>
    /// إشعار منبثق جميل
    /// </summary>
    public class Toast : Form
    {
        private Timer hideTimer;
        private Timer slideTimer;
        private int targetY;
        private int currentStep = 0;
        private const int TotalSteps = 20;

        public Toast(string message, string title, ToastType type)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Size = new Size(360, 80);
            BackColor = DesignSystem.Colors.Card;

            // Position: bottom-left
            var screen = Screen.PrimaryScreen.WorkingArea;
            int xPos = screen.Width - Width - 20;
            int yPos = screen.Height - Height - 20;

            // Slide animation start (from bottom)
            Location = new Point(xPos, screen.Height);
            targetY = yPos;

            // Colors
            Color accent = GetAccentColor(type);
            string icon = GetIcon(type);

            // Right accent stripe
            var stripe = new Panel();
            stripe.Dock = DockStyle.Right;
            stripe.Width = 5;
            stripe.BackColor = accent;
            Controls.Add(stripe);

            // Icon
            var lblIcon = new Label();
            lblIcon.Text = icon;
            lblIcon.Font = new Font("Segoe UI Emoji", 20F);
            lblIcon.ForeColor = accent;
            lblIcon.AutoSize = false;
            lblIcon.Width = 55;
            lblIcon.Dock = DockStyle.Right;
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblIcon.BackColor = Color.Transparent;
            Controls.Add(lblIcon);

            // Title
            var lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = DesignSystem.Fonts.BodyBold;
            lblTitle.ForeColor = DesignSystem.Colors.TextPrimary;
            lblTitle.AutoSize = false;
            lblTitle.Location = new Point(15, 12);
            lblTitle.Width = Width - 80;
            lblTitle.Height = 22;
            lblTitle.TextAlign = ContentAlignment.MiddleRight;
            lblTitle.RightToLeft = RightToLeft.Yes;
            lblTitle.BackColor = Color.Transparent;
            Controls.Add(lblTitle);

            // Message
            var lblMsg = new Label();
            lblMsg.Text = message;
            lblMsg.Font = DesignSystem.Fonts.Small;
            lblMsg.ForeColor = DesignSystem.Colors.TextSecondary;
            lblMsg.AutoSize = false;
            lblMsg.Location = new Point(15, 36);
            lblMsg.Width = Width - 80;
            lblMsg.Height = 34;
            lblMsg.TextAlign = ContentAlignment.TopRight;
            lblMsg.RightToLeft = RightToLeft.Yes;
            lblMsg.BackColor = Color.Transparent;
            Controls.Add(lblMsg);

            // Border
            Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(DesignSystem.Colors.BorderLight, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            };

            // Slide in
            slideTimer = new Timer();
            slideTimer.Interval = 15;
            slideTimer.Tick += SlideTick;
            slideTimer.Start();

            // Auto hide after 3 seconds
            hideTimer = new Timer();
            hideTimer.Interval = 3500;
            hideTimer.Tick += delegate
            {
                hideTimer.Stop();
                slideTimer.Start();
            };
            hideTimer.Start();
        }

        private void SlideTick(object sender, EventArgs e)
        {
            if (currentStep < TotalSteps && Location.Y > targetY)
            {
                currentStep++;
                int newY = Location.Y - (Location.Y - targetY) / (TotalSteps - currentStep + 2) - 2;
                if (newY < targetY) newY = targetY;
                Location = new Point(Location.X, newY);
            }
            else if (currentStep >= TotalSteps)
            {
                slideTimer.Stop();
                Close();
            }
        }

        private static Color GetAccentColor(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success: return DesignSystem.Colors.Success;
                case ToastType.Warning: return DesignSystem.Colors.Warning;
                case ToastType.Error: return DesignSystem.Colors.Danger;
                case ToastType.Info: return DesignSystem.Colors.Info;
                default: return DesignSystem.Colors.Primary;
            }
        }

        private static string GetIcon(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success: return "✅";
                case ToastType.Warning: return "⚠";
                case ToastType.Error: return "⛔";
                case ToastType.Info: return "ℹ";
                default: return "🔔";
            }
        }

        public static void Show(string message, string title = null, ToastType type = ToastType.Info)
        {
            if (title == null)
            {
                switch (type)
                {
                    case ToastType.Success: title = "تم بنجاح"; break;
                    case ToastType.Warning: title = "تنبيه"; break;
                    case ToastType.Error: title = "خطأ"; break;
                    case ToastType.Info: title = "معلومة"; break;
                }
            }

            var toast = new Toast(message, title, type);
            toast.Show();
        }

        public static void Success(string message) { Show(message, null, ToastType.Success); }
        public static void Warning(string message) { Show(message, null, ToastType.Warning); }
        public static void Error(string message) { Show(message, null, ToastType.Error); }
        public static void Info(string message) { Show(message, null, ToastType.Info); }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;   // WS_EX_TOOLWINDOW
                cp.ExStyle |= 0x08000000;   // WS_EX_NOACTIVATE
                return cp;
            }
        }
    }
}