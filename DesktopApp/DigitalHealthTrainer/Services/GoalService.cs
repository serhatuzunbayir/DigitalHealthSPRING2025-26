using DigitalHealthTrainer.Data;
using DigitalHealthTrainer.Models;
using Npgsql;

namespace DigitalHealthTrainer.Services
{
    public static class GoalService
    {
        public static List<FitnessGoal> GetGoalsForClient(int clientId)
        {
            var goals = new List<FitnessGoal>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            const string query = @"SELECT goal_id, goal_type, target_value, current_value, deadline, status
                                   FROM fitness_goals WHERE client_id = @clientId
                                   ORDER BY goal_id DESC";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                goals.Add(new FitnessGoal
                {
                    GoalId       = reader.GetInt32(0),
                    ClientId     = clientId,
                    GoalType     = reader.GetString(1),
                    TargetValue  = reader.GetDecimal(2),
                    CurrentValue = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Deadline     = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Status       = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }
            return goals;
        }

        public static void CreateGoal(int clientId, string goalType, decimal targetValue, DateTime? deadline)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            var hasDeadline = deadline.HasValue;
            var query = hasDeadline
                ? "INSERT INTO fitness_goals (client_id, goal_type, target_value, status, deadline) VALUES (@cid, @type, @target, 'in_progress', @dl)"
                : "INSERT INTO fitness_goals (client_id, goal_type, target_value, status) VALUES (@cid, @type, @target, 'in_progress')";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@cid",    clientId);
            cmd.Parameters.AddWithValue("@type",   goalType);
            cmd.Parameters.AddWithValue("@target", targetValue);
            if (hasDeadline)
                cmd.Parameters.AddWithValue("@dl", deadline!.Value.Date);

            cmd.ExecuteNonQuery();
        }

        public static void DeleteGoal(int goalId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM fitness_goals WHERE goal_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", goalId);
            cmd.ExecuteNonQuery();
        }
    }
}
