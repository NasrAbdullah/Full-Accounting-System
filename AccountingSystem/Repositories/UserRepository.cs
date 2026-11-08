using System;
using AccountingSystem.Data;
using AccountingSystem.Models;
using System.Data.SqlClient;

namespace AccountingSystem.Repositories
{
    public class UserRepository
    {
        public User GetByUsername(string username)
        {
            const string sql = @"
                SELECT u.UserId, u.Username, u.PasswordHash, u.FullName,
                       u.RoleId, r.Name AS RoleName, u.IsActive, u.CreatedAt
                FROM Users u JOIN Roles r ON u.RoleId = r.RoleId
                WHERE u.Username = @u";
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@u", username);
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    return new User
                    {
                        UserId = rd.GetInt32(0),
                        Username = rd.GetString(1),
                        PasswordHash = rd.GetString(2),
                        FullName = rd.GetString(3),
                        RoleId = rd.GetInt32(4),
                        RoleName = rd.GetString(5),
                        IsActive = rd.GetBoolean(6),
                        CreatedAt = rd.GetDateTime(7)
                    };
                }
            }
        }

        public void UpdatePassword(int userId, string passwordHash)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(
                "UPDATE Users SET PasswordHash=@h, UpdatedAt=GETDATE() WHERE UserId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@h", passwordHash);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}