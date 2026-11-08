using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AccountingSystem.Theme
{
    public enum ButtonVariant
    {
        Primary,
        Success,
        Warning,
        Danger,
        Info,
        Ghost,
        Outline
    }

    /// <summary>
    /// زر حديث بألوان متدرجة وظلال
    /// </summary>
    public class ModernButton : Button
    {
        private bool isHovered = false;
        private bool isPressed = false;

        public ButtonVariant Variant { get; set; }
        public int CornerRadius { get; set; }
        public string IconText { get; set; }

        public ModernButton()
        {
            Variant = ButtonVariant.Primary;
            CornerRadius = DesignSystem.Radius.M;
            IconText = "";
            DoubleBuffered = true;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = DesignSystem.Fonts.BodyBold;
            Cursor = Cursors.Hand;
            Height = DesignSystem.Sizes.ButtonHeight;
            Padding = new Padding(DesignSystem.Spacing.M, 0, DesignSystem.Spacing.M, 0);

            MouseEnter += (s, e) => { isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { isHovered = false; isPressed = false; Invalidate(); };
            MouseDown += (s, e) => { isPressed = true; Invalidate(); };
            MouseUp += (s, e) => { isPressed = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            // ═══════════════════════════════════════════════════════════
            //  حماية شاملة من أي استثناء
            // ═══════════════════════════════════════════════════════════
            try
            {
                // إذا الأبعاد صفر أو سلبية → ارسم بدون تدرج
                if (Width <= 0 || Height <= 0)
                {
                    base.OnPaint(pevent);
                    return;
                }

                var g = pevent.Graphics;
                if (g == null) { base.OnPaint(pevent); return; }

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                if (rect.Width <= 0 || rect.Height <= 0)
                {
                    base.OnPaint(pevent);
                    return;
                }

                GraphicsPath path = GetRoundedRect(rect, CornerRadius);
                if (path == null) { base.OnPaint(pevent); return; }

                // Get colors
                Color bg1, bg2, border, fg;
                GetColors(out bg1, out bg2, out border, out fg);

                // ═══════════════════════════════════════════════════════════
                //  Shadow (only when hovered)
                // ═══════════════════════════════════════════════════════════
                if (isHovered && !isPressed && Width > 4 && Height > 4)
                {
                    try
                    {
                        var shadowRect = new Rectangle(2, 3, Width - 4, Height - 3);
                        if (shadowRect.Width > 0 && shadowRect.Height > 0)
                        {
                            using (var shadowPath = GetRoundedRect(shadowRect, CornerRadius))
                            using (var shadowBrush = new SolidBrush(Color.FromArgb(40, bg1)))
                            {
                                g.FillPath(shadowBrush, shadowPath);
                            }
                        }
                    }
                    catch { /* تجاهل أخطاء الظل */ }
                }

                // ═══════════════════════════════════════════════════════════
                //  Background
                // ═══════════════════════════════════════════════════════════
                var bgRect = isPressed
                    ? new Rectangle(1, 1, Width - 2, Height - 2)
                    : rect;

                if (bgRect.Width > 0 && bgRect.Height > 0)
                {
                    // إذا الألوان متشابهة → SolidBrush (أسرع وأكثر أماناً)
                    if (bg1 == bg2 || Variant == ButtonVariant.Ghost)
                    {
                        using (var brush = new SolidBrush(bg1))
                        {
                            g.FillPath(brush, path);
                        }
                    }
                    else
                    {
                        // Gradient → لكن داخل try/catch للاحتياط
                        try
                        {
                            using (var brush = new LinearGradientBrush(
                                bgRect, bg1, bg2, LinearGradientMode.Vertical))
                            {
                                g.FillPath(brush, path);
                            }
                        }
                        catch
                        {
                            // Fallback: SolidBrush
                            using (var brush = new SolidBrush(bg1))
                            {
                                g.FillPath(brush, path);
                            }
                        }
                    }
                }

                // ═══════════════════════════════════════════════════════════
                //  Border
                // ═══════════════════════════════════════════════════════════
                if (border != Color.Empty && border.A > 0)
                {
                    try
                    {
                        using (var pen = new Pen(border, 1))
                        {
                            g.DrawPath(pen, path);
                        }
                    }
                    catch { /* تجاهل أخطاء الحدود */ }
                }

                // ═══════════════════════════════════════════════════════════
                //  Icon + Text
                // ═══════════════════════════════════════════════════════════
                string display = string.IsNullOrEmpty(IconText) ? Text : IconText + "  " + Text;
                if (!string.IsNullOrEmpty(display))
                {
                    try
                    {
                        using (var font = Font ?? SystemFonts.DefaultFont)
                        using (var brush = new SolidBrush(fg))
                        using (var sf = new StringFormat())
                        {
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;
                            sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                            g.DrawString(display, font, brush, rect, sf);
                        }
                    }
                    catch { /* تجاهل أخطاء النص */ }
                }

                path.Dispose();
            }
            catch
            {
                // ═══════════════════════════════════════════════════════════
                //  في حالة فشل كل شيء → ارسم الزر بشكل افتراضي
                // ═══════════════════════════════════════════════════════════
                try
                {
                    base.OnPaint(pevent);
                }
                catch { /* لا شيء */ }
            }
        }

        private void GetColors(out Color bg1, out Color bg2, out Color border, out Color fg)
        {
            bg1 = DesignSystem.Colors.Primary;
            bg2 = DesignSystem.Colors.PrimaryDark;
            border = Color.Empty;
            fg = Color.White;

            try
            {
                switch (Variant)
                {
                    case ButtonVariant.Primary:
                        bg1 = isHovered ? DesignSystem.Colors.PrimaryHover : DesignSystem.Colors.Primary;
                        bg2 = DesignSystem.Colors.PrimaryDark;
                        break;

                    case ButtonVariant.Success:
                        bg1 = isHovered ? DesignSystem.Colors.SuccessHover : DesignSystem.Colors.Success;
                        bg2 = DesignSystem.Colors.SuccessDark;
                        break;

                    case ButtonVariant.Warning:
                        bg1 = isHovered ? DesignSystem.Colors.WarningHover : DesignSystem.Colors.Warning;
                        bg2 = DesignSystem.Colors.WarningDark;
                        fg = Color.FromArgb(40, 30, 10);
                        break;

                    case ButtonVariant.Danger:
                        bg1 = isHovered ? DesignSystem.Colors.DangerHover : DesignSystem.Colors.Danger;
                        bg2 = DesignSystem.Colors.DangerDark;
                        break;

                    case ButtonVariant.Info:
                        bg1 = isHovered ? DesignSystem.Colors.InfoHover : DesignSystem.Colors.Info;
                        bg2 = Color.FromArgb(40, 130, 190);
                        break;

                    case ButtonVariant.Ghost:
                        bg1 = isHovered ? DesignSystem.Colors.SurfaceHover : DesignSystem.Colors.Surface;
                        bg2 = bg1;
                        border = DesignSystem.Colors.Border;
                        fg = DesignSystem.Colors.TextPrimary;
                        break;

                    case ButtonVariant.Outline:
                        bg1 = isHovered ? DesignSystem.Colors.SurfaceHover : Color.Transparent;
                        bg2 = bg1;
                        border = DesignSystem.Colors.Primary;
                        fg = isHovered ? DesignSystem.Colors.PrimaryHover : DesignSystem.Colors.Primary;
                        break;
                }
            }
            catch
            {
                // Fallback
                bg1 = Color.SteelBlue;
                bg2 = Color.DarkBlue;
                fg = Color.White;
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0)
                return path;

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int d = radius * 2;
            // تأكد أن القطر لا يتجاوز العرض أو الارتفاع
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            try
            {
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
            }
            catch
            {
                // Fallback
                path.Reset();
                path.AddRectangle(rect);
            }

            return path;
        }
    }
}