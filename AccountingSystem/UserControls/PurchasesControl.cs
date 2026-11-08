using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Forms.Sub;
using AccountingSystem.Helpers;
using AccountingSystem.Theme;

namespace AccountingSystem.UserControls
{
    public class PurchasesControl : UserControl
    {
        public PurchasesControl()
        {
            RightToLeft = RightToLeft.Yes;
            
            
            
            BackColor = ThemeManager.Background;
            Dock = DockStyle.Fill;
            Build();
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

            var btnNew = new Button();
            btnNew.Text = "➕ فاتورة شراء جديدة";
            btnNew.Width = 200;
            btnNew.Location = new Point(20, 13);
            ThemeManager.ApplyPrimaryButton(btnNew);
            btnNew.Click += delegate
            {
                using (var frm = new PurchaseInvoiceForm())
                {
                    frm.ShowDialog();
                }
            };
            toolbar.Controls.Add(btnNew);

            var dgv = new DoubleBufferedGrid();
            dgv.RightToLeft = RightToLeft.Yes;
            dgv.Dock = DockStyle.Fill;
            ThemeManager.ApplyGrid(dgv);
            dgv.Columns.Add(Col("InvoiceNumber", "رقم الفاتورة", 150));
            dgv.Columns.Add(Col("PurchaseDate", "التاريخ", 130));
            dgv.Columns.Add(Col("SupplierName", "المورد", 200));
            dgv.Columns.Add(Col("GrandTotal", "الإجمالي", 120, "N2"));
            dgv.Columns.Add(Col("Paid", "المدفوع", 110, "N2"));
            dgv.Columns.Add(Col("Remaining", "المتبقي", 110, "N2"));
            Controls.Add(dgv);
            dgv.BringToFront();

            LoadPurchases();
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

        private void LoadPurchases()
        {
            try
            {
                var dt = new System.Data.DataTable();
                using (var conn = Data.DatabaseConnection.GetConnection())
                using (var cmd = new System.Data.SqlClient.SqlCommand(@"
                    SELECT TOP 200 p.InvoiceNumber, p.PurchaseDate, s.Name AS SupplierName,
                           p.GrandTotal, p.Paid, p.Remaining
                    FROM Purchases p JOIN Suppliers s ON p.SupplierId=s.SupplierId
                    ORDER BY p.PurchaseId DESC", conn))
                using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                var dgv = FindGrid();
                if (dgv == null) return;
                dgv.Rows.Clear();
                foreach (System.Data.DataRow r in dt.Rows)
                    dgv.Rows.Add(r["InvoiceNumber"], r["PurchaseDate"], r["SupplierName"],
                        r["GrandTotal"], r["Paid"], r["Remaining"]);
            }
            catch { }
        }

        private DataGridView FindGrid()
        {
            foreach (Control c in Controls)
            {
                if (c is DataGridView) return (DataGridView)c;
            }
            return null;
        }
    }
}