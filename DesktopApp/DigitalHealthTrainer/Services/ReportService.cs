using System.Globalization;
using DigitalHealthTrainer.Data;
using DigitalHealthTrainer.Models;
using Npgsql;

namespace DigitalHealthTrainer.Services
{
    // DataGrid binding modelleri
    public class WeeklySummary
    {
        public int WeekNumber { get; set; }
        public string WeekLabel => $"Week {WeekNumber}";
        public double AvgDuration { get; set; }
        public double AvgCalories { get; set; }
        public int TotalSessions { get; set; }
        public string ExerciseTypes { get; set; } = string.Empty;
    }

    public class HealthMetricDisplay
    {
        public DateTime RecordDate { get; set; }
        public string DateDisplay => RecordDate.ToString("dd/MM/yyyy");
        public decimal? Weight { get; set; }
        public string WeightDisplay => Weight?.ToString("F1") ?? "-";
        public int? HeartRate { get; set; }
        public string HeartRateDisplay => HeartRate?.ToString() ?? "-";
        public decimal? Sleep { get; set; }
        public string SleepDisplay => Sleep?.ToString("F1") ?? "-";
        public decimal? Water { get; set; }
        public string WaterDisplay => Water?.ToString("F1") ?? "-";
    }

    public class GoalDisplay
    {
        public string GoalType { get; set; } = string.Empty;
        public string GoalTypeDisplay => GoalType.Replace("_", " ").ToUpper();
        public decimal TargetValue { get; set; }
        public decimal? CurrentValue { get; set; }
        public string CurrentValueDisplay => CurrentValue?.ToString("F1") ?? "-";
        public DateTime? Deadline { get; set; }
        public string DeadlineDisplay => Deadline?.ToString("dd/MM/yyyy") ?? "-";
        public string? Status { get; set; }
        public string StatusDisplay => Status?.Replace("_", " ").ToUpper() ?? "-";
    }

    public static class ReportService
    {
        // ===== VERİ ÇEKME =====

        public static List<ExerciseLog> GetExerciseLogs(int clientId)
        {
            var logs = new List<ExerciseLog>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = "SELECT log_id, exercise_type, duration_minutes, sets, reps, calories_burned, log_date FROM exercise_logs WHERE client_id = @clientId";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                logs.Add(new ExerciseLog
                {
                    LogId = reader.GetInt32(0),
                    ClientId = clientId,
                    ExerciseType = reader.GetString(1),
                    DurationMinutes = reader.GetInt32(2),
                    Sets = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Reps = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    CaloriesBurned = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    LogDate = reader.GetDateTime(6)
                });
            }
            return logs;
        }

        public static List<HealthMetric> GetHealthMetrics(int clientId)
        {
            var metrics = new List<HealthMetric>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = "SELECT metric_id, weight_kg, heart_rate_bpm, sleep_hours, water_intake_liters, record_date FROM health_metrics WHERE client_id = @clientId";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                metrics.Add(new HealthMetric
                {
                    MetricId = reader.GetInt32(0),
                    ClientId = clientId,
                    WeightKg = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                    HeartRateBpm = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    SleepHours = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    WaterIntakeLiters = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    RecordDate = reader.GetDateTime(5)
                });
            }
            return metrics;
        }

        public static List<FitnessGoal> GetGoals(int clientId)
        {
            var goals = new List<FitnessGoal>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = "SELECT goal_id, goal_type, target_value, current_value, deadline, status FROM fitness_goals WHERE client_id = @clientId";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                goals.Add(new FitnessGoal
                {
                    GoalId = reader.GetInt32(0),
                    ClientId = clientId,
                    GoalType = reader.GetString(1),
                    TargetValue = reader.GetDecimal(2),
                    CurrentValue = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Deadline = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Status = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }
            return goals;
        }

        public static List<string> GetDistinctExerciseTypes(int clientId)
        {
            var types = new List<string>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = "SELECT DISTINCT exercise_type FROM exercise_logs WHERE client_id = @clientId ORDER BY exercise_type";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) types.Add(reader.GetString(0));
            return types;
        }

        // ===== LINQ ANALİZ =====

        // Haftalık ortalamalar
        public static List<WeeklySummary> GetWeeklyAverages(List<ExerciseLog> logs, DateTime from, DateTime to, string? exerciseType)
        {
            var filtered = logs.Where(l => l.LogDate >= from && l.LogDate <= to);

            if (!string.IsNullOrEmpty(exerciseType))
                filtered = filtered.Where(l => l.ExerciseType == exerciseType);

            var result = filtered
                .GroupBy(l => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    l.LogDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .Select(g => new WeeklySummary
                {
                    WeekNumber = g.Key,
                    AvgDuration = Math.Round(g.Average(e => e.DurationMinutes), 1),
                    AvgCalories = Math.Round(g.Average(e => e.CaloriesBurned ?? 0), 1),
                    TotalSessions = g.Count(),
                    ExerciseTypes = string.Join(", ", g.Select(e => e.ExerciseType).Distinct())
                })
                .OrderBy(w => w.WeekNumber)
                .ToList();

            return result;
        }

        // Sağlık metrikleri (tarih aralığına göre LINQ filtreleme)
        public static List<HealthMetricDisplay> GetFilteredHealthMetrics(List<HealthMetric> metrics, DateTime from, DateTime to)
        {
            return metrics
                .Where(m => m.RecordDate >= from && m.RecordDate <= to)
                .OrderByDescending(m => m.RecordDate)
                .Select(m => new HealthMetricDisplay
                {
                    RecordDate = m.RecordDate,
                    Weight = m.WeightKg,
                    HeartRate = m.HeartRateBpm,
                    Sleep = m.SleepHours,
                    Water = m.WaterIntakeLiters
                })
                .ToList();
        }

        // Goal analizi
        public static List<GoalDisplay> GetFilteredGoals(List<FitnessGoal> goals, string? category)
        {
            var filtered = goals.AsEnumerable();

            if (!string.IsNullOrEmpty(category))
                filtered = filtered.Where(g => g.GoalType == category);

            return filtered
                .OrderBy(g => g.Status)
                .Select(g => new GoalDisplay
                {
                    GoalType = g.GoalType,
                    TargetValue = g.TargetValue,
                    CurrentValue = g.CurrentValue,
                    Deadline = g.Deadline,
                    Status = g.Status
                })
                .ToList();
        }

        // Goal tamamlanma istatistikleri
        public static (int total, int completed, int inProgress, int missed, double rate) GetGoalStats(List<FitnessGoal> goals, string? category)
        {
            var filtered = goals.AsEnumerable();
            if (!string.IsNullOrEmpty(category))
                filtered = filtered.Where(g => g.GoalType == category);

            var list = filtered.ToList();
            int total = list.Count;
            int completed = list.Count(g => g.Status == "completed");
            int inProgress = list.Count(g => g.Status == "in_progress");
            int missed = list.Count(g => g.Status == "missed");
            double rate = total > 0 ? Math.Round(completed * 100.0 / total, 1) : 0;

            return (total, completed, inProgress, missed, rate);
        }
    }
}
