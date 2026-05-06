using Npgsql;

namespace DigitalHealthTrainer.Data
{
    public static class DatabaseHelper
    {
        private static readonly string _connectionString =
            "Host=92.249.61.114;Port=5432;Database=postgres;Username=admin;Password=secret123";

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        // Bağlantıyı test etmek için
        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
