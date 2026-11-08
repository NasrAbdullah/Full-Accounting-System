using System;
using System.Collections.Generic;
using AccountingSystem.Data;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Repositories
{
    public class CustomerRepository
    {
        public List<Customer> GetAll(string search)
        {
            var list = new List<Customer>();
            const string sql = @"
                SELECT c.CustomerId, c.Name, c.Phone, c.Address, c.Email,
                       c.OpeningBalance, c.Notes, c.IsActive, c.CreatedAt, c.UpdatedAt,
                       ISNULL((SELECT SUM(GrandTotal) FROM Sales s WHERE s.CustomerId=c.CustomerId),0) AS TotalSales,
                       ISNULL((SELECT SUM(Paid) FROM Sales s WHERE s.CustomerId=c.CustomerId),0) AS TotalPaid
                FROM Customers c
                WHERE (@s IS NULL OR c.Name LIKE '%'+@s+'%' OR ISNULL(c.Phone,'') LIKE '%'+@s+'%' 
                       OR ISNULL(c.Email,'') LIKE '%'+@s+'%')
                ORDER BY c.CustomerId DESC";

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

        public Customer GetById(int id)
        {
            const string sql = @"
                SELECT c.CustomerId, c.Name, c.Phone, c.Address, c.Email,
                       c.OpeningBalance, c.Notes, c.IsActive, c.CreatedAt, c.UpdatedAt,
                       ISNULL((SELECT SUM(GrandTotal) FROM Sales s WHERE s.CustomerId=c.CustomerId),0),
                       ISNULL((SELECT SUM(Paid) FROM Sales s WHERE s.CustomerId=c.CustomerId),0)
                FROM Customers c WHERE c.CustomerId=@id";
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

        public int Add(Customer c)
        {
            const string sql = @"
                INSERT INTO Customers(Name, Phone, Address, Email, OpeningBalance, Notes, IsActive)
                VALUES(@n, @p, @a, @e, @ob, @no, @ac);
                SELECT SCOPE_IDENTITY();";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                AddParams(cmd, c);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Customer c)
        {
            const string sql = @"
                UPDATE Customers SET Name=@n, Phone=@p, Address=@a, Email=@e,
                       OpeningBalance=@ob, Notes=@no, IsActive=@ac, UpdatedAt=GETDATE()
                WHERE CustomerId=@id";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                AddParams(cmd, c);
                cmd.Parameters.AddWithValue("@id", c.CustomerId);
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("DELETE FROM Customers WHERE CustomerId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public bool HasSales(int customerId)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Sales WHERE CustomerId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", customerId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public List<Customer> GetComboList()
        {
            var list = new List<Customer>();
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(
                "SELECT CustomerId, Name FROM Customers WHERE IsActive=1 ORDER BY Name", conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    list.Add(new Customer
                    {
                        CustomerId = rd.GetInt32(0),
                        Name = rd.GetString(1)
                    });
                }
            }
            return list;
        }

        private static void AddParams(SqlCommand cmd, Customer c)
        {
            cmd.Parameters.AddWithValue("@n", c.Name);
            cmd.Parameters.AddWithValue("@p", (object)c.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@a", (object)c.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@e", (object)c.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ob", c.OpeningBalance);
            cmd.Parameters.AddWithValue("@no", (object)c.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ac", c.IsActive);
        }

        private static Customer Map(SqlDataReader rd)
        {
            return new Customer
            {
                CustomerId = rd.GetInt32(0),
                Name = rd.GetString(1),
                Phone = rd.IsDBNull(2) ? null : rd.GetString(2),
                Address = rd.IsDBNull(3) ? null : rd.GetString(3),
                Email = rd.IsDBNull(4) ? null : rd.GetString(4),
                OpeningBalance = rd.GetDecimal(5),
                Notes = rd.IsDBNull(6) ? null : rd.GetString(6),
                IsActive = rd.GetBoolean(7),
                CreatedAt = rd.GetDateTime(8),
                UpdatedAt = rd.IsDBNull(9) ? (DateTime?)null : rd.GetDateTime(9),
                TotalSales = rd.GetDecimal(10),
                TotalPaid = rd.GetDecimal(11),
                Balance = rd.GetDecimal(5) + rd.GetDecimal(10) - rd.GetDecimal(11)
            };
        }
    }
}