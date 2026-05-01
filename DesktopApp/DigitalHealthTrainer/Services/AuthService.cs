using System.Security.Cryptography;
using System.Text;
using DigitalHealthTrainer.Data;
using DigitalHealthTrainer.Models;
using Npgsql;

namespace DigitalHealthTrainer.Services
{
    public static class AuthService
    {
        // SHA256 hash - requirement'ta belirtildiği gibi
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        // Trainer girişi - sadece trainer tablosundan doğrulama
        public static Trainer? Login(string username, string password)
        {
            string hashedPassword = HashPassword(password);

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = @"SELECT trainer_id, username, password_hash, email
                             FROM trainers
                             WHERE username = @username AND password_hash = @hash";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", hashedPassword);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Trainer
                {
                    TrainerId = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Email = reader.GetString(3)
                };
            }

            return null;
        }

        // Trainer kaydı
        public static bool Register(string username, string password, string email)
        {
            string hashedPassword = HashPassword(password);

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                string query = @"INSERT INTO trainers (username, password_hash, email)
                                 VALUES (@username, @hash, @email)";

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@hash", hashedPassword);
                cmd.Parameters.AddWithValue("@email", email);

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // unique violation
            {
                return false;
            }
        }
    }
}
