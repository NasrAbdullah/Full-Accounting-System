using System;
using AccountingSystem.Data;
using AccountingSystem.Helpers;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Services
{
    public class SaleService
    {
        public int CreateSale(Sale sale)
        {
            if (sale.Items.Count == 0)
                throw new InvalidOperationException("لا يمكن حفظ فاتورة بدون منتجات.");
            if (sale.GrandTotal <= 0)
                throw new InvalidOperationException("إجمالي الفاتورة غير صحيح.");
            if (sale.Paid < 0 || sale.Paid > sale.GrandTotal)
                throw new InvalidOperationException("المبلغ المدفوع غير صحيح.");

            using (var conn = DatabaseConnection.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int saleId = InsertSaleHeader(conn, tx, sale);

                    foreach (var item in sale.Items)
                    {
                        if (item.Quantity <= 0)
                            throw new InvalidOperationException(
                                "الكمية غير صحيحة للمنتج: " + item.ProductName);

                        decimal currentQty = GetProductQty(conn, tx, item.ProductId);
                        if (currentQty < item.Quantity)
                            throw new InvalidOperationException(
                                "الكمية المطلوبة غير متوفرة في المخزون للمنتج: " + item.ProductName +
                                ". المتاح: " + currentQty);

                        item.PurchasePrice = GetProductPurchasePrice(conn, tx, item.ProductId);
                        item.Total = item.Quantity * item.SalePrice - item.Discount;
                        item.SaleId = saleId;

                        InsertSaleItem(conn, tx, item);
                        DecreaseStock(conn, tx, item.ProductId, item.Quantity);
                        InsertStockMovement(conn, tx, item.ProductId, "Sale",
                            -item.Quantity, saleId, "Sale");
                    }

                    if (sale.Paid > 0)
                        InsertPayment(conn, tx, sale.CustomerId, saleId,
                            sale.Paid, sale.PaymentMethod);

                    tx.Commit();
                    return saleId;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        private static int InsertSaleHeader(SqlConnection conn, SqlTransaction tx, Sale s)
        {
            const string sql = @"
                INSERT INTO Sales(InvoiceNumber,SaleDate,CustomerId,Subtotal,Discount,Tax,
                                  GrandTotal,Paid,Remaining,PaymentMethod,Notes,UserId)
                VALUES(@num,@date,@cust,@sub,@disc,@tax,@grand,@paid,@rem,@pm,@notes,@uid);
                SELECT SCOPE_IDENTITY();";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@num", s.InvoiceNumber);
                cmd.Parameters.AddWithValue("@date", s.SaleDate);
                cmd.Parameters.AddWithValue("@cust", s.CustomerId);
                cmd.Parameters.AddWithValue("@sub", s.Subtotal);
                cmd.Parameters.AddWithValue("@disc", s.Discount);
                cmd.Parameters.AddWithValue("@tax", s.Tax);
                cmd.Parameters.AddWithValue("@grand", s.GrandTotal);
                cmd.Parameters.AddWithValue("@paid", s.Paid);
                cmd.Parameters.AddWithValue("@rem", s.GrandTotal - s.Paid);
                cmd.Parameters.AddWithValue("@pm", (object)s.PaymentMethod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@notes", (object)s.Notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@uid",
                    Session.CurrentUser != null ? (object)Session.CurrentUser.UserId : DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static void InsertSaleItem(SqlConnection conn, SqlTransaction tx, SaleItem it)
        {
            const string sql = @"
                INSERT INTO SaleItems(SaleId,ProductId,Quantity,SalePrice,PurchasePrice,Discount,Total)
                VALUES(@sid,@pid,@q,@sp,@pp,@d,@t);";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@sid", it.SaleId);
                cmd.Parameters.AddWithValue("@pid", it.ProductId);
                cmd.Parameters.AddWithValue("@q", it.Quantity);
                cmd.Parameters.AddWithValue("@sp", it.SalePrice);
                cmd.Parameters.AddWithValue("@pp", it.PurchasePrice);
                cmd.Parameters.AddWithValue("@d", it.Discount);
                cmd.Parameters.AddWithValue("@t", it.Total);
                cmd.ExecuteNonQuery();
            }
        }

        private static decimal GetProductQty(SqlConnection conn, SqlTransaction tx, int productId)
        {
            using (var cmd = new SqlCommand("SELECT Quantity FROM Products WHERE ProductId=@id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", productId);
                var val = cmd.ExecuteScalar();
                return (val == null || val == DBNull.Value) ? 0m : Convert.ToDecimal(val);
            }
        }

        private static decimal GetProductPurchasePrice(SqlConnection conn, SqlTransaction tx, int productId)
        {
            using (var cmd = new SqlCommand("SELECT PurchasePrice FROM Products WHERE ProductId=@id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", productId);
                var val = cmd.ExecuteScalar();
                return (val == null || val == DBNull.Value) ? 0m : Convert.ToDecimal(val);
            }
        }

        private static void DecreaseStock(SqlConnection conn, SqlTransaction tx, int productId, decimal qty)
        {
            using (var cmd = new SqlCommand(
                "UPDATE Products SET Quantity = Quantity - @q, UpdatedAt=GETDATE() WHERE ProductId=@id",
                conn, tx))
            {
                cmd.Parameters.AddWithValue("@q", qty);
                cmd.Parameters.AddWithValue("@id", productId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertStockMovement(SqlConnection conn, SqlTransaction tx,
            int productId, string type, decimal qty, int refId, string refType)
        {
            using (var cmd = new SqlCommand(@"
                INSERT INTO StockMovements(ProductId,MovementType,Quantity,ReferenceId,ReferenceType)
                VALUES(@pid,@t,@q,@rid,@rt);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@pid", productId);
                cmd.Parameters.AddWithValue("@t", type);
                cmd.Parameters.AddWithValue("@q", qty);
                cmd.Parameters.AddWithValue("@rid", refId);
                cmd.Parameters.AddWithValue("@rt", refType);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertPayment(SqlConnection conn, SqlTransaction tx,
            int customerId, int saleId, decimal amount, string method)
        {
            using (var cmd = new SqlCommand(@"
                INSERT INTO Payments(PaymentDate,Amount,PaymentMethod,CustomerId,SaleId)
                VALUES(GETDATE(),@a,@m,@c,@s);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@a", amount);
                cmd.Parameters.AddWithValue("@m", (object)method ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@c", customerId);
                cmd.Parameters.AddWithValue("@s", saleId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}