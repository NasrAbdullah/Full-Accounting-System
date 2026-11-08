using System;
using System.Collections.Generic;
using AccountingSystem.Data;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Repositories
{
    public class SupplierRepository
    {
        public List<Supplier> GetAll(string search)
        {
            var list = new List<Supplier>();
            const string sql = @"
                SELECT s.SupplierId, s.Name, s.Phone, s.Address, s.Email,
                       s.OpeningBalance, s.Notes, s.IsActive, s.CreatedAt, s.UpdatedAt,
                       ISNULL((SELECT SUM(GrandTotal) FROM Purchases p WHERE p.SupplierId=s.SupplierId),0),
                       ISNULL((SELECT SUM(Paid) FROM Purchases p WHERE p.SupplierId=s.SupplierId),0)
                FROM Suppliers s
                WHERE (@s IS NULL OR s.Name LIKE '%'+@s+'%' OR ISNULL(s.Phone,'') LIKE '%'+@s+'%')
                ORDER BY s.SupplierId DESC";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@s", (object)search ?? DBNull.Value);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read()) list.Add(Map(rd));
                }
            }
            return list;
        }

        public Supplier GetById(int id)
        {
            const string sql = @"
                SELECT s.SupplierId, s.Name, s.Phone, s.Address, s.Email,
                       s.OpeningBalance, s.Notes, s.IsActive, s.CreatedAt, s.UpdatedAt,
                       ISNULL((SELECT SUM(GrandTotal) FROM Purchases p WHERE p.SupplierId=s.SupplierId),0),
                       ISNULL((SELECT SUM(Paid) FROM Purchases p WHERE p.SupplierId=s.SupplierId),0)
                FROM Suppliers s WHERE s.SupplierId=@id";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read()) return Map(rd);
                }
            }
            return null;
        }

        public int Add(Supplier s)
        {
            const string sql = @"
                INSERT INTO Suppliers(Name,Phone,Address,Email,OpeningBalance,Notes,IsActive)
                VALUES(@n,@p,@a,@e,@ob,@no,@ac);
                SELECT SCOPE_IDENTITY();";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                AddParams(cmd, s);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Supplier s)
        {
            const string sql = @"
                UPDATE Suppliers SET Name=@n,Phone=@p,Address=@a,Email=@e,
                       OpeningBalance=@ob,Notes=@no,IsActive=@ac,UpdatedAt=GETDATE()
                WHERE SupplierId=@id";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                AddParams(cmd, s);
                cmd.Parameters.AddWithValue("@id", s.SupplierId);
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("DELETE FROM Suppliers WHERE SupplierId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public bool HasPurchases(int id)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Purchases WHERE SupplierId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public List<Supplier> GetComboList()
        {
            var list = new List<Supplier>();
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(
                "SELECT SupplierId, Name FROM Suppliers WHERE IsActive=1 ORDER BY Name", conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    list.Add(new Supplier
                    {
                        SupplierId = rd.GetInt32(0),
                        Name = rd.GetString(1)
                    });
                }
            }
            return list;
        }

        private static void AddParams(SqlCommand cmd, Supplier s)
        {
            cmd.Parameters.AddWithValue("@n", s.Name);
            cmd.Parameters.AddWithValue("@p", (object)s.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@a", (object)s.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@e", (object)s.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ob", s.OpeningBalance);
            cmd.Parameters.AddWithValue("@no", (object)s.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ac", s.IsActive);
        }

        private static Supplier Map(SqlDataReader rd)
        {
            return new Supplier
            {
                SupplierId = rd.GetInt32(0),
                Name = rd.GetString(1),
                Phone = rd.IsDBNull(2) ? null : rd.GetString(2),
                Address = rd.IsDBNull(3) ? null : rd.GetString(3),
                Email = rd.IsDBNull(4) ? null : rd.GetString(4),
                OpeningBalance = rd.GetDecimal(5),
                Notes = rd.IsDBNull(6) ? null : rd.GetString(6),
                IsActive = rd.GetBoolean(7),
                CreatedAt = rd.GetDateTime(8),
                UpdatedAt = rd.IsDBNull(9) ? (DateTime?)null : rd.GetDateTime(9),
                TotalPurchases = rd.GetDecimal(10),
                TotalPaid = rd.GetDecimal(11),
                Balance = rd.GetDecimal(5) + rd.GetDecimal(10) - rd.GetDecimal(11)
            };
        }
    }
}