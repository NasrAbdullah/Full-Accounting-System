using System;
using System.Collections.Generic;
using AccountingSystem.Data;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Repositories
{
    public class ExpenseRepository
    {
        public List<Expense> GetAll(DateTime? from, DateTime? to)
        {
            var list = new List<Expense>();
            const string sql = @"
                SELECT ExpenseId,Category,Description,Amount,ExpenseDate,PaymentMethod,Notes
                FROM Expenses
                WHERE (@f IS NULL OR ExpenseDate >= @f) AND (@t IS NULL OR ExpenseDate <= @t)
                ORDER BY ExpenseDate DESC";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@f", (object)from ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@t", (object)to ?? DBNull.Value);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new Expense
                        {
                            ExpenseId = rd.GetInt32(0),
                            Category = rd.GetString(1),
                            Description = rd.IsDBNull(2) ? null : rd.GetString(2),
                            Amount = rd.GetDecimal(3),
                            ExpenseDate = rd.GetDateTime(4),
                            PaymentMethod = rd.IsDBNull(5) ? null : rd.GetString(5),
                            Notes = rd.IsDBNull(6) ? null : rd.GetString(6)
                        });
                    }
                }
            }
            return list;
        }

        public int Add(Expense e)
        {
            const string sql = @"
                INSERT INTO Expenses(Category,Description,Amount,ExpenseDate,PaymentMethod,Notes)
                VALUES(@c,@d,@a,@dt,@pm,@n); SELECT SCOPE_IDENTITY();";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@c", e.Category);
                cmd.Parameters.AddWithValue("@d", (object)e.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@a", e.Amount);
                cmd.Parameters.AddWithValue("@dt", e.ExpenseDate);
                cmd.Parameters.AddWithValue("@pm", (object)e.PaymentMethod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@n", (object)e.Notes ?? DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("DELETE FROM Expenses WHERE ExpenseId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}