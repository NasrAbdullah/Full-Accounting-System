using System;
using System.Data;
using AccountingSystem.Data;
using System.Data.SqlClient;

namespace AccountingSystem.Services
{
    public class ReportService
    {
        public DataTable GetDashboardStats()
        {
            const string sql = @"
                SELECT
                    (SELECT ISNULL(SUM(GrandTotal),0) FROM Sales WHERE CAST(SaleDate AS DATE)=CAST(GETDATE() AS DATE)) AS TodaySales,
                    (SELECT ISNULL(SUM(GrandTotal),0) FROM Sales 
                     WHERE MONTH(SaleDate)=MONTH(GETDATE()) AND YEAR(SaleDate)=YEAR(GETDATE())) AS MonthSales,
                    (SELECT ISNULL(SUM(GrandTotal),0) FROM Purchases 
                     WHERE MONTH(PurchaseDate)=MONTH(GETDATE()) AND YEAR(PurchaseDate)=YEAR(GETDATE())) AS MonthPurchases,
                    (SELECT COUNT(1) FROM Customers WHERE IsActive=1) AS CustomersCount,
                    (SELECT COUNT(1) FROM Suppliers WHERE IsActive=1) AS SuppliersCount,
                    (SELECT COUNT(1) FROM Products WHERE IsActive=1) AS ProductsCount,
                    (SELECT ISNULL(SUM(si.Quantity * (si.SalePrice - si.PurchasePrice) - si.Discount),0)
                     FROM SaleItems si JOIN Sales s ON si.SaleId = s.SaleId
                     WHERE MONTH(s.SaleDate)=MONTH(GETDATE()) AND YEAR(s.SaleDate)=YEAR(GETDATE())) AS MonthProfit,
                    (SELECT ISNULL(SUM(GrandTotal - Paid),0) FROM Sales) AS CustomerDebts,
                    (SELECT ISNULL(SUM(GrandTotal - Paid),0) FROM Purchases) AS SupplierDebts";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetSalesReport(DateTime from, DateTime to)
        {
            const string sql = @"
                SELECT s.InvoiceNumber, s.SaleDate, c.Name AS CustomerName,
                       s.GrandTotal, s.Paid, s.Remaining, s.PaymentMethod
                FROM Sales s
                JOIN Customers c ON s.CustomerId = c.CustomerId
                WHERE s.SaleDate BETWEEN @f AND @t
                ORDER BY s.SaleDate DESC";
            return Fill(sql, from, to);
        }

        public DataTable GetPurchasesReport(DateTime from, DateTime to)
        {
            const string sql = @"
                SELECT p.InvoiceNumber, p.PurchaseDate, s.Name AS SupplierName,
                       p.GrandTotal, p.Paid, p.Remaining, p.PaymentMethod
                FROM Purchases p
                JOIN Suppliers s ON p.SupplierId = s.SupplierId
                WHERE p.PurchaseDate BETWEEN @f AND @t
                ORDER BY p.PurchaseDate DESC";
            return Fill(sql, from, to);
        }

        public DataTable GetProfitReport(DateTime from, DateTime to)
        {
            const string sql = @"
                SELECT s.InvoiceNumber, s.SaleDate, c.Name AS CustomerName,
                       SUM(si.Quantity) AS TotalQty,
                       SUM(si.Total) AS SalesAmount,
                       SUM(si.Quantity * si.PurchasePrice) AS CostAmount,
                       SUM(si.Total - (si.Quantity * si.PurchasePrice)) AS GrossProfit
                FROM Sales s
                JOIN Customers c ON s.CustomerId = c.CustomerId
                JOIN SaleItems si ON s.SaleId = si.SaleId
                WHERE s.SaleDate BETWEEN @f AND @t
                GROUP BY s.InvoiceNumber, s.SaleDate, c.Name
                ORDER BY s.SaleDate DESC";
            return Fill(sql, from, to);
        }

        public DataTable GetTopSellingProducts(DateTime from, DateTime to, int top)
        {
            string sql = @"
                SELECT TOP " + top + @" p.Name AS ProductName, p.ProductCode,
                       SUM(si.Quantity) AS TotalQty,
                       SUM(si.Total) AS TotalAmount
                FROM SaleItems si
                JOIN Products p ON si.ProductId = p.ProductId
                JOIN Sales s ON si.SaleId = s.SaleId
                WHERE s.SaleDate BETWEEN @f AND @t
                GROUP BY p.Name, p.ProductCode
                ORDER BY TotalQty DESC";
            return Fill(sql, from, to);
        }

        private static DataTable Fill(string sql, DateTime from, DateTime to)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@f", from);
                cmd.Parameters.AddWithValue("@t", to);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }
}