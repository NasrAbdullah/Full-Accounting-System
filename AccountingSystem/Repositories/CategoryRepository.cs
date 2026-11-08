using System;
using System.Collections.Generic;
using AccountingSystem.Data;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Repositories
{
    public class CategoryRepository
    {
        public List<Category> GetAll(string search)
        {
            var list = new List<Category>();
            const string sql = @"
                SELECT c.CategoryId, c.Name, c.Notes, c.CreatedAt,
                       (SELECT COUNT(1) FROM Products p WHERE p.CategoryId=c.CategoryId) AS ProductCount
                FROM Categories c
                WHERE (@s IS NULL OR c.Name LIKE '%'+@s+'%')
                ORDER BY c.CategoryId DESC";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@s", (object)search ?? DBNull.Value);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new Category
                        {
                            CategoryId = rd.GetInt32(0),
                            Name = rd.GetString(1),
                            Notes = rd.IsDBNull(2) ? null : rd.GetString(2),
                            CreatedAt = rd.GetDateTime(3),
                            ProductCount = rd.GetInt32(4)
                        });
                    }
                }
            }
            return list;
        }

        public int Add(Category c)
        {
            const string sql = "INSERT INTO Categories(Name,Notes) VALUES(@n,@no); SELECT SCOPE_IDENTITY();";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@n", c.Name);
                cmd.Parameters.AddWithValue("@no", (object)c.Notes ?? DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Category c)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("UPDATE Categories SET Name=@n, Notes=@no WHERE CategoryId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@n", c.Name);
                cmd.Parameters.AddWithValue("@no", (object)c.Notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", c.CategoryId);
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("DELETE FROM Categories WHERE CategoryId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public bool HasProducts(int id)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Products WHERE CategoryId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}