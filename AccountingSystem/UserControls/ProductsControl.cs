using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Forms.Sub;
using AccountingSystem.Helpers;
using AccountingSystem.Repositories;
using AccountingSystem.Theme;

namespace AccountingSystem.UserControls
{
    public class ProductsControl : UserControl
    {
        private TextBox txtSearch;
        private DoubleBufferedGrid dgv;
        private ProductRepository repo = new ProductRepository();

        public ProductsControl()
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

            var btnAdd = new Button();
            btnAdd.Text = "➕ منتج جديد";
            btnAdd.Width = 140;
            btnAdd.Location = new Point(20, 13);
            ThemeManager.ApplyPrimaryButton(btnAdd);
            btnAdd.Click += delegate { OpenAdd(); };
            toolbar.Controls.Add(btnAdd);

            var btnRefresh = new Button();
            btnRefresh.Text = "🔄 تحديث";
            btnRefresh.Width = 100;
            btnRefresh.Location = new Point(170, 13);
            ThemeManager.ApplyGhostButton(btnRefresh);
            btnRefresh.Click += delegate { LoadData(); };
            toolbar.Controls.Add(btnRefresh);

            dgv = new DoubleBufferedGrid();
            dgv.RightToLeft = RightToLeft.Yes;
            dgv.Dock = DockStyle.Fill;
            ThemeManager.ApplyGrid(dgv);
            dgv.Columns.Add(Col("ProductId", "المعرف", 60, null));
            dgv.Columns.Add(Col("ProductCode", "الكود", 130, null));
            dgv.Columns.Add(Col("Name", "المنتج", 220, null));
            dgv.Columns.Add(Col("CategoryName", "التصنيف", 130, null));
            dgv.Columns.Add(Col("PurchasePrice", "سعر الشراء", 100, "N2"));
            dgv.Columns.Add(Col("SalePrice", "سعر البيع", 100, "N2"));
            dgv.Columns.Add(Col("Quantity", "الكمية", 90, "N2"));
            dgv.Columns.Add(Col("MinimumStock", "الحد الأدنى", 100, "N2"));
            dgv.Columns.Add(MakeBtnCol("Edit", "تعديل", 80));
            dgv.Columns.Add(MakeBtnCol("Delete", "حذف", 80));

            dgv.CellContentClick += Dgv_CellContentClick;
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

        private DataGridViewButtonColumn MakeBtnCol(string name, string header, int width)
        {
            var c = new DataGridViewButtonColumn();
            c.Name = name;
            c.HeaderText = header;
            c.Text = header;
            c.UseColumnTextForButtonValue = true;
            c.Width = width;
            c.FlatStyle = FlatStyle.Flat;
            return c;
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];

            decimal qty;
            decimal min;
            decimal.TryParse(Convert.ToString(row.Cells["Quantity"].Value), out qty);
            decimal.TryParse(Convert.ToString(row.Cells["MinimumStock"].Value), out min);

            if (qty <= 0) row.DefaultCellStyle.ForeColor = ThemeManager.Danger;
            else if (qty <= min) row.DefaultCellStyle.ForeColor = ThemeManager.Warning;
            else row.DefaultCellStyle.ForeColor = ThemeManager.Text;
        }

        private void LoadData()
        {
            try
            {
                string search = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
                var list = repo.GetAll(search);
                dgv.Rows.Clear();
                foreach (var p in list)
                    dgv.Rows.Add(p.ProductId, p.ProductCode, p.Name, p.CategoryName,
                        p.PurchasePrice, p.SalePrice, p.Quantity, p.MinimumStock);
            }
            catch (Exception ex) { UiHelper.Error("خطأ: " + ex.Message); }
        }

        private void OpenAdd()
        {
            using (var frm = new ProductEditForm(null))
            {
                if (frm.ShowDialog() == DialogResult.OK) LoadData();
            }
        }

        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["ProductId"].Value);
            string col = dgv.Columns[e.ColumnIndex].Name;

            if (col == "Edit")
            {
                var prod = repo.GetById(id);
                if (prod == null) return;
                using (var frm = new ProductEditForm(prod))
                {
                    if (frm.ShowDialog() == DialogResult.OK) LoadData();
                }
            }
            else if (col == "Delete")
            {
                if (repo.HasTransactions(id)) { UiHelper.Warn("لا يمكن حذف منتج لديه حركات."); return; }
                if (!UiHelper.Confirm("هل أنت متأكد من الحذف؟")) return;
                try { repo.Delete(id); LoadData(); } catch (Exception ex) { UiHelper.Error(ex.Message); }
            }
        }
    }
}