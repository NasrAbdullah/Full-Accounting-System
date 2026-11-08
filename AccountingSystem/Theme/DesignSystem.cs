using System.Drawing;
using System.Windows.Forms;

namespace AccountingSystem.Theme
{
    /// <summary>
    /// نظام التصميم الموحّد v2.0
    /// كل الألوان والخطوط والمسافات من هنا
    /// </summary>
    public static class DesignSystem
    {
        // ================================================================
        //  الألوان الأساسية
        // ================================================================
        public static class Colors
        {
            // الخلفيات
            public static readonly Color Background = Color.FromArgb(11, 15, 25);
            public static readonly Color Surface = Color.FromArgb(18, 24, 38);
            public static readonly Color SurfaceHover = Color.FromArgb(24, 32, 50);
            public static readonly Color SurfacePressed = Color.FromArgb(30, 40, 60);
            public static readonly Color Sidebar = Color.FromArgb(14, 19, 32);
            public static readonly Color Card = Color.FromArgb(22, 30, 46);
            public static readonly Color CardHover = Color.FromArgb(28, 38, 58);

            // الحدود
            public static readonly Color Border = Color.FromArgb(38, 48, 70);
            public static readonly Color BorderLight = Color.FromArgb(52, 64, 92);
            public static readonly Color BorderActive = Color.FromArgb(96, 132, 200);

            // النصوص
            public static readonly Color TextPrimary = Color.FromArgb(240, 245, 255);
            public static readonly Color TextSecondary = Color.FromArgb(170, 185, 210);
            public static readonly Color TextMuted = Color.FromArgb(120, 140, 170);
            public static readonly Color TextDisabled = Color.FromArgb(90, 105, 130);

            // الألوان الأساسية
            public static readonly Color Primary = Color.FromArgb(96, 132, 240);
            public static readonly Color PrimaryHover = Color.FromArgb(116, 152, 255);
            public static readonly Color PrimaryDark = Color.FromArgb(64, 96, 200);

            // الألوان الدلالية
            public static readonly Color Success = Color.FromArgb(52, 199, 123);
            public static readonly Color SuccessHover = Color.FromArgb(72, 219, 143);
            public static readonly Color SuccessDark = Color.FromArgb(34, 160, 96);

            public static readonly Color Warning = Color.FromArgb(245, 176, 65);
            public static readonly Color WarningHover = Color.FromArgb(255, 196, 85);
            public static readonly Color WarningDark = Color.FromArgb(200, 140, 40);

            public static readonly Color Danger = Color.FromArgb(239, 82, 96);
            public static readonly Color DangerHover = Color.FromArgb(255, 102, 116);
            public static readonly Color DangerDark = Color.FromArgb(190, 55, 70);

            public static readonly Color Info = Color.FromArgb(72, 174, 232);
            public static readonly Color InfoHover = Color.FromArgb(92, 194, 252);

            // ألوان مميزة للـ KPIs
            public static readonly Color Emerald = Color.FromArgb(16, 185, 129);
            public static readonly Color EmeraldDark = Color.FromArgb(5, 150, 105);
            public static readonly Color Blue = Color.FromArgb(59, 130, 246);
            public static readonly Color BlueDark = Color.FromArgb(29, 78, 216);
            public static readonly Color Amber = Color.FromArgb(245, 158, 11);
            public static readonly Color AmberDark = Color.FromArgb(217, 119, 6);
            public static readonly Color Violet = Color.FromArgb(139, 92, 246);
            public static readonly Color VioletDark = Color.FromArgb(109, 40, 217);
            public static readonly Color Rose = Color.FromArgb(244, 63, 94);
            public static readonly Color RoseDark = Color.FromArgb(190, 18, 60);
            public static readonly Color Cyan = Color.FromArgb(6, 182, 212);
            public static readonly Color CyanDark = Color.FromArgb(14, 116, 144);
            public static readonly Color Indigo = Color.FromArgb(99, 102, 241);
            public static readonly Color IndigoDark = Color.FromArgb(67, 56, 202);

            // ألوان الشفافية (Overlays)
            public static readonly Color Overlay = Color.FromArgb(120, 0, 0, 0);
            public static readonly Color GlassLight = Color.FromArgb(30, 255, 255, 255);
        }

        // ================================================================
        //  الخطوط
        // ================================================================
        public static class Fonts
        {
            public const string Family = "Segoe UI";
            public const string Mono = "Consolas";

            public static readonly Font Display = new Font(Family, 24F, FontStyle.Bold);
            public static readonly Font H1 = new Font(Family, 18F, FontStyle.Bold);
            public static readonly Font H2 = new Font(Family, 15F, FontStyle.Bold);
            public static readonly Font H3 = new Font(Family, 13F, FontStyle.Bold);
            public static readonly Font BodyLarge = new Font(Family, 11F, FontStyle.Regular);
            public static readonly Font Body = new Font(Family, 10F, FontStyle.Regular);
            public static readonly Font BodyBold = new Font(Family, 10F, FontStyle.Bold);
            public static readonly Font Small = new Font(Family, 9F, FontStyle.Regular);
            public static readonly Font SmallBold = new Font(Family, 9F, FontStyle.Bold);
            public static readonly Font Tiny = new Font(Family, 8F, FontStyle.Regular);
            public static readonly Font Number = new Font(Family, 11F, FontStyle.Bold);
            public static readonly Font Money = new Font(Family, 12F, FontStyle.Bold);
        }

        // ================================================================
        //  الأيقونات (Unicode Emoji)
        // ================================================================
        public static class Icons
        {
            public const string Dashboard = "🏠";
            public const string Customers = "🧑‍💼";
            public const string Suppliers = "🏭";
            public const string Products = "📱";
            public const string Categories = "🏷";
            public const string Sales = "🛒";
            public const string Purchases = "🚚";
            public const string Inventory = "📦";
            public const string Payments = "💳";
            public const string Expenses = "🧾";
            public const string Income = "💵";
            public const string Reports = "📊";
            public const string Users = "👥";
            public const string Settings = "⚙";
            public const string Backup = "💾";

            public const string Add = "➕";
            public const string Edit = "✏";
            public const string Delete = "🗑";
            public const string Save = "💾";
            public const string Cancel = "❌";
            public const string Search = "🔍";
            public const string Refresh = "🔄";
            public const string Print = "🖨";
            public const string Export = "📤";
            public const string Import = "📥";
            public const string Filter = "⚙";
            public const string Sort = "⇅";
            public const string Check = "✓";
            public const string Close = "✕";
            public const string Warn = "⚠";
            public const string Info = "ℹ";
            public const string Error = "⛔";
            public const string Success = "✅";
            public const string Calendar = "📅";
            public const string Clock = "🕐";
            public const string User = "👤";
            public const string Logout = "🚪";
            public const string Money = "💰";
            public const string Chart = "📈";
            public const string Help = "❓";
            public const string Menu = "☰";
        }

        // ================================================================
        //  المسافات
        // ================================================================
        public static class Spacing
        {
            public const int XS = 4;
            public const int S = 8;
            public const int M = 12;
            public const int L = 16;
            public const int XL = 24;
            public const int XXL = 32;
            public const int Huge = 48;
        }

        // ================================================================
        //  الزوايا
        // ================================================================
        public static class Radius
        {
            public const int XS = 4;
            public const int S = 6;
            public const int M = 10;
            public const int L = 14;
            public const int XL = 20;
            public const int Full = 999;
        }

        // ================================================================
        //  الظلال
        // ================================================================
        public static class Shadow
        {
            public const int None = 0;
            public const int Small = 4;
            public const int Medium = 8;
            public const int Large = 12;
            public const int XLarge = 20;
        }

        // ================================================================
        //  الأحجام القياسية
        // ================================================================
        public static class Sizes
        {
            public const int SidebarWidth = 250;
            public const int HeaderHeight = 68;
            public const int ButtonHeight = 40;
            public const int InputHeight = 38;
            public const int SmallButtonHeight = 32;
            public const int RowHeight = 42;
            public const int HeaderRowHeight = 46;
        }

        // ================================================================
        //  تطبيق سريع
        // ================================================================
        public static void ApplyForm(Form form)
        {
            form.BackColor = Colors.Background;
            form.ForeColor = Colors.TextPrimary;
            form.Font = Fonts.Body;
            form.RightToLeft = RightToLeft.Yes;
            form.RightToLeftLayout = true;
            form.StartPosition = FormStartPosition.CenterScreen;
        }
    }
}