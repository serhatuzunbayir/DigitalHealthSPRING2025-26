using DigitalHealthTrainer.Data;
using DigitalHealthTrainer.Models;
using Npgsql;

namespace DigitalHealthTrainer.Services
{
    public static class WorkoutService
    {
        // Yeni workout programı oluştur
        public static void CreateProgram(int trainerId, int clientId, string programName, string description)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = @"INSERT INTO workout_programs (trainer_id, client_id, program_name, description, created_date)
                             VALUES (@trainerId, @clientId, @name, @desc, @date)";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@trainerId", trainerId);
            cmd.Parameters.AddWithValue("@clientId", clientId);
            cmd.Parameters.AddWithValue("@name", programName);
            cmd.Parameters.AddWithValue("@desc", (object?)description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@date", DateTime.Now);

            cmd.ExecuteNonQuery();
        }

        // Trainer'ın belirli bir client için oluşturduğu programları getir
        public static List<WorkoutProgram> GetProgramsByClient(int trainerId, int clientId)
        {
            var programs = new List<WorkoutProgram>();

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = @"SELECT program_id, trainer_id, client_id, program_name, description, created_date
                             FROM workout_programs
                             WHERE trainer_id = @trainerId AND client_id = @clientId
                             ORDER BY created_date DESC";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@trainerId", trainerId);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                programs.Add(new WorkoutProgram
                {
                    ProgramId = reader.GetInt32(0),
                    TrainerId = reader.GetInt32(1),
                    ClientId = reader.GetInt32(2),
                    ProgramName = reader.GetString(3),
                    Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                    CreatedDate = reader.GetDateTime(5)
                });
            }

            return programs;
        }

        // Program sil
        public static void DeleteProgram(int programId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = "DELETE FROM workout_programs WHERE program_id = @programId";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@programId", programId);

            cmd.ExecuteNonQuery();
        }
    }
}
