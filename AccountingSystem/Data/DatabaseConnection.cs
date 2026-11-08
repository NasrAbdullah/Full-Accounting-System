using System.Data.SqlClient;

namespace AccountingSystem.Data
{
    public static class DatabaseConnection
    {
        private const string ServerName = @".";
        private const string DatabaseName = "AccountingDB";

        private static readonly string MasterConnectionString =
            "Server=" + ServerName + ";Database=master;Integrated Security=True;";

        private static readonly string ConnectionString =
            "Server=" + ServerName + ";Database=" + DatabaseName + ";Integrated Security=True;";

        public static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static SqlConnection GetMasterConnection()
        {
            var conn = new SqlConnection(MasterConnectionString);
            conn.Open();
            return conn;
        }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    return conn.State == System.Data.ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}