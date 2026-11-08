using System;
using System.Collections.Generic;
using AccountingSystem.Data;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Repositories
{
    public class IncomeRepository
    {
        public List<Income> GetAll(DateTime? from, DateTime? to)
        {
            var list = new List<Income>();
            const string sql = @"
                SELECT IncomeId,Category,Description,Amount,IncomeDate,PaymentMethod,Notes
                FROM Income
                WHERE (@f IS NULL OR IncomeDate >= @f) AND (@t IS NULL OR IncomeDate <= @t)
                ORDER BY IncomeDate DESC";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@f", (object)from ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@t", (object)to ?? DBNull.Value);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new Income
                        {
                            IncomeId = rd.GetInt32(0),
                            Category = rd.GetString(1),
                            Description = rd.IsDBNull(2) ? null : rd.GetString(2),
                            Amount = rd.GetDecimal(3),
                            IncomeDate = rd.GetDateTime(4),
                            PaymentMethod = rd.IsDBNull(5) ? null : rd.GetString(5),
                            Notes = rd.IsDBNull(6) ? null : rd.GetString(6)
                        });
                    }
                }
            }
            return list;
        }

        public int Add(Income i)
        {
            const string sql = @"
                INSERT INTO Income(Category,Description,Amount,IncomeDate,PaymentMethod,Notes)
                VALUES(@c,@d,@a,@dt,@pm,@n); SELECT SCOPE_IDENTITY();";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@c", i.Category);
                cmd.Parameters.AddWithValue("@d", (object)i.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@a", i.Amount);
                cmd.Parameters.AddWithValue("@dt", i.IncomeDate);
                cmd.Parameters.AddWithValue("@pm", (object)i.PaymentMethod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@n", (object)i.Notes ?? DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("DELETE FROM Income WHERE IncomeId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}