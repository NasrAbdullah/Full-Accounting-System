using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Helpers;
using AccountingSystem.Repositories;
using AccountingSystem.Theme;

namespace AccountingSystem.UserControls
{
    public class InventoryControl : UserControl
    {
        private TextBox txtSearch;
        private DoubleBufferedGrid dgv;
        private ProductRepository repo = new ProductRepository();
        private Label lblSummary;

        public InventoryControl()
        {
            RightToLeft = RightToLeft.Yes;
            
            
            
            BackColor = ThemeManager.Background;
            Dock = DockStyle.Fill;
            Build();
            LoadData();
        }

        private void Build()
        {
            var toolbar = new Panel();
            toolbar.RightToLeft = RightToLeft.Yes;
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 60;
            toolbar.
            RightToLeft = RightToLeft.Yes;
            
            
            BackColor = ThemeManager.Background;
            Controls.Add(toolbar);

            txtSearch = new TextBox();
            txtSearch.Width = 280;
            txtSearch.Location = new Point(toolbar.Width - 300, 15);
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ThemeManager.ApplyTextBox(txtSearch);
            txtSearch.TextChanged += delegate { LoadData(); };
            toolbar.Controls.Add(txtSearch);

            var btnRefresh = new Button();
            btnRefresh.Text = "🔄 تحديث";
            btnRefresh.Width = 100;
            btnRefresh.Location = new Point(20, 13);
            ThemeManager.ApplyGhostButton(btnRefresh);
            btnRefresh.Click += delegate { LoadData(); };
            toolbar.Controls.Add(btnRefresh);

            lblSummary = new Label();
            lblSummary.RightToLeft = RightToLeft.Yes;
            lblSummary.Dock = DockStyle.Top;
            lblSummary.Height = 40;
            lblSummary.Font = ThemeManager.FontBold;
            lblSummary.ForeColor = ThemeManager.Text;
            lblSummary.Padding = new Padding(20, 10, 20, 0);
            lblSummary.BackColor = ThemeManager.Panel;
            Controls.Add(lblSummary);
            lblSummary.BringToFront();
            toolbar.BringToFront();

            dgv = new DoubleBufferedGrid();
            dgv.RightToLeft = RightToLeft.Yes;
            dgv.Dock = DockStyle.Fill;
            ThemeManager.ApplyGrid(dgv);
            dgv.Columns.Add(Col("ProductCode", "الكود", 130));
            dgv.Columns.Add(Col("Name", "المنتج", 250));
            dgv.Columns.Add(Col("CategoryName", "التصنيف", 130));
            dgv.Columns.Add(Col("Quantity", "الكمية الحالية", 120, "N2"));
            dgv.Columns.Add(Col("MinimumStock", "الحد الأدنى", 100, "N2"));
            dgv.Columns.Add(Col("PurchasePrice", "سعر الشراء", 110, "N2"));
            dgv.Columns.Add(Col("SalePrice", "سعر البيع", 110, "N2"));
            dgv.Columns.Add(Col("StockStatus", "الحالة", 120));
            dgv.CellFormatting += Dgv_CellFormatting;
            Controls.Add(dgv);
            dgv.BringToFront();
        }

        private DataGridViewTextBoxColumn Col(string prop, string header, int width, string format)
        {
            var c = new DataGridViewTextBoxColumn();
            c.DataPropertyName = prop;
            c.Name = prop;
            c.HeaderText = header;
            c.Width = width;
            if (!string.IsNullOrEmpty(format)) c.DefaultCellStyle.Format = format;
            return c;
        }

        private DataGridViewTextBoxColumn Col(string prop, string header, int width)
        {
            return Col(prop, header, width, null);
        }

        private void LoadData()
        {
            try
            {
                string search = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
                var list = repo.GetAll(search);

                int available = 0, low = 0, outOfStock = 0;

                dgv.Rows.Clear();
                foreach (var p in list)
                {
                    string status;
                    if (p.Quantity <= 0) { status = "نفد المخزون"; outOfStock++; }
                    else if (p.Quantity <= p.MinimumStock) { status = "منخفض"; low++; }
                    else { status = "متوفر"; available++; }

                    dgv.Rows.Add(p.ProductCode, p.Name, p.CategoryName,
                        p.Quantity, p.MinimumStock, p.PurchasePrice, p.SalePrice, status);
                }

                lblSummary.Text = "✅ متوفر: " + available +
                    "   |   ⚠ منخفض: " + low +
                    "   |   ❌ نفد: " + outOfStock;
            }
            catch (Exception ex) { UiHelper.Error("خطأ: " + ex.Message); }
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            if (row.Cells["StockStatus"].Value == null) return;
            string status = row.Cells["StockStatus"].Value.ToString();

            if (status == "نفد المخزون")
                row.DefaultCellStyle.ForeColor = ThemeManager.Danger;
            else if (status == "منخفض")
                row.DefaultCellStyle.ForeColor = ThemeManager.Warning;
            else
                row.DefaultCellStyle.ForeColor = ThemeManager.Text;
        }
    }
}