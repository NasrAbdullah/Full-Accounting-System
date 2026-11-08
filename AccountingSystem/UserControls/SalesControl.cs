using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Forms.Sub;
using AccountingSystem.Helpers;
using AccountingSystem.Theme;

namespace AccountingSystem.UserControls
{
    public class SalesControl : UserControl
    {
        private DoubleBufferedGrid dgv;
        private TextBox txtSearch;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;

        public SalesControl()
        {
            RightToLeft = RightToLeft.Yes;
            BackColor = ThemeManager.Background;
            Dock = DockStyle.Fill;
            Build();
        }

        private void Build()
        {
            // ============ Toolbar ============
            var toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 60;
            toolbar.BackColor = ThemeManager.Background;
            toolbar.RightToLeft = RightToLeft.Yes;
            Controls.Add(toolbar);

            var btnNew = new Button();
            btnNew.Text = "➕ فاتورة بيع جديدة";
            btnNew.Width = 200;
            btnNew.Location = new Point(20, 13);
            ThemeManager.ApplyPrimaryButton(btnNew);
            btnNew.Click += delegate
            {
                using (var frm = new SalesInvoiceForm())
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                        LoadSales();
                }
            };
            toolbar.Controls.Add(btnNew);

            var btnRefresh = new Button();
            btnRefresh.Text = "🔄 تحديث";
            btnRefresh.Width = 100;
            btnRefresh.Location = new Point(230, 13);
            ThemeManager.ApplyGhostButton(btnRefresh);
            btnRefresh.Click += delegate { LoadSales(); };
            toolbar.Controls.Add(btnRefresh);

            var btnExport = new Button();
            btnExport.Text = "📊 تصدير Excel";
            btnExport.Width = 130;
            btnExport.Location = new Point(340, 13);
            ThemeManager.ApplyGhostButton(btnExport);
            btnExport.Click += delegate
            {
                ExcelExporter.ExportDataGridView(dgv, "Sales_Report");
            };
            toolbar.Controls.Add(btnExport);

            // Search box
            txtSearch = new TextBox();
            txtSearch.Width = 250;
            txtSearch.Location = new Point(toolbar.Width - 270, 15);
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ThemeManager.ApplyTextBox(txtSearch);
            txtSearch.TextChanged += delegate { LoadSales(); };
            toolbar.Controls.Add(txtSearch);

            // ============ Grid ============
            dgv = new DoubleBufferedGrid();
            dgv.Dock = DockStyle.Fill;
            dgv.RightToLeft = RightToLeft.Yes;
            ThemeManager.ApplyGrid(dgv);

            dgv.Columns.Add(Col("SaleId", "ID", 60));
            dgv.Columns.Add(Col("InvoiceNumber", "رقم الفاتورة", 130));
            dgv.Columns.Add(Col("SaleDate", "التاريخ", 120));
            dgv.Columns.Add(Col("CustomerName", "العميل", 180));
            dgv.Columns.Add(Col("GrandTotal", "الإجمالي", 110, "N2"));
            dgv.Columns.Add(Col("Paid", "المدفوع", 100, "N2"));
            dgv.Columns.Add(Col("Remaining", "المتبقي", 100, "N2"));

            var btnPrintCol = new DataGridViewButtonColumn();
            btnPrintCol.Name = "Print";
            btnPrintCol.HeaderText = "طباعة";
            btnPrintCol.Text = "🖨️ طباعة";
            btnPrintCol.UseColumnTextForButtonValue = true;
            btnPrintCol.Width = 90;
            btnPrintCol.FlatStyle = FlatStyle.Flat;
            dgv.Columns.Add(btnPrintCol);

            var btnDetailsCol = new DataGridViewButtonColumn();
            btnDetailsCol.Name = "Details";
            btnDetailsCol.HeaderText = "التفاصيل";
            btnDetailsCol.Text = "عرض";
            btnDetailsCol.UseColumnTextForButtonValue = true;
            btnDetailsCol.Width = 80;
            btnDetailsCol.FlatStyle = FlatStyle.Flat;
            dgv.Columns.Add(btnDetailsCol);

            dgv.CellContentClick += Dgv_CellContentClick;
            Controls.Add(dgv);
            dgv.BringToFront();

            LoadSales();
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

        private void LoadSales()
        {
            try
            {
                var dt = new DataTable();
                string search = txtSearch != null ? txtSearch.Text.Trim() : "";

                string sql = @"
                    SELECT TOP 200 s.SaleId, s.InvoiceNumber, s.SaleDate,
                           c.Name AS CustomerName,
                           s.GrandTotal, s.Paid, s.Remaining
                    FROM Sales s
                    JOIN Customers c ON s.CustomerId = c.CustomerId
                    WHERE (@search = '' OR s.InvoiceNumber LIKE '%' + @search + '%'
                           OR c.Name LIKE '%' + @search + '%')
                    ORDER BY s.SaleId DESC";

                using (var conn = Data.DatabaseConnection.GetConnection())
                using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@search", search);
                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                        da.Fill(dt);
                }

                dgv.Rows.Clear();
                foreach (DataRow r in dt.Rows)
                {
                    dgv.Rows.Add(r["SaleId"], r["InvoiceNumber"],
                        Convert.ToDateTime(r["SaleDate"]).ToString("yyyy-MM-dd"),
                        r["CustomerName"], r["GrandTotal"], r["Paid"], r["Remaining"]);
                }
            }
            catch (Exception ex) { UiHelper.Error("خطأ: " + ex.Message); }
        }

        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int saleId = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["SaleId"].Value);
            string col = dgv.Columns[e.ColumnIndex].Name;

            if (col == "Print")
            {
                try
                {
                    InvoicePrinter.PrintSale(saleId);
                }
                catch (Exception ex)
                {
                    UiHelper.Error("فشل الطباعة: " + ex.Message);
                }
            }
            else if (col == "Details")
            {
                UiHelper.Info("عرض التفاصيل: الفاتورة رقم " + saleId);
            }
        }
    }
}