using DigitalHealthTrainer.Data;
using DigitalHealthTrainer.Models;
using Npgsql;

namespace DigitalHealthTrainer.Services
{
    public static class AssignmentService
    {
        public static List<Client> GetAllClients()
        {
            var clients = new List<Client>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string query = "SELECT client_id, username, email FROM clients ORDER BY username";
            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                clients.Add(new Client { ClientId = reader.GetInt32(0), Username = reader.GetString(1), Email = reader.GetString(2) });
            return clients;
        }

        public static List<Client> GetUnassignedClients(int trainerId)
        {
            var clients = new List<Client>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string query = @"SELECT c.client_id, c.username, c.email
                                   FROM clients c
                                   WHERE NOT EXISTS (
                                       SELECT 1 FROM assignments a
                                       WHERE a.client_id = c.client_id AND a.trainer_id = @trainerId
                                   )
                                   ORDER BY c.username";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@trainerId", trainerId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                clients.Add(new Client { ClientId = reader.GetInt32(0), Username = reader.GetString(1), Email = reader.GetString(2) });
            return clients;
        }

        public static void AssignClient(int trainerId, int clientId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string query = @"INSERT INTO assignments (trainer_id, client_id)
                                   VALUES (@trainerId, @clientId)
                                   ON CONFLICT DO NOTHING";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@trainerId", trainerId);
            cmd.Parameters.AddWithValue("@clientId", clientId);
            cmd.ExecuteNonQuery();
        }

        public static void UnassignClient(int trainerId, int clientId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string query = "DELETE FROM assignments WHERE trainer_id = @trainerId AND client_id = @clientId";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@trainerId", trainerId);
            cmd.Parameters.AddWithValue("@clientId", clientId);
            cmd.ExecuteNonQuery();
        }
    }
}
