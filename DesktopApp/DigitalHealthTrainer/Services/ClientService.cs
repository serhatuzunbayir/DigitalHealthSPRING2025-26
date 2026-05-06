using DigitalHealthTrainer.Data;
using DigitalHealthTrainer.Models;
using Npgsql;

namespace DigitalHealthTrainer.Services
{
    public static class ClientService
    {
        // Trainer'a atanmış tüm client'ları getir
        public static List<Client> GetAssignedClients(int trainerId)
        {
            var clients = new List<Client>();

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = @"SELECT c.client_id, c.username, c.email
                             FROM clients c
                             INNER JOIN assignments a ON c.client_id = a.client_id
                             WHERE a.trainer_id = @trainerId
                             ORDER BY c.username";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@trainerId", trainerId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                clients.Add(new Client
                {
                    ClientId = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2)
                });
            }

            return clients;
        }

        // Dashboard için özet veri — LINQ ile analiz
        public static List<ClientDashboardSummary> GetDashboardData(int trainerId)
        {
            var clients = GetAssignedClients(trainerId);
            var summaries = new List<ClientDashboardSummary>();

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            foreach (var client in clients)
            {
                // Exercise loglarını çek
                var exerciseLogs = new List<ExerciseLog>();
                string logQuery = @"SELECT log_id, exercise_type, duration_minutes, sets, reps,
                                           calories_burned, log_date
                                    FROM exercise_logs WHERE client_id = @clientId";

                using (var cmd = new NpgsqlCommand(logQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@clientId", client.ClientId);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        exerciseLogs.Add(new ExerciseLog
                        {
                            LogId = reader.GetInt32(0),
                            ClientId = client.ClientId,
                            ExerciseType = reader.GetString(1),
                            DurationMinutes = reader.GetInt32(2),
                            Sets = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            Reps = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                            CaloriesBurned = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                            LogDate = reader.GetDateTime(6)
                        });
                    }
                }

                // Fitness goal'ları çek
                var goals = new List<FitnessGoal>();
                string goalQuery = @"SELECT goal_id, goal_type, target_value, current_value,
                                            deadline, status
                                     FROM fitness_goals WHERE client_id = @clientId";

                using (var cmd = new NpgsqlCommand(goalQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@clientId", client.ClientId);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        goals.Add(new FitnessGoal
                        {
                            GoalId = reader.GetInt32(0),
                            ClientId = client.ClientId,
                            GoalType = reader.GetString(1),
                            TargetValue = reader.GetDecimal(2),
                            CurrentValue = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                            Deadline = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            Status = reader.IsDBNull(5) ? null : reader.GetString(5)
                        });
                    }
                }

                // ===== LINQ ANALİZ =====
                DateTime weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);

                // Son aktivite tarihi
                DateTime? lastActivity = exerciseLogs.Any()
                    ? exerciseLogs.Max(e => e.LogDate)
                    : null;

                // Bu haftaki egzersiz sayısı
                int weeklyCount = exerciseLogs
                    .Count(e => e.LogDate >= weekStart && e.LogDate <= DateTime.Today);

                // Adherence rate (tamamlanan hedef / toplam hedef * 100)
                double adherenceRate = goals.Any()
                    ? goals.Count(g => g.Status == "completed") * 100.0 / goals.Count()
                    : 0;

                // Aktif hedef sayısı
                int activeGoals = goals.Count(g => g.Status == "in_progress");

                summaries.Add(new ClientDashboardSummary
                {
                    ClientId = client.ClientId,
                    Username = client.Username,
                    Email = client.Email,
                    LastActivity = lastActivity,
                    WeeklyExerciseCount = weeklyCount,
                    AdherenceRate = Math.Round(adherenceRate, 1),
                    ActiveGoals = activeGoals
                });
            }

            return summaries;
        }
    }
}
