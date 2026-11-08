using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Data;
using AccountingSystem.Helpers;
using AccountingSystem.Repositories;
using AccountingSystem.Theme;
using System.Data.SqlClient;

namespace AccountingSystem.Forms.Sub
{
    public class CustomerAccountForm : Form
    {
        private int customerId;
        private DataGridView dgv;
        private Label lblSummary;

        public CustomerAccountForm(int customerId)
        {
            this.customerId = customerId;
            ThemeManager.ApplyForm(this);
            Size = new Size(950, 650);
            var repo = new CustomerRepository();
            var c = repo.GetById(customerId);
            Text = "حساب العميل: " + (c != null ? c.Name : "");
            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            lblSummary = new Label();
            lblSummary.Dock = DockStyle.Top;
            lblSummary.Height = 80;
            lblSummary.Font = ThemeManager.FontH1;
            lblSummary.ForeColor = ThemeManager.Text;
            lblSummary.Padding = new Padding(20, 15, 20, 0);
            lblSummary.BackColor = ThemeManager.Panel;
            Controls.Add(lblSummary);

            dgv = new DoubleBufferedGrid();
            dgv.Dock = DockStyle.Fill;
            ThemeManager.ApplyGrid(dgv);
            dgv.Columns.Add(MakeCol("التاريخ", 130));
            dgv.Columns.Add(MakeCol("النوع", 120));
            dgv.Columns.Add(MakeCol("المرجع", 150));
            dgv.Columns.Add(MakeCol("مدين", 120));
            dgv.Columns.Add(MakeCol("دائن", 120));
            dgv.Columns.Add(MakeCol("الرصيد", 120));
            Controls.Add(dgv);
            dgv.BringToFront();
        }

        private DataGridViewTextBoxColumn MakeCol(string header, int width)
        {
            var c = new DataGridViewTextBoxColumn();
            c.HeaderText = header;
            c.Width = width;
            return c;
        }

        private void LoadData()
        {
            var repo = new CustomerRepository();
            var c = repo.GetById(customerId);
            if (c == null) return;
            lblSummary.Text = "العميل: " + c.Name +
                "   |   إجمالي المبيعات: " + c.TotalSales.ToString("N2") +
                "   |   المدفوع: " + c.TotalPaid.ToString("N2") +
                "   |   الرصيد: " + c.Balance.ToString("N2");

            dgv.Rows.Clear();
            decimal running = c.OpeningBalance;
            if (c.OpeningBalance != 0)
            {
                dgv.Rows.Add(c.CreatedAt.ToString("yyyy-MM-dd"),
                    "رصيد افتتاحي", "-", c.OpeningBalance, 0, c.OpeningBalance);
            }

            using (var conn = DatabaseConnection.GetConnection())
            {
                using (var cmd = new SqlCommand(
                    "SELECT InvoiceNumber, SaleDate, GrandTotal FROM Sales WHERE CustomerId=@id ORDER BY SaleDate",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", customerId);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            decimal amt = rd.GetDecimal(2);
                            running += amt;
                            dgv.Rows.Add(rd.GetDateTime(1).ToString("yyyy-MM-dd"),
                                "فاتورة بيع", rd.GetString(0), amt, 0, running);
                        }
                    }
                }
                using (var cmd = new SqlCommand(
                    "SELECT PaymentDate, Amount FROM Payments WHERE CustomerId=@id ORDER BY PaymentDate",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", customerId);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            decimal amt = rd.GetDecimal(1);
                            running -= amt;
                            dgv.Rows.Add(rd.GetDateTime(0).ToString("yyyy-MM-dd"),
                                "دفعة", "دفعة", 0, amt, running);
                        }
                    }
                }
            }
        }
    }
}