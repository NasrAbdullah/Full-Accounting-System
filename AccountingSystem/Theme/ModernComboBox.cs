using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AccountingSystem.Theme
{
    /// <summary>
    /// ComboBox حديث
    /// </summary>
    public class ModernComboBox : ComboBox
    {
        public ModernComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            BackColor = DesignSystem.Colors.Surface;
            ForeColor = DesignSystem.Colors.TextPrimary;
            Font = DesignSystem.Fonts.BodyLarge;
            Height = DesignSystem.Sizes.InputHeight;
            FlatStyle = FlatStyle.Flat;
            RightToLeft = RightToLeft.Yes;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var bg = new SolidBrush(selected ? DesignSystem.Colors.Primary : DesignSystem.Colors.Surface))
            {
                g.FillRectangle(bg, e.Bounds);
            }

            string text = Items[e.Index].ToString();
            using (var brush = new SolidBrush(selected ? Color.White : DesignSystem.Colors.TextPrimary))
            using (var sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Far;
                sf.LineAlignment = StringAlignment.Center;
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                var textRect = new Rectangle(
                    e.Bounds.X + DesignSystem.Spacing.M,
                    e.Bounds.Y,
                    e.Bounds.Width - DesignSystem.Spacing.M * 2,
                    e.Bounds.Height);
                g.DrawString(text, Font, brush, textRect, sf);
            }
        }
    }
}