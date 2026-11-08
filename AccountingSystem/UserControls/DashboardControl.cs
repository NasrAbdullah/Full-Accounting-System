using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Data;
using AccountingSystem.Helpers;
using AccountingSystem.Services;
using AccountingSystem.Theme;
using System.Data.SqlClient;

namespace AccountingSystem.UserControls
{
    public class DashboardControl : UserControl
    {
        private ReportService reportService = new ReportService();
        private string currency = "ر.ي";

        // ألوان جميلة
        private Color colorSalesToday = Color.FromArgb(16, 185, 129);   // Emerald
        private Color colorSalesTodayDark = Color.FromArgb(5, 150, 105);

        private Color colorSalesMonth = Color.FromArgb(59, 130, 246);   // Blue
        private Color colorSalesMonthDark = Color.FromArgb(29, 78, 216);

        private Color colorPurchases = Color.FromArgb(245, 158, 11);    // Amber
        private Color colorPurchasesDark = Color.FromArgb(217, 119, 6);

        private Color colorProfit = Color.FromArgb(139, 92, 246);       // Violet
        private Color colorProfitDark = Color.FromArgb(109, 40, 217);

        private Color colorDebts = Color.FromArgb(239, 68, 68);         // Red
        private Color colorDebtsDark = Color.FromArgb(185, 28, 28);

        public DashboardControl()
        {
            RightToLeft = RightToLeft.Yes;
            BackColor = ThemeManager.Background;
            Dock = DockStyle.Fill;
            AutoScroll = true;
            Load += delegate { Build(); };
        }

        private void Build()
        {
            Controls.Clear();
            try { currency = GetCurrency(); } catch { }

            // ============ 1) KPI Cards ============
            var stats = reportService.GetDashboardStats();
            if (stats.Rows.Count > 0)
            {
                var row = stats.Rows[0];

                var kpiPanel = new TableLayoutPanel();
                kpiPanel.Dock = DockStyle.Top;
                kpiPanel.Height = 150;
                kpiPanel.ColumnCount = 5;
                kpiPanel.RowCount = 1;
                kpiPanel.BackColor = ThemeManager.Background;
                kpiPanel.Padding = new Padding(10, 10, 10, 10);
                for (int i = 0; i < 5; i++)
                    kpiPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));

                kpiPanel.Controls.Add(MakeCard("مبيعات اليوم",
                    Fmt(Get<decimal>(row, "TodaySales")), "🛒",
                    colorSalesToday, colorSalesTodayDark, "اليوم"), 0, 0);

                kpiPanel.Controls.Add(MakeCard("مبيعات الشهر",
                    Fmt(Get<decimal>(row, "MonthSales")), "📈",
                    colorSalesMonth, colorSalesMonthDark, "هذا الشهر"), 1, 0);

                kpiPanel.Controls.Add(MakeCard("مشتريات الشهر",
                    Fmt(Get<decimal>(row, "MonthPurchases")), "🚚",
                    colorPurchases, colorPurchasesDark, "هذا الشهر"), 2, 0);

                kpiPanel.Controls.Add(MakeCard("ربح الشهر",
                    Fmt(Get<decimal>(row, "MonthProfit")), "💰",
                    colorProfit, colorProfitDark, "صافي"), 3, 0);

                kpiPanel.Controls.Add(MakeCard("ديون العملاء",
                    Fmt(Get<decimal>(row, "CustomerDebts")), "📉",
                    colorDebts, colorDebtsDark, "مستحقة"), 4, 0);

                Controls.Add(kpiPanel);
            }

            // ============ 2) Charts Row ============
            var chartsRow = new TableLayoutPanel();
            chartsRow.Dock = DockStyle.Top;
            chartsRow.Height = 340;
            chartsRow.ColumnCount = 2;
            chartsRow.RowCount = 1;
            chartsRow.BackColor = ThemeManager.Background;
            chartsRow.Padding = new Padding(10, 10, 10, 0);
            chartsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            chartsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));

            var barChart = new BarChartPanel();
            barChart.Dock = DockStyle.Fill;
            barChart.Margin = new Padding(5);
            barChart.Title = "📊 المبيعات آخر 7 أيام";
            barChart.BarColor1 = Color.FromArgb(96, 165, 250);   // Light Blue
            barChart.BarColor2 = Color.FromArgb(37, 99, 235);    // Dark Blue
            try { barChart.Data = GetSalesLast7Days(); } catch { }
            chartsRow.Controls.Add(barChart, 0, 0);

            var pieChart = new PieChartPanel();
            pieChart.Dock = DockStyle.Fill;
            pieChart.Margin = new Padding(5);
            pieChart.Title = "🥧 المبيعات حسب التصنيف";
            try { pieChart.Data = GetSalesByCategory(); } catch { }
            chartsRow.Controls.Add(pieChart, 1, 0);

            Controls.Add(chartsRow);

            // ============ 3) Second Row ============
            var row2 = new TableLayoutPanel();
            row2.Dock = DockStyle.Top;
            row2.Height = 320;
            row2.ColumnCount = 2;
            row2.RowCount = 1;
            row2.BackColor = ThemeManager.Background;
            row2.Padding = new Padding(10, 10, 10, 0);
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            var topPanel = MakePanel("🏆 أفضل 5 منتجات مبيعاً",
                Color.FromArgb(245, 158, 11));
            var topGrid = MakeMiniGrid();
            topGrid.Dock = DockStyle.Fill;
            topGrid.Columns.Add(MiniCol("ProductName", "المنتج", 200));
            topGrid.Columns.Add(MiniCol("TotalQty", "الكمية", 80));
            topGrid.Columns.Add(MiniCol("TotalAmount", "الإجمالي", 120, "N2"));
            try { LoadTopProducts(topGrid); } catch { }
            topPanel.Controls.Add(topGrid);
            topGrid.BringToFront();
            row2.Controls.Add(topPanel, 0, 0);

            var lowPanel = MakePanel("⚠ منتجات منخفضة المخزون",
                Color.FromArgb(239, 68, 68));
            var lowGrid = MakeMiniGrid();
            lowGrid.Dock = DockStyle.Fill;
            lowGrid.Columns.Add(MiniCol("ProductCode", "الكود", 100));
            lowGrid.Columns.Add(MiniCol("Name", "المنتج", 180));
            lowGrid.Columns.Add(MiniCol("Quantity", "الكمية", 80, "N2"));
            lowGrid.Columns.Add(MiniCol("MinimumStock", "الحد الأدنى", 90, "N2"));
            try { LoadLowStock(lowGrid); } catch { }
            lowPanel.Controls.Add(lowGrid);
            lowGrid.BringToFront();
            row2.Controls.Add(lowPanel, 1, 0);

            Controls.Add(row2);

            // ============ 4) Recent Activity ============
            var recentPanel = MakePanel("🕐 آخر العمليات",
                Color.FromArgb(59, 130, 246));
            recentPanel.Dock = DockStyle.Top;
            recentPanel.Height = 280;

            var recentGrid = MakeMiniGrid();
            recentGrid.Dock = DockStyle.Fill;
            recentGrid.Columns.Add(MiniCol("Type", "النوع", 120));
            recentGrid.Columns.Add(MiniCol("Ref", "المرجع", 150));
            recentGrid.Columns.Add(MiniCol("Party", "الجهة", 200));
            recentGrid.Columns.Add(MiniCol("Amount", "المبلغ", 120, "N2"));
            recentGrid.Columns.Add(MiniCol("Date", "التاريخ", 150));
            try { LoadRecentActivity(recentGrid); } catch { }
            recentPanel.Controls.Add(recentGrid);
            recentGrid.BringToFront();

            var wrap = new Panel();
            wrap.Dock = DockStyle.Top;
            wrap.Height = 300;
            wrap.BackColor = ThemeManager.Background;
            wrap.Padding = new Padding(15, 10, 15, 20);
            recentPanel.Dock = DockStyle.Fill;
            wrap.Controls.Add(recentPanel);
            Controls.Add(wrap);

            var spacer = new Panel();
            spacer.Dock = DockStyle.Top;
            spacer.Height = 20;
            Controls.Add(spacer);
        }

        private KpiCard MakeCard(string title, string value, string icon,
            Color accent, Color accentDark, string sub)
        {
            var card = new KpiCard();
            card.Dock = DockStyle.Fill;
            card.Title = title;
            card.Value = value;
            card.Icon = icon;
            card.AccentColor = accent;
            card.AccentColorDark = accentDark;
            card.SubText = sub;
            return card;
        }

        private Panel MakePanel(string title, Color accent)
        {
            var panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.FromArgb(22, 28, 40);
            panel.Margin = new Padding(5);
            panel.Padding = new Padding(15);

            // Accent stripe at top
            var topStripe = new Panel();
            topStripe.Dock = DockStyle.Top;
            topStripe.Height = 3;
            topStripe.BackColor = accent;
            panel.Controls.Add(topStripe);

            // Title
            var lbl = new Label();
            lbl.Text = title;
            lbl.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(248, 250, 252);
            lbl.AutoSize = true;
            lbl.Location = new Point(15, 15);
            lbl.BackColor = Color.Transparent;
            panel.Controls.Add(lbl);
            lbl.BringToFront();

            // Spacer
            var spacer = new Panel();
            spacer.Dock = DockStyle.Top;
            spacer.Height = 45;
            spacer.BackColor = Color.Transparent;
            panel.Controls.Add(spacer);
            spacer.BringToFront();

            return panel;
        }

        private DoubleBufferedGrid MakeMiniGrid()
        {
            var g = new DoubleBufferedGrid();
            g.RightToLeft = RightToLeft.Yes;
            ThemeManager.ApplyGrid(g);
            g.RowTemplate.Height = 32;
            g.ColumnHeadersHeight = 34;
            g.Font = new Font("Segoe UI", 9F);
            return g;
        }

        private DataGridViewTextBoxColumn MiniCol(string prop, string header, int width, string format)
        {
            var c = new DataGridViewTextBoxColumn();
            c.DataPropertyName = prop;
            c.Name = prop;
            c.HeaderText = header;
            c.Width = width;
            if (!string.IsNullOrEmpty(format))
            {
                c.DefaultCellStyle.Format = format;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
            return c;
        }

        private DataGridViewTextBoxColumn MiniCol(string prop, string header, int width)
        {
            return MiniCol(prop, header, width, null);
        }

        private void LoadTopProducts(DataGridView g)
        {
            var dt = reportService.GetTopSellingProducts(
                DateTime.Now.AddDays(-30), DateTime.Now, 5);
            g.Rows.Clear();
            foreach (DataRow r in dt.Rows)
                g.Rows.Add(r["ProductName"], r["TotalQty"], r["TotalAmount"]);
        }

        private void LoadLowStock(DataGridView g)
        {
            var products = new Repositories.ProductRepository().GetLowStock();
            g.Rows.Clear();
            int count = 0;
            foreach (var p in products)
            {
                if (count++ >= 5) break;
                g.Rows.Add(p.ProductCode, p.Name, p.Quantity, p.MinimumStock);
            }
        }

        private void LoadRecentActivity(DataGridView g)
        {
            g.Rows.Clear();
            using (var conn = DatabaseConnection.GetConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 3 N'مبيعات' AS Type, s.InvoiceNumber AS Ref,
                           c.Name AS Party, s.GrandTotal AS Amount, s.SaleDate AS Date
                    FROM Sales s JOIN Customers c ON s.CustomerId=c.CustomerId
                    ORDER BY s.SaleId DESC", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        g.Rows.Add(rd["Type"], rd["Ref"], rd["Party"],
                            rd["Amount"], Convert.ToDateTime(rd["Date"]).ToString("yyyy-MM-dd"));
                }

                using (var cmd = new SqlCommand(@"
                    SELECT TOP 3 N'مشتريات' AS Type, p.InvoiceNumber AS Ref,
                           s.Name AS Party, p.GrandTotal AS Amount, p.PurchaseDate AS Date
                    FROM Purchases p JOIN Suppliers s ON p.SupplierId=s.SupplierId
                    ORDER BY p.PurchaseId DESC", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        g.Rows.Add(rd["Type"], rd["Ref"], rd["Party"],
                            rd["Amount"], Convert.ToDateTime(rd["Date"]).ToString("yyyy-MM-dd"));
                }
            }
        }

        private System.Collections.Generic.List<ChartData> GetSalesLast7Days()
        {
            var list = new System.Collections.Generic.List<ChartData>();
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT CAST(SaleDate AS DATE) AS D, ISNULL(SUM(GrandTotal),0) AS Total
                FROM Sales
                WHERE SaleDate >= DATEADD(day, -6, CAST(GETDATE() AS DATE))
                GROUP BY CAST(SaleDate AS DATE)
                ORDER BY D", conn))
            using (var rd = cmd.ExecuteReader())
            {
                var dict = new System.Collections.Generic.Dictionary<DateTime, decimal>();
                while (rd.Read())
                {
                    var d = rd.GetDateTime(0).Date;
                    dict[d] = rd.GetDecimal(1);
                }

                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Now.Date.AddDays(-i);
                    decimal val = 0;
                    if (dict.ContainsKey(date)) val = dict[date];
                    string label = date.ToString("ddd", new System.Globalization.CultureInfo("ar-EG"));
                    list.Add(new ChartData(label, val));
                }
            }
            return list;
        }

        private System.Collections.Generic.List<ChartData> GetSalesByCategory()
        {
            var list = new System.Collections.Generic.List<ChartData>();
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(c.Name, N'بدون تصنيف') AS CatName,
                       ISNULL(SUM(si.Total),0) AS Total
                FROM SaleItems si
                JOIN Products p ON si.ProductId = p.ProductId
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                JOIN Sales s ON si.SaleId = s.SaleId
                WHERE s.SaleDate >= DATEADD(month, -1, GETDATE())
                GROUP BY c.Name
                ORDER BY Total DESC", conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                    list.Add(new ChartData(rd.GetString(0), rd.GetDecimal(1)));
            }
            return list;
        }

        private string Fmt(decimal v)
        {
            if (v >= 1000000) return (v / 1000000).ToString("N1") + "M " + currency;
            if (v >= 1000) return (v / 1000).ToString("N1") + "K " + currency;
            return v.ToString("N0") + " " + currency;
        }

        private static T Get<T>(DataRow row, string col)
        {
            if (row[col] == DBNull.Value) return default(T);
            return (T)Convert.ChangeType(row[col], typeof(T));
        }

        private string GetCurrency()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand(
                    "SELECT Currency FROM CompanySettings WHERE SettingId=1", conn))
                {
                    var r = cmd.ExecuteScalar();
                    return r != null ? r.ToString() : "ر.ي";
                }
            }
            catch { return "ر.ي"; }
        }
    }
}