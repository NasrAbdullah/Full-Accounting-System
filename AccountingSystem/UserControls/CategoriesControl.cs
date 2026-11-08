using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Forms.Sub;
using AccountingSystem.Helpers;
using AccountingSystem.Repositories;
using AccountingSystem.Theme;

namespace AccountingSystem.UserControls
{
    public class CategoriesControl : UserControl
    {
        private TextBox txtSearch;
        private DoubleBufferedGrid dgv;
        private CategoryRepository repo = new CategoryRepository();

        public CategoriesControl()
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
            btnAdd.Text = "➕ تصنيف جديد";
            btnAdd.Width = 150;
            btnAdd.Location = new Point(20, 13);
            ThemeManager.ApplyPrimaryButton(btnAdd);
            btnAdd.Click += delegate { OpenAdd(); };
            toolbar.Controls.Add(btnAdd);

            var btnRefresh = new Button();
            btnRefresh.Text = "🔄 تحديث";
            btnRefresh.Width = 100;
            btnRefresh.Location = new Point(180, 13);
            ThemeManager.ApplyGhostButton(btnRefresh);
            btnRefresh.Click += delegate { LoadData(); };
            toolbar.Controls.Add(btnRefresh);

            dgv = new DoubleBufferedGrid();
            dgv.RightToLeft = RightToLeft.Yes;
            dgv.Dock = DockStyle.Fill;
            ThemeManager.ApplyGrid(dgv);
            dgv.Columns.Add(Txt("CategoryId", "المعرف", 70));
            dgv.Columns.Add(Txt("Name", "التصنيف", 250));
            dgv.Columns.Add(Txt("Notes", "ملاحظات", 350));
            dgv.Columns.Add(Txt("ProductCount", "عدد المنتجات", 130));
            dgv.Columns.Add(MakeBtnCol("Edit", "تعديل", 80));
            dgv.Columns.Add(MakeBtnCol("Delete", "حذف", 80));

            dgv.CellContentClick += Dgv_CellContentClick;
            Controls.Add(dgv);
            dgv.BringToFront();
        }

        private DataGridViewTextBoxColumn Txt(string prop, string header, int width)
        {
            var c = new DataGridViewTextBoxColumn();
            c.DataPropertyName = prop;
            c.Name = prop;
            c.HeaderText = header;
            c.Width = width;
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

        private void LoadData()
        {
            try
            {
                string search = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
                var list = repo.GetAll(search);
                dgv.Rows.Clear();
                foreach (var c in list)
                    dgv.Rows.Add(c.CategoryId, c.Name, c.Notes, c.ProductCount);
            }
            catch (Exception ex) { UiHelper.Error("خطأ: " + ex.Message); }
        }

        private void OpenAdd()
        {
            using (var frm = new CategoryEditForm(null))
            {
                if (frm.ShowDialog() == DialogResult.OK) LoadData();
            }
        }

        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["CategoryId"].Value);
            string col = dgv.Columns[e.ColumnIndex].Name;

            if (col == "Edit")
            {
                var list = repo.GetAll(null);
                Models.Category cat = null;
                foreach (var c in list) if (c.CategoryId == id) { cat = c; break; }
                if (cat == null) return;
                using (var frm = new CategoryEditForm(cat))
                {
                    if (frm.ShowDialog() == DialogResult.OK) LoadData();
                }
            }
            else if (col == "Delete")
            {
                if (repo.HasProducts(id)) { UiHelper.Warn("لا يمكن حذف تصنيف مرتبط بمنتجات."); return; }
                if (!UiHelper.Confirm("هل أنت متأكد من الحذف؟")) return;
                try { repo.Delete(id); LoadData(); } catch (Exception ex) { UiHelper.Error(ex.Message); }
            }
        }
    }
}