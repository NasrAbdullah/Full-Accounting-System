using System;
using AccountingSystem.Data;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Services
{
    public class PaymentService
    {
        public int AddCustomerPayment(Payment p)
        {
            if (p.Amount <= 0)
                throw new InvalidOperationException("المبلغ يجب أن يكون أكبر من صفر.");

            using (var conn = DatabaseConnection.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int id = Insert(conn, tx, p);
                    if (p.SaleId.HasValue)
                    {
                        using (var cmd = new SqlCommand(@"
                            UPDATE Sales SET Paid = Paid + @a, Remaining = GrandTotal - (Paid + @a)
                            WHERE SaleId=@sid", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@a", p.Amount);
                            cmd.Parameters.AddWithValue("@sid", p.SaleId.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return id;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public int AddSupplierPayment(Payment p)
        {
            if (p.Amount <= 0)
                throw new InvalidOperationException("المبلغ يجب أن يكون أكبر من صفر.");

            using (var conn = DatabaseConnection.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int id = Insert(conn, tx, p);
                    if (p.PurchaseId.HasValue)
                    {
                        using (var cmd = new SqlCommand(@"
                            UPDATE Purchases SET Paid = Paid + @a, Remaining = GrandTotal - (Paid + @a)
                            WHERE PurchaseId=@pid", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@a", p.Amount);
                            cmd.Parameters.AddWithValue("@pid", p.PurchaseId.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return id;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        private static int Insert(SqlConnection conn, SqlTransaction tx, Payment p)
        {
            const string sql = @"
                INSERT INTO Payments(PaymentDate,Amount,PaymentMethod,CustomerId,SupplierId,SaleId,PurchaseId,Notes)
                VALUES(@d,@a,@m,@c,@s,@sid,@pid,@n); SELECT SCOPE_IDENTITY();";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@d", p.PaymentDate);
                cmd.Parameters.AddWithValue("@a", p.Amount);
                cmd.Parameters.AddWithValue("@m", (object)p.PaymentMethod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@c", (object)p.CustomerId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@s", (object)p.SupplierId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@sid", (object)p.SaleId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@pid", (object)p.PurchaseId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@n", (object)p.Notes ?? DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}