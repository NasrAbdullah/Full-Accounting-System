using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AccountingSystem.Theme
{
    /// <summary>
    /// زر Sidebar حديث مع Icon + Text + Active indicator
    /// </summary>
    public class NavButton : Panel
    {
        private Label lblIcon;
        private Label lblText;
        private Label lblBadge;
        private bool isHovered = false;
        private bool isActive = false;

        public string Text { get { return lblText.Text; } set { lblText.Text = value; } }
        public string IconText { get { return lblIcon.Text; } set { lblIcon.Text = value; } }
        public string Badge { get; set; }
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; Invalidate(); UpdateColors(); }
        }

        public event EventHandler Clicked;

        public NavButton()
        {
            DoubleBuffered = true;
            Height = 44;
            Dock = DockStyle.Top;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Margin = new Padding(8, 2, 8, 2);
            Padding = new Padding(8);

            lblIcon = new Label();
            lblIcon.Dock = DockStyle.Right;
            lblIcon.Width = 40;
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblIcon.Font = new Font("Segoe UI Emoji", 13F);
            lblIcon.ForeColor = DesignSystem.Colors.TextSecondary;
            lblIcon.BackColor = Color.Transparent;
            lblIcon.Cursor = Cursors.Hand;
            Controls.Add(lblIcon);

            lblText = new Label();
            lblText.Dock = DockStyle.Fill;
            lblText.TextAlign = ContentAlignment.MiddleRight;
            lblText.Font = DesignSystem.Fonts.Body;
            lblText.ForeColor = DesignSystem.Colors.TextSecondary;
            lblText.BackColor = Color.Transparent;
            lblText.Padding = new Padding(0, 0, 8, 0);
            lblText.Cursor = Cursors.Hand;
            Controls.Add(lblText);
            lblText.BringToFront();

            lblBadge = new Label();
            lblBadge.Text = "";
            lblBadge.Font = DesignSystem.Fonts.Tiny;
            lblBadge.ForeColor = Color.White;
            lblBadge.BackColor = DesignSystem.Colors.Danger;
            lblBadge.AutoSize = false;
            lblBadge.Width = 22;
            lblBadge.Height = 18;
            lblBadge.TextAlign = ContentAlignment.MiddleCenter;
            lblBadge.Visible = false;
            lblBadge.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblBadge.Location = new Point(4, 13);
            Controls.Add(lblBadge);

            // Events
            this.MouseEnter += OnMouseEnter;
            this.MouseLeave += OnMouseLeave;
            this.Click += OnClick;

            lblIcon.Click += OnClick;
            lblIcon.MouseEnter += OnMouseEnter;
            lblIcon.MouseLeave += OnMouseLeave;

            lblText.Click += OnClick;
            lblText.MouseEnter += OnMouseEnter;
            lblText.MouseLeave += OnMouseLeave;
        }

        private void OnClick(object sender, EventArgs e)
        {
            if (Clicked != null) Clicked(this, EventArgs.Empty);
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            isHovered = true;
            UpdateColors();
            Invalidate();
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            // Only clear if mouse really left the whole panel
            if (!ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                isHovered = false;
                UpdateColors();
                Invalidate();
            }
        }

        private void UpdateColors()
        {
            Color iconColor, textColor;

            if (isActive)
            {
                iconColor = Color.White;
                textColor = Color.White;
            }
            else if (isHovered)
            {
                iconColor = DesignSystem.Colors.PrimaryHover;
                textColor = DesignSystem.Colors.TextPrimary;
            }
            else
            {
                iconColor = DesignSystem.Colors.TextMuted;
                textColor = DesignSystem.Colors.TextSecondary;
            }

            lblIcon.ForeColor = iconColor;
            lblText.ForeColor = textColor;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = DesignSystem.Radius.M;

            // Background
            if (isActive)
            {
                using (var path = GetRoundedRect(rect, radius))
                using (var brush = new LinearGradientBrush(
                    rect,
                    DesignSystem.Colors.Primary,
                    DesignSystem.Colors.PrimaryDark,
                    LinearGradientMode.Horizontal))
                {
                    g.FillPath(brush, path);
                }
            }
            else if (isHovered)
            {
                using (var path = GetRoundedRect(rect, radius))
                using (var brush = new SolidBrush(DesignSystem.Colors.SurfaceHover))
                {
                    g.FillPath(brush, path);
                }
            }

            base.OnPaint(e);
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