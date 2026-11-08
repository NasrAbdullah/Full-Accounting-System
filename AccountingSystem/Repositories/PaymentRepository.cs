using System;
using System.Collections.Generic;
using AccountingSystem.Data;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Repositories
{
    public class PaymentRepository
    {
        public int Add(Payment p)
        {
            const string sql = @"
                INSERT INTO Payments(PaymentDate,Amount,PaymentMethod,CustomerId,SupplierId,SaleId,PurchaseId,Notes)
                VALUES(@d,@a,@m,@c,@s,@sid,@pid,@n);
                SELECT SCOPE_IDENTITY();";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
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

        public List<Payment> GetByCustomer(int customerId)
        {
            var list = new List<Payment>();
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(
                "SELECT PaymentId,PaymentDate,Amount,PaymentMethod,CustomerId,SupplierId,SaleId,PurchaseId,Notes " +
                "FROM Payments WHERE CustomerId=@id ORDER BY PaymentDate DESC", conn))
            {
                cmd.Parameters.AddWithValue("@id", customerId);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read()) list.Add(Map(rd));
                }
            }
            return list;
        }

        private static Payment Map(SqlDataReader rd)
        {
            return new Payment
            {
                PaymentId = rd.GetInt32(0),
                PaymentDate = rd.GetDateTime(1),
                Amount = rd.GetDecimal(2),
                PaymentMethod = rd.IsDBNull(3) ? null : rd.GetString(3),
                CustomerId = rd.IsDBNull(4) ? (int?)null : rd.GetInt32(4),
                SupplierId = rd.IsDBNull(5) ? (int?)null : rd.GetInt32(5),
                SaleId = rd.IsDBNull(6) ? (int?)null : rd.GetInt32(6),
                PurchaseId = rd.IsDBNull(7) ? (int?)null : rd.GetInt32(7),
                Notes = rd.IsDBNull(8) ? null : rd.GetString(8)
            };
        }
    }
}