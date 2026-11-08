using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AccountingSystem.Theme;

namespace AccountingSystem.Helpers
{
    // ================================================================
    //  Bar Chart Panel - رسم بياني شريطي بألوان متدرجة
    // ================================================================
    public class BarChartPanel : Panel
    {
        public List<ChartData> Data { get; set; }
        public string Title { get; set; }
        public Color BarColor1 { get; set; }
        public Color BarColor2 { get; set; }

        public BarChartPanel()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(22, 28, 40);
            Data = new List<ChartData>();
            Title = "";
            BarColor1 = Color.FromArgb(59, 130, 246);
            BarColor2 = Color.FromArgb(29, 78, 216);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Rounded background
            DrawRoundedBackground(g);

            // Title
            int headerHeight = 50;
            using (var font = new Font("Segoe UI", 12F, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                g.DrawString(Title, font, brush, new PointF(Width - 250, 15));
            }

            // Subtitle line
            using (var pen = new Pen(Color.FromArgb(50, 255, 255, 255), 1))
            {
                g.DrawLine(pen, 20, headerHeight, Width - 20, headerHeight);
            }

            if (Data == null || Data.Count == 0)
            {
                using (var font = new Font("Segoe UI", 11F))
                using (var brush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                {
                    var text = "لا توجد بيانات";
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, brush,
                        (Width - size.Width) / 2, (Height - size.Height) / 2);
                }
                return;
            }

            // Chart area
            int padding = 45;
            int bottomPadding = 55;
            int chartLeft = padding;
            int chartRight = Width - padding;
            int chartTop = headerHeight + 25;
            int chartBottom = Height - bottomPadding;
            int chartWidth = chartRight - chartLeft;
            int chartHeight = chartBottom - chartTop;

            // Max value
            decimal maxValue = 0;
            foreach (var d in Data)
                if (d.Value > maxValue) maxValue = d.Value;
            if (maxValue == 0) maxValue = 1;

            // Grid lines
            using (var gridPen = new Pen(Color.FromArgb(30, 148, 163, 184), 1))
            using (var labelFont = new Font("Segoe UI", 8F))
            using (var labelBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = chartBottom - (chartHeight * i / 4);
                    using (var dashPen = new Pen(Color.FromArgb(30, 148, 163, 184), 1))
                    {
                        dashPen.DashStyle = DashStyle.Dash;
                        g.DrawLine(dashPen, chartLeft, y, chartRight, y);
                    }

                    decimal val = maxValue * i / 4;
                    string lbl = FormatValue(val);
                    g.DrawString(lbl, labelFont, labelBrush, 5, y - 8);
                }
            }

            // Bars
            int barCount = Data.Count;
            int barWidth = Math.Max(28, (chartWidth / barCount) - 20);
            int gap = (chartWidth - (barWidth * barCount)) / (barCount + 1);

            for (int i = 0; i < Data.Count; i++)
            {
                var d = Data[i];
                int x = chartLeft + gap + (i * (barWidth + gap));
                int barHeight = (int)(chartHeight * (double)d.Value / (double)maxValue);
                int y = chartBottom - barHeight;

                if (barHeight < 2) barHeight = 2;

                // Bar with gradient
                if (barHeight > 0)
                {
                    var barRect = new Rectangle(x, y, barWidth, barHeight);
                    using (var brush = new LinearGradientBrush(
                        barRect, BarColor1, BarColor2, LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(brush, barRect);
                    }

                    // Top highlight
                    using (var pen = new Pen(Color.FromArgb(150, 255, 255, 255), 1))
                    {
                        g.DrawLine(pen, x, y, x + barWidth, y);
                    }

                    // Rounded top
                    using (var brush = new LinearGradientBrush(
                        new Rectangle(x, y, barWidth, Math.Min(8, barHeight)),
                        BarColor1, BarColor1, LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(brush, x, y, barWidth, Math.Min(6, barHeight));
                    }
                }

                // Value on top
                using (var valueFont = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var valueBrush = new SolidBrush(Color.FromArgb(248, 250, 252)))
                {
                    string val = FormatValue(d.Value);
                    var valSize = g.MeasureString(val, valueFont);
                    g.DrawString(val, valueFont, valueBrush,
                        x + (barWidth - valSize.Width) / 2, y - 18);
                }

                // X label
                using (var xFont = new Font("Segoe UI", 8F))
                using (var xBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                {
                    var lblSize = g.MeasureString(d.Label, xFont);
                    g.DrawString(d.Label, xFont, xBrush,
                        x + (barWidth - lblSize.Width) / 2, chartBottom + 10);
                }
            }
        }

        private void DrawRoundedBackground(Graphics g)
        {
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = GetRoundedRect(rect, 10))
            using (var brush = new LinearGradientBrush(
                rect, Color.FromArgb(28, 36, 52), Color.FromArgb(20, 26, 38),
                LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            using (var pen = new Pen(Color.FromArgb(50, 59, 130, 246), 1))
            using (var path = GetRoundedRect(rect, 10))
            {
                g.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private string FormatValue(decimal v)
        {
            if (v >= 1000000) return (v / 1000000).ToString("N1") + "M";
            if (v >= 1000) return (v / 1000).ToString("N0") + "K";
            return v.ToString("N0");
        }
    }

    // ================================================================
    //  Pie Chart Panel - رسم دائري بألوان زاهية
    // ================================================================
    public class PieChartPanel : Panel
    {
        public List<ChartData> Data { get; set; }
        public string Title { get; set; }

        private Color[] palette = new Color[]
        {
            Color.FromArgb(59, 130, 246),   // Blue
            Color.FromArgb(16, 185, 129),   // Emerald
            Color.FromArgb(245, 158, 11),   // Amber
            Color.FromArgb(239, 68, 68),    // Red
            Color.FromArgb(139, 92, 246),   // Violet
            Color.FromArgb(236, 72, 153),   // Pink
            Color.FromArgb(6, 182, 212),    // Cyan
            Color.FromArgb(234, 179, 8),    // Yellow
        };

        public PieChartPanel()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(22, 28, 40);
            Data = new List<ChartData>();
            Title = "";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawRoundedBackground(g);

            // Title
            int headerHeight = 50;
            using (var font = new Font("Segoe UI", 12F, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                g.DrawString(Title, font, brush, new PointF(Width - 250, 15));
            }

            using (var pen = new Pen(Color.FromArgb(50, 255, 255, 255), 1))
            {
                g.DrawLine(pen, 20, headerHeight, Width - 20, headerHeight);
            }

            if (Data == null || Data.Count == 0)
            {
                using (var font = new Font("Segoe UI", 11F))
                using (var brush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                {
                    var text = "لا توجد بيانات";
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, brush,
                        (Width - size.Width) / 2, (Height - size.Height) / 2);
                }
                return;
            }

            decimal total = 0;
            foreach (var d in Data) total += d.Value;
            if (total == 0) return;

            // Pie area
            int pieSize = Math.Min(Width - 260, Height - 100);
            if (pieSize < 80) pieSize = 80;
            int pieX = 30;
            int pieY = (Height - pieSize) / 2 + 15;
            var pieRect = new Rectangle(pieX, pieY, pieSize, pieSize);

            // Draw shadow
            var shadowRect = new Rectangle(pieX + 3, pieY + 3, pieSize, pieSize);
            using (var shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
            {
                g.FillEllipse(shadowBrush, shadowRect);
            }

            // Draw pie slices
            float startAngle = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                float sweep = (float)(360.0 * (double)Data[i].Value / (double)total);

                using (var brush = new SolidBrush(palette[i % palette.Length]))
                {
                    g.FillPie(brush, pieRect, startAngle, sweep);
                }

                // White border between slices
                using (var pen = new Pen(Color.FromArgb(22, 28, 40), 3))
                {
                    g.DrawPie(pen, pieRect, startAngle, sweep);
                }

                startAngle += sweep;
            }

            // Inner circle for donut effect
            int innerSize = pieSize / 2;
            var innerRect = new Rectangle(
                pieX + (pieSize - innerSize) / 2,
                pieY + (pieSize - innerSize) / 2,
                innerSize, innerSize);
            using (var innerBrush = new SolidBrush(Color.FromArgb(22, 28, 40)))
            {
                g.FillEllipse(innerBrush, innerRect);
            }

            // Center total
            using (var font = new Font("Segoe UI", 9F))
            using (var brush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                string txt = "الإجمالي";
                var size = g.MeasureString(txt, font);
                g.DrawString(txt, font, brush,
                    pieX + (pieSize - size.Width) / 2, pieY + pieSize / 2 - 18);
            }
            using (var font = new Font("Segoe UI", 11F, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                string txt = total >= 1000 ? (total / 1000).ToString("N0") + "K" : total.ToString("N0");
                var size = g.MeasureString(txt, font);
                g.DrawString(txt, font, brush,
                    pieX + (pieSize - size.Width) / 2, pieY + pieSize / 2);
            }

            // Legend on right side
            int legendX = pieX + pieSize + 40;
            int legendY = pieY + 20;
            int legendItemHeight = 30;

            using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var valueFont = new Font("Segoe UI", 8F))
            using (var textBrush = new SolidBrush(Color.FromArgb(248, 250, 252)))
            using (var mutedBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                for (int i = 0; i < Data.Count; i++)
                {
                    int y = legendY + (i * legendItemHeight);
                    if (y + 25 > Height - 10) break;

                    using (var boxBrush = new SolidBrush(palette[i % palette.Length]))
                    {
                        var boxRect = new Rectangle(legendX, y + 4, 16, 16);
                        using (var path = GetRoundedRect(boxRect, 4))
                        {
                            g.FillPath(boxBrush, path);
                        }
                    }

                    string label = Data[i].Label;
                    if (label.Length > 12) label = label.Substring(0, 12) + "...";

                    g.DrawString(label, font, textBrush, legendX + 24, y + 2);

                    float percent = (float)(100.0 * (double)Data[i].Value / (double)total);
                    string pct = percent.ToString("N1") + "%";
                    g.DrawString(pct, valueFont, mutedBrush, legendX + 24, y + 17);
                }
            }
        }

        private void DrawRoundedBackground(Graphics g)
        {
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = GetRoundedRect(rect, 10))
            using (var brush = new LinearGradientBrush(
                rect, Color.FromArgb(28, 36, 52), Color.FromArgb(20, 26, 38),
                LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            using (var pen = new Pen(Color.FromArgb(50, 139, 92, 246), 1))
            using (var path = GetRoundedRect(rect, 10))
            {
                g.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // ================================================================
    //  KPI Card - بطاقة إحصائية بألوان جميلة
    // ================================================================
    public class KpiCard : Panel
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Icon { get; set; }
        public Color AccentColor { get; set; }
        public Color AccentColorDark { get; set; }
        public string SubText { get; set; }

        public KpiCard()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Margin = new Padding(8);
            Title = "";
            Value = "";
            Icon = "";
            SubText = "";
            AccentColor = Color.FromArgb(59, 130, 246);
            AccentColorDark = Color.FromArgb(29, 78, 216);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // ============ Rounded Card Background ============
            using (var path = GetRoundedRect(rect, 12))
            using (var brush = new LinearGradientBrush(
                rect, Color.FromArgb(30, 40, 58), Color.FromArgb(20, 26, 38),
                LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            // ============ Gradient Border ============
            using (var path = GetRoundedRect(rect, 12))
            using (var pen = new Pen(AccentColor, 2))
            {
                g.DrawPath(pen, path);
            }

            // ============ Icon Circle Background ============
            int iconSize = 52;
            int iconX = Width - iconSize - 20;
            int iconY = 18;

            var iconRect = new Rectangle(iconX, iconY, iconSize, iconSize);
            using (var path = GetRoundedRect(iconRect, 12))
            using (var brush = new LinearGradientBrush(
                iconRect, AccentColor, AccentColorDark,
                LinearGradientMode.ForwardDiagonal))
            {
                g.FillPath(brush, path);
            }

            // Icon
            using (var font = new Font("Segoe UI Emoji", 22F))
            using (var brush = new SolidBrush(Color.White))
            using (var sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(Icon, font, brush,
                    new RectangleF(iconX, iconY, iconSize, iconSize), sf);
            }

            // ============ Title ============
            using (var font = new Font("Segoe UI", 9F))
            using (var brush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                g.DrawString(Title, font, brush, 18, 20);
            }

            // ============ Value ============
            using (var font = new Font("Segoe UI", 16F, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                g.DrawString(Value, font, brush, 18, 48);
            }

            // ============ SubText (Colored) ============
            if (!string.IsNullOrEmpty(SubText))
            {
                using (var font = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var brush = new SolidBrush(AccentColor))
                {
                    g.DrawString(SubText, font, brush, 18, Height - 24);
                }
            }

            // ============ Decorative Dot ============
            using (var brush = new SolidBrush(Color.FromArgb(80, AccentColor)))
            {
                g.FillEllipse(brush, Width - 30, Height - 20, 6, 6);
                g.FillEllipse(brush, Width - 20, Height - 20, 6, 6);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // ================================================================
    //  Chart Data
    // ================================================================
    public class ChartData
    {
        public string Label { get; set; }
        public decimal Value { get; set; }

        public ChartData(string label, decimal value)
        {
            Label = label;
            Value = value;
        }
    }
}