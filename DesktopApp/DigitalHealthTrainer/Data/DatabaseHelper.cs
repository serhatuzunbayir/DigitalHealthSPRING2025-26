using Npgsql;

namespace DigitalHealthTrainer.Data
{
    public static class DatabaseHelper
    {
        private static string _connectionString = string.Empty;

        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static NpgsqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("DatabaseHelper has not been initialized. Call Initialize() first.");
            return new NpgsqlConnection(_connectionString);
        }

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
