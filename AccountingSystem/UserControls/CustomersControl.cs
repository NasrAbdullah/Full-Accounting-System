using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Forms.Sub;
using AccountingSystem.Helpers;
using AccountingSystem.Repositories;
using AccountingSystem.Theme;

namespace AccountingSystem.UserControls
{
    public class CustomersControl : UserControl
    {
        private TextBox txtSearch;
        private DoubleBufferedGrid dgv;
        private CustomerRepository repo = new CustomerRepository();

        public CustomersControl()
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
            btnAdd.Text = "➕ عميل جديد";
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

            dgv.Columns.Add(Txt("CustomerId", "المعرف", 70));
            dgv.Columns.Add(Txt("Name", "الاسم", 200));
            dgv.Columns.Add(Txt("Phone", "الهاتف", 120));
            dgv.Columns.Add(Txt("Email", "البريد", 180));
            dgv.Columns.Add(Txt("TotalSales", "إجمالي المبيعات", 130, "N2"));
            dgv.Columns.Add(Txt("TotalPaid", "المدفوع", 110, "N2"));
            dgv.Columns.Add(Txt("Balance", "الرصيد", 110, "N2"));

            dgv.Columns.Add(MakeBtnCol("Edit", "تعديل", 80));
            dgv.Columns.Add(MakeBtnCol("Delete", "حذف", 80));
            dgv.Columns.Add(MakeBtnCol("Details", "الحساب", 90));

            dgv.CellContentClick += Dgv_CellContentClick;
            Controls.Add(dgv);
            dgv.BringToFront();
        }

        private DataGridViewTextBoxColumn Txt(string prop, string header, int width, string format)
        {
            var c = new DataGridViewTextBoxColumn();
            c.DataPropertyName = prop;
            c.Name = prop;
            c.HeaderText = header;
            c.Width = width;
            if (!string.IsNullOrEmpty(format)) c.DefaultCellStyle.Format = format;
            return c;
        }

        private DataGridViewTextBoxColumn Txt(string prop, string header, int width)
        {
            return Txt(prop, header, width, null);
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
                    dgv.Rows.Add(c.CustomerId, c.Name, c.Phone, c.Email,
                        c.TotalSales, c.TotalPaid, c.Balance);
            }
            catch (Exception ex) { UiHelper.Error("خطأ: " + ex.Message); }
        }

        private void OpenAdd()
        {
            using (var frm = new CustomerEditForm(null))
            {
                if (frm.ShowDialog() == DialogResult.OK) LoadData();
            }
        }

        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["CustomerId"].Value);
            string col = dgv.Columns[e.ColumnIndex].Name;

            if (col == "Edit")
            {
                var cust = repo.GetById(id);
                if (cust == null) return;
                using (var frm = new CustomerEditForm(cust))
                {
                    if (frm.ShowDialog() == DialogResult.OK) LoadData();
                }
            }
            else if (col == "Delete")
            {
                if (repo.HasSales(id)) { UiHelper.Warn("لا يمكن حذف عميل لديه فواتير."); return; }
                if (!UiHelper.Confirm("هل أنت متأكد من الحذف؟")) return;
                try { repo.Delete(id); LoadData(); } catch (Exception ex) { UiHelper.Error(ex.Message); }
            }
            else if (col == "Details")
            {
                using (var frm = new CustomerAccountForm(id))
                {
                    frm.ShowDialog();
                }
            }
        }
    }
}