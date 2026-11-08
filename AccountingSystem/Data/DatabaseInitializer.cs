using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AccountingSystem.Data
{
    public static class DatabaseInitializer
    {
        public static void EnsureDatabase()
        {
            using (var master = DatabaseConnection.GetMasterConnection())
            using (var cmd = new SqlCommand(
                "IF DB_ID('AccountingDB') IS NULL CREATE DATABASE AccountingDB;", master))
            {
                cmd.ExecuteNonQuery();
            }

            string script = LoadEmbeddedScript("AccountingSystem.Data.CreateDatabase.sql");
            ExecuteScript(script);
        }

        private static string LoadEmbeddedScript(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException("Embedded resource '" + resourceName + "' not found.");
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static void ExecuteScript(string script)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                var batches = Regex.Split(
                    script,
                    @"^\s*GO\s*$",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);

                foreach (var batch in batches)
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    using (var cmd = new SqlCommand(batch, conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}