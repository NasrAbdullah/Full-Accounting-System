using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AccountingSystem.Theme
{
    /// <summary>
    /// حقل نصي حديث مع placeholder + icon
    /// </summary>
    public class ModernTextBox : UserControl
    {
        private TextBox innerText;
        private Label lblIcon;
        private string placeholder = "";
        private bool isFocused = false;
        private string iconText = "";

        public string Placeholder
        {
            get { return placeholder; }
            set { placeholder = value; UpdatePlaceholder(); }
        }

        public string IconText
        {
            get { return iconText; }
            set
            {
                iconText = value;
                if (lblIcon != null)
                {
                    lblIcon.Text = value;
                    lblIcon.Visible = !string.IsNullOrEmpty(value);
                }
            }
        }

        public override string Text
        {
            get { return innerText != null ? innerText.Text : ""; }
            set
            {
                if (innerText != null)
                {
                    innerText.Text = value;
                    UpdatePlaceholder();
                }
            }
        }

        public bool UseSystemPasswordChar
        {
            get { return innerText != null ? innerText.UseSystemPasswordChar : false; }
            set { if (innerText != null) innerText.UseSystemPasswordChar = value; }
        }

        public char PasswordChar
        {
            get { return innerText != null ? innerText.PasswordChar : '\0'; }
            set { if (innerText != null) innerText.PasswordChar = value; }
        }

        public bool ReadOnly
        {
            get { return innerText != null ? innerText.ReadOnly : false; }
            set { if (innerText != null) innerText.ReadOnly = value; }
        }

        public ModernTextBox()
        {
            DoubleBuffered = true;
            BackColor = DesignSystem.Colors.Surface;
            Height = DesignSystem.Sizes.InputHeight;
            Padding = new Padding(DesignSystem.Spacing.M, 0, DesignSystem.Spacing.M, 0);

            // Icon label
            lblIcon = new Label();
            lblIcon.Dock = DockStyle.Right;
            lblIcon.Width = 36;
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblIcon.Font = new Font("Segoe UI Emoji", 12F);
            lblIcon.ForeColor = DesignSystem.Colors.TextMuted;
            lblIcon.BackColor = Color.Transparent;
            lblIcon.Visible = false;
            Controls.Add(lblIcon);

            // TextBox
            innerText = new TextBox();
            innerText.BorderStyle = BorderStyle.None;
            innerText.BackColor = DesignSystem.Colors.Surface;
            innerText.ForeColor = DesignSystem.Colors.TextPrimary;
            innerText.Font = DesignSystem.Fonts.BodyLarge;
            innerText.Dock = DockStyle.Fill;
            innerText.RightToLeft = RightToLeft.Yes;
            innerText.GotFocus += (s, e) => { isFocused = true; Invalidate(); };
            innerText.LostFocus += (s, e) => { isFocused = false; Invalidate(); };
            innerText.TextChanged += (s, e) =>
            {
                UpdatePlaceholder();
                OnTextChanged(EventArgs.Empty);
            };
            Controls.Add(innerText);
            innerText.BringToFront();
        }

        // ============ Helper methods for TextBox compatibility ============
        public new void Focus()
        {
            if (innerText != null) innerText.Focus();
        }

        public new void SelectAll()
        {
            if (innerText != null) innerText.SelectAll();
        }

        public void Clear()
        {
            if (innerText != null) innerText.Clear();
        }

        public int SelectionStart
        {
            get { return innerText != null ? innerText.SelectionStart : 0; }
            set { if (innerText != null) innerText.SelectionStart = value; }
        }

        public int SelectionLength
        {
            get { return innerText != null ? innerText.SelectionLength : 0; }
            set { if (innerText != null) innerText.SelectionLength = value; }
        }

        public new event KeyEventHandler KeyDown
        {
            add { if (innerText != null) innerText.KeyDown += value; }
            remove { if (innerText != null) innerText.KeyDown -= value; }
        }

        public new event KeyPressEventHandler KeyPress
        {
            add { if (innerText != null) innerText.KeyPress += value; }
            remove { if (innerText != null) innerText.KeyPress -= value; }
        }

        public new event EventHandler TextChanged
        {
            add { if (innerText != null) innerText.TextChanged += value; }
            remove { if (innerText != null) innerText.TextChanged -= value; }
        }

        public new event EventHandler GotFocus
        {
            add { if (innerText != null) innerText.GotFocus += value; }
            remove { if (innerText != null) innerText.GotFocus -= value; }
        }

        public new event EventHandler LostFocus
        {
            add { if (innerText != null) innerText.LostFocus += value; }
            remove { if (innerText != null) innerText.LostFocus -= value; }
        }

        // ============ Painting ============
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Background
            using (var brush = new SolidBrush(DesignSystem.Colors.Surface))
            using (var path = GetRoundedRect(rect, DesignSystem.Radius.S))
            {
                g.FillPath(brush, path);
            }

            // Border
            Color borderColor = isFocused
                ? DesignSystem.Colors.Primary
                : DesignSystem.Colors.Border;

            using (var pen = new Pen(borderColor, isFocused ? 2 : 1))
            using (var path = GetRoundedRect(rect, DesignSystem.Radius.S))
            {
                g.DrawPath(pen, path);
            }

            // Placeholder
            if (string.IsNullOrEmpty(innerText.Text) && !isFocused)
            {
                using (var font = new Font(DesignSystem.Fonts.Family, 10F, FontStyle.Italic))
                using (var brush = new SolidBrush(DesignSystem.Colors.TextMuted))
                using (var sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Far;
                    sf.LineAlignment = StringAlignment.Center;
                    sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    var textRect = new Rectangle(
                        DesignSystem.Spacing.M,
                        0,
                        Width - DesignSystem.Spacing.M * 2 - (lblIcon.Visible ? 36 : 0),
                        Height);
                    g.DrawString(placeholder, font, brush, textRect, sf);
                }
            }
        }

        private void UpdatePlaceholder()
        {
            Invalidate();
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