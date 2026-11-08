using System;
using AccountingSystem.Data;
using AccountingSystem.Helpers;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Services
{
    public class PurchaseService
    {
        public int CreatePurchase(Purchase purchase)
        {
            if (purchase.Items.Count == 0)
                throw new InvalidOperationException("لا يمكن حفظ فاتورة بدون منتجات.");
            if (purchase.GrandTotal <= 0)
                throw new InvalidOperationException("إجمالي الفاتورة غير صحيح.");

            using (var conn = DatabaseConnection.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int purchaseId = InsertPurchaseHeader(conn, tx, purchase);

                    foreach (var item in purchase.Items)
                    {
                        if (item.Quantity <= 0)
                            throw new InvalidOperationException(
                                "الكمية غير صحيحة للمنتج: " + item.ProductName);

                        item.Total = item.Quantity * item.PurchasePrice - item.Discount;
                        item.PurchaseId = purchaseId;

                        InsertPurchaseItem(conn, tx, item);
                        IncreaseStock(conn, tx, item.ProductId, item.Quantity);
                        UpdatePurchasePrice(conn, tx, item.ProductId, item.PurchasePrice);
                        InsertStockMovement(conn, tx, item.ProductId, "Purchase",
                            item.Quantity, purchaseId, "Purchase");
                    }

                    if (purchase.Paid > 0)
                        InsertPayment(conn, tx, purchase.SupplierId, purchaseId,
                            purchase.Paid, purchase.PaymentMethod);

                    tx.Commit();
                    return purchaseId;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        private static int InsertPurchaseHeader(SqlConnection conn, SqlTransaction tx, Purchase p)
        {
            const string sql = @"
                INSERT INTO Purchases(InvoiceNumber,PurchaseDate,SupplierId,Subtotal,Discount,Tax,
                                      GrandTotal,Paid,Remaining,PaymentMethod,Notes,UserId)
                VALUES(@num,@date,@sup,@sub,@disc,@tax,@grand,@paid,@rem,@pm,@notes,@uid);
                SELECT SCOPE_IDENTITY();";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@num", p.InvoiceNumber);
                cmd.Parameters.AddWithValue("@date", p.PurchaseDate);
                cmd.Parameters.AddWithValue("@sup", p.SupplierId);
                cmd.Parameters.AddWithValue("@sub", p.Subtotal);
                cmd.Parameters.AddWithValue("@disc", p.Discount);
                cmd.Parameters.AddWithValue("@tax", p.Tax);
                cmd.Parameters.AddWithValue("@grand", p.GrandTotal);
                cmd.Parameters.AddWithValue("@paid", p.Paid);
                cmd.Parameters.AddWithValue("@rem", p.GrandTotal - p.Paid);
                cmd.Parameters.AddWithValue("@pm", (object)p.PaymentMethod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@notes", (object)p.Notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@uid",
                    Session.CurrentUser != null ? (object)Session.CurrentUser.UserId : DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static void InsertPurchaseItem(SqlConnection conn, SqlTransaction tx, PurchaseItem it)
        {
            const string sql = @"
                INSERT INTO PurchaseItems(PurchaseId,ProductId,Quantity,PurchasePrice,Discount,Total)
                VALUES(@pid,@prod,@q,@pp,@d,@t);";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@pid", it.PurchaseId);
                cmd.Parameters.AddWithValue("@prod", it.ProductId);
                cmd.Parameters.AddWithValue("@q", it.Quantity);
                cmd.Parameters.AddWithValue("@pp", it.PurchasePrice);
                cmd.Parameters.AddWithValue("@d", it.Discount);
                cmd.Parameters.AddWithValue("@t", it.Total);
                cmd.ExecuteNonQuery();
            }
        }

        private static void IncreaseStock(SqlConnection conn, SqlTransaction tx, int productId, decimal qty)
        {
            using (var cmd = new SqlCommand(
                "UPDATE Products SET Quantity = Quantity + @q, UpdatedAt=GETDATE() WHERE ProductId=@id",
                conn, tx))
            {
                cmd.Parameters.AddWithValue("@q", qty);
                cmd.Parameters.AddWithValue("@id", productId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void UpdatePurchasePrice(SqlConnection conn, SqlTransaction tx, int productId, decimal price)
        {
            using (var cmd = new SqlCommand(
                "UPDATE Products SET PurchasePrice=@p WHERE ProductId=@id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@p", price);
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
            int supplierId, int purchaseId, decimal amount, string method)
        {
            using (var cmd = new SqlCommand(@"
                INSERT INTO Payments(PaymentDate,Amount,PaymentMethod,SupplierId,PurchaseId)
                VALUES(GETDATE(),@a,@m,@s,@p);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@a", amount);
                cmd.Parameters.AddWithValue("@m", (object)method ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@s", supplierId);
                cmd.Parameters.AddWithValue("@p", purchaseId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}