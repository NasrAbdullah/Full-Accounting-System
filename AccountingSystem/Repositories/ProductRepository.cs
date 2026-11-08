using System;
using System.Collections.Generic;
using AccountingSystem.Data;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Repositories
{
    public class ProductRepository
    {
        public List<Product> GetAll(string search)
        {
            var list = new List<Product>();
            const string sql = @"
                SELECT p.ProductId, p.ProductCode, p.Name, p.CategoryId, c.Name AS CategoryName,
                       p.PurchasePrice, p.SalePrice, p.Quantity, p.MinimumStock, p.Unit,
                       p.Barcode, p.Description, p.IsActive, p.CreatedAt, p.UpdatedAt
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE (@s IS NULL OR p.Name LIKE '%'+@s+'%' OR p.ProductCode LIKE '%'+@s+'%' 
                       OR ISNULL(p.Barcode,'') LIKE '%'+@s+'%')
                ORDER BY p.ProductId DESC";
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

        public Product GetById(int id)
        {
            const string sql = @"
                SELECT p.ProductId, p.ProductCode, p.Name, p.CategoryId, c.Name,
                       p.PurchasePrice, p.SalePrice, p.Quantity, p.MinimumStock, p.Unit,
                       p.Barcode, p.Description, p.IsActive, p.CreatedAt, p.UpdatedAt
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId=c.CategoryId
                WHERE p.ProductId=@id";
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

        public int Add(Product p)
        {
            const string sql = @"
                INSERT INTO Products(ProductCode,Name,CategoryId,PurchasePrice,SalePrice,
                                     Quantity,MinimumStock,Unit,Barcode,Description,IsActive)
                VALUES(@code,@n,@cat,@pp,@sp,@q,@min,@u,@b,@d,@ac);
                SELECT SCOPE_IDENTITY();";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                AddParams(cmd, p);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Product p)
        {
            const string sql = @"
                UPDATE Products SET ProductCode=@code, Name=@n, CategoryId=@cat,
                       PurchasePrice=@pp, SalePrice=@sp, MinimumStock=@min,
                       Unit=@u, Barcode=@b, Description=@d, IsActive=@ac, UpdatedAt=GETDATE()
                WHERE ProductId=@id";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                AddParams(cmd, p);
                cmd.Parameters.AddWithValue("@id", p.ProductId);
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("DELETE FROM Products WHERE ProductId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public bool HasTransactions(int id)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(
                "SELECT (SELECT COUNT(1) FROM SaleItems WHERE ProductId=@id) + " +
                "(SELECT COUNT(1) FROM PurchaseItems WHERE ProductId=@id)", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public bool CodeExists(string code, int excludeId)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Products WHERE ProductCode=@c AND ProductId<>@id", conn))
            {
                cmd.Parameters.AddWithValue("@c", code);
                cmd.Parameters.AddWithValue("@id", excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public List<Product> GetLowStock()
        {
            var list = new List<Product>();
            const string sql = @"
                SELECT p.ProductId, p.ProductCode, p.Name, p.CategoryId, c.Name,
                       p.PurchasePrice, p.SalePrice, p.Quantity, p.MinimumStock, p.Unit,
                       p.Barcode, p.Description, p.IsActive, p.CreatedAt, p.UpdatedAt
                FROM Products p LEFT JOIN Categories c ON p.CategoryId=c.CategoryId
                WHERE p.IsActive=1 AND p.Quantity <= p.MinimumStock
                ORDER BY p.Quantity ASC";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read()) list.Add(Map(rd));
            }
            return list;
        }

        public List<Product> GetActiveList()
        {
            var list = new List<Product>();
            const string sql = @"
                SELECT p.ProductId, p.ProductCode, p.Name, p.CategoryId, c.Name,
                       p.PurchasePrice, p.SalePrice, p.Quantity, p.MinimumStock, p.Unit,
                       p.Barcode, p.Description, p.IsActive, p.CreatedAt, p.UpdatedAt
                FROM Products p LEFT JOIN Categories c ON p.CategoryId=c.CategoryId
                WHERE p.IsActive=1 ORDER BY p.Name";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read()) list.Add(Map(rd));
            }
            return list;
        }

        private static void AddParams(SqlCommand cmd, Product p)
        {
            cmd.Parameters.AddWithValue("@code", p.ProductCode);
            cmd.Parameters.AddWithValue("@n", p.Name);
            cmd.Parameters.AddWithValue("@cat", (object)p.CategoryId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pp", p.PurchasePrice);
            cmd.Parameters.AddWithValue("@sp", p.SalePrice);
            cmd.Parameters.AddWithValue("@q", p.Quantity);
            cmd.Parameters.AddWithValue("@min", p.MinimumStock);
            cmd.Parameters.AddWithValue("@u", (object)p.Unit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@b", (object)p.Barcode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@d", (object)p.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ac", p.IsActive);
        }

        private static Product Map(SqlDataReader rd)
        {
            return new Product
            {
                ProductId = rd.GetInt32(0),
                ProductCode = rd.GetString(1),
                Name = rd.GetString(2),
                CategoryId = rd.IsDBNull(3) ? (int?)null : rd.GetInt32(3),
                CategoryName = rd.IsDBNull(4) ? null : rd.GetString(4),
                PurchasePrice = rd.GetDecimal(5),
                SalePrice = rd.GetDecimal(6),
                Quantity = rd.GetDecimal(7),
                MinimumStock = rd.GetDecimal(8),
                Unit = rd.IsDBNull(9) ? null : rd.GetString(9),
                Barcode = rd.IsDBNull(10) ? null : rd.GetString(10),
                Description = rd.IsDBNull(11) ? null : rd.GetString(11),
                IsActive = rd.GetBoolean(12),
                CreatedAt = rd.GetDateTime(13),
                UpdatedAt = rd.IsDBNull(14) ? (DateTime?)null : rd.GetDateTime(14)
            };
        }
    }
}