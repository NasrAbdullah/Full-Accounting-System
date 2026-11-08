using System.Drawing;
using System.Windows.Forms;

namespace AccountingSystem.Theme
{
    public static class ThemeManager
    {
        public static readonly Color Background = Color.FromArgb(16, 20, 28);
        public static readonly Color Sidebar = Color.FromArgb(21, 27, 38);
        public static readonly Color Panel = Color.FromArgb(27, 35, 48);
        public static readonly Color PanelAlt = Color.FromArgb(34, 44, 60);
        public static readonly Color Primary = Color.FromArgb(59, 130, 246);
        public static readonly Color PrimaryHover = Color.FromArgb(37, 99, 235);
        public static readonly Color Success = Color.FromArgb(34, 197, 94);
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);
        public static readonly Color Text = Color.FromArgb(248, 250, 252);
        public static readonly Color Muted = Color.FromArgb(148, 163, 184);
        public static readonly Color Border = Color.FromArgb(45, 55, 72);

        public static readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font FontTitle = new Font("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font FontH1 = new Font("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 9F, FontStyle.Regular);

        public static void ApplyForm(Form form)
        {
            form.BackColor = Background;
            form.ForeColor = Text;
            form.Font = FontRegular;
            form.RightToLeft = RightToLeft.Yes;
            form.RightToLeftLayout = true;
            form.StartPosition = FormStartPosition.CenterScreen;
        }

        public static void ApplyPrimaryButton(Button btn)
        {
            btn.BackColor = Primary;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = FontBold;
            btn.Cursor = Cursors.Hand;
            btn.Height = 38;
            btn.Padding = new Padding(12, 0, 12, 0);
        }

        public static void ApplySuccessButton(Button btn)
        {
            ApplyPrimaryButton(btn);
            btn.BackColor = Success;
        }

        public static void ApplyDangerButton(Button btn)
        {
            ApplyPrimaryButton(btn);
            btn.BackColor = Danger;
        }

        public static void ApplyGhostButton(Button btn)
        {
            btn.BackColor = Panel;
            btn.ForeColor = Text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Border;
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = FontBold;
            btn.Cursor = Cursors.Hand;
            btn.Height = 38;
        }

        public static void ApplyTextBox(TextBox txt)
        {
            txt.BackColor = PanelAlt;
            txt.ForeColor = Text;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Font = FontRegular;
            txt.RightToLeft = RightToLeft.Yes;
        }

        public static void ApplyComboBox(ComboBox cb)
        {
            cb.BackColor = PanelAlt;
            cb.ForeColor = Text;
            cb.FlatStyle = FlatStyle.Flat;
            cb.Font = FontRegular;
            cb.RightToLeft = RightToLeft.Yes;
        }

        public static void ApplyGrid(DataGridView grid)
        {
            grid.BackgroundColor = Panel;
            grid.GridColor = Border;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.RightToLeft = RightToLeft.Yes;
            grid.AutoGenerateColumns = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.RowHeadersVisible = false;
            grid.RowTemplate.Height = 36;
            grid.Font = FontRegular;

            grid.ColumnHeadersDefaultCellStyle.BackColor = PanelAlt;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.Font = FontBold;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersHeight = 42;

            grid.DefaultCellStyle.BackColor = Panel;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = Primary;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = PanelAlt;
        }
    }
}