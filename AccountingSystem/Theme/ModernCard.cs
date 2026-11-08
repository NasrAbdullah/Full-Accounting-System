using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AccountingSystem.Theme
{
    /// <summary>
    /// بطاقة حديثة مع ظل وحواف مستديرة
    /// </summary>
    public class ModernCard : Panel
    {
        public int CornerRadius { get; set; }
        public Color CardColor { get; set; }
        public bool ShowShadow { get; set; }
        public Color AccentColor { get; set; }
        public bool ShowAccent { get; set; }

        public ModernCard()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            CornerRadius = DesignSystem.Radius.L;
            CardColor = DesignSystem.Colors.Card;
            ShowShadow = true;
            AccentColor = DesignSystem.Colors.Primary;
            ShowAccent = false;
            Padding = new Padding(DesignSystem.Spacing.L);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Shadow
            if (ShowShadow)
            {
                for (int i = 0; i < 4; i++)
                {
                    var shadowRect = new Rectangle(2 + i, 2 + i, Width - 4, Height - 4);
                    using (var path = GetRoundedRect(shadowRect, CornerRadius))
                    using (var brush = new SolidBrush(Color.FromArgb(15 - i * 3, 0, 0, 0)))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }

            // Card background
            using (var brush = new SolidBrush(CardColor))
            using (var path = GetRoundedRect(rect, CornerRadius))
            {
                g.FillPath(brush, path);
            }

            // Accent stripe (right side for RTL)
            if (ShowAccent)
            {
                using (var brush = new SolidBrush(AccentColor))
                using (var path = GetRoundedRect(rect, CornerRadius))
                {
                    var clip = g.Clip;
                    g.SetClip(path);
                    g.FillRectangle(brush, new Rectangle(Width - 5, 0, 5, Height));
                    g.Clip = clip;
                }
            }

            // Border
            using (var pen = new Pen(DesignSystem.Colors.Border, 1))
            using (var path = GetRoundedRect(rect, CornerRadius))
            {
                g.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}