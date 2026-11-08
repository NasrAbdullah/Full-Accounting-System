using System;
using AccountingSystem.Data;
using System.Data.SqlClient;

namespace AccountingSystem.Helpers
{
    public static class InvoiceNumberGenerator
    {
        public static string NextSale()
        {
            return Next("Sales", "INV");
        }

        public static string NextPurchase()
        {
            return Next("Purchases", "PUR");
        }

        private static string Next(string table, string prefix)
        {
            string year = DateTime.Now.Year.ToString();
            string like = prefix + "-" + year + "-%";

            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand(
                "SELECT TOP 1 InvoiceNumber FROM " + table +
                " WHERE InvoiceNumber LIKE @p ORDER BY InvoiceNumber DESC", conn))
            {
                cmd.Parameters.AddWithValue("@p", like);

                object last = cmd.ExecuteScalar();
                int nextNum = 1;

                if (last != null && last != DBNull.Value)
                {
                    var parts = last.ToString().Split('-');
                    int n;
                    if (parts.Length == 3 && int.TryParse(parts[2], out n))
                        nextNum = n + 1;
                }

                return prefix + "-" + year + "-" + nextNum.ToString("D6");
            }
        }
    }
}