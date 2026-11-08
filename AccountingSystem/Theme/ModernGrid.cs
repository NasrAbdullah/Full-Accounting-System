using System;
using System.Drawing;
using System.Windows.Forms;

namespace AccountingSystem.Theme
{
    /// <summary>
    /// DataGridView احترافي
    /// </summary>
    public class ModernGrid : DataGridView
    {
        public ModernGrid()
        {
            DoubleBuffered = true;
            ApplyStyle();
        }

        private void ApplyStyle()
        {
            BackgroundColor = DesignSystem.Colors.Surface;
            GridColor = DesignSystem.Colors.Border;
            BorderStyle = BorderStyle.None;
            EnableHeadersVisualStyles = false;
            RightToLeft = RightToLeft.Yes;
            AutoGenerateColumns = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect = false;
            ReadOnly = true;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            RowHeadersVisible = false;
            RowTemplate.Height = DesignSystem.Sizes.RowHeight;
            Font = DesignSystem.Fonts.Body;

            ColumnHeadersDefaultCellStyle.BackColor = DesignSystem.Colors.SurfaceHover;
            ColumnHeadersDefaultCellStyle.ForeColor = DesignSystem.Colors.TextPrimary;
            ColumnHeadersDefaultCellStyle.Font = DesignSystem.Fonts.SmallBold;
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            ColumnHeadersHeight = DesignSystem.Sizes.HeaderRowHeight;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            DefaultCellStyle.BackColor = DesignSystem.Colors.Surface;
            DefaultCellStyle.ForeColor = DesignSystem.Colors.TextPrimary;
            DefaultCellStyle.SelectionBackColor = DesignSystem.Colors.Primary;
            DefaultCellStyle.SelectionForeColor = Color.White;
            DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            DefaultCellStyle.Font = DesignSystem.Fonts.Body;

            AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(
                DesignSystem.Colors.Surface.R + 4,
                DesignSystem.Colors.Surface.G + 4,
                DesignSystem.Colors.Surface.B + 6);

            BorderStyle = BorderStyle.None;
        }

        /// <summary>
        /// إضافة عمود نصي
        /// </summary>
        public DataGridViewTextBoxColumn AddTextColumn(string name, string header, int width, string format = null)
        {
            var col = new DataGridViewTextBoxColumn();
            col.Name = name;
            col.DataPropertyName = name;
            col.HeaderText = header;
            col.Width = width;
            col.SortMode = DataGridViewColumnSortMode.Automatic;
            if (!string.IsNullOrEmpty(format))
            {
                col.DefaultCellStyle.Format = format;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
            Columns.Add(col);
            return col;
        }

        /// <summary>
        /// إضافة عمود أزرار
        /// </summary>
        public DataGridViewButtonColumn AddButtonColumn(string name, string header, string text, int width)
        {
            var col = new DataGridViewButtonColumn();
            col.Name = name;
            col.HeaderText = header;
            col.Text = text;
            col.UseColumnTextForButtonValue = true;
            col.Width = width;
            col.FlatStyle = FlatStyle.Flat;
            col.DefaultCellStyle.BackColor = DesignSystem.Colors.Primary;
            col.DefaultCellStyle.ForeColor = Color.White;
            col.DefaultCellStyle.SelectionBackColor = DesignSystem.Colors.PrimaryHover;
            col.DefaultCellStyle.SelectionForeColor = Color.White;
            Columns.Add(col);
            return col;
        }
    }
}