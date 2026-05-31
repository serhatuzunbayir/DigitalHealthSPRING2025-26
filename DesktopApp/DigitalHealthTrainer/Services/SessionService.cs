using System.Globalization;
using DigitalHealthTrainer.Data;
using DigitalHealthTrainer.Models;
using Npgsql;

namespace DigitalHealthTrainer.Services
{
    public class LiveColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is true ? Color.FromArgb("#ECFDF5") : Colors.White;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // Session display model for UI binding
    public class SessionDisplay
    {
        public int SessionId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime SessionTime { get; set; }
        public string SessionTimeDisplay => SessionTime.ToString("dd/MM/yyyy HH:mm");
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay => Status.Replace("_", " ").ToUpper();

        public Color StatusColor => Status switch
        {
            "completed" => Color.FromArgb("#059669"),
            "canceled"  => Color.FromArgb("#DC2626"),
            "active"    => Color.FromArgb("#10B981"),
            _           => Color.FromArgb("#D97706")
        };

        public bool IsLive => Status == "active";
    }

    public static class SessionService
    {
        // Trainer'ın tüm session'larını getir (client isimleriyle)
        public static List<SessionDisplay> GetSessionsByTrainer(int trainerId, List<Client> clients)
        {
            var sessions = new List<SessionDisplay>();

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = @"SELECT vs.session_id, vs.client_id, vs.session_time,
                                    vs.duration_minutes, vs.status
                             FROM virtual_sessions vs
                             WHERE vs.trainer_id = @trainerId
                             ORDER BY vs.session_time DESC";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@trainerId", trainerId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int clientId = reader.GetInt32(1);
                string clientName = clients.FirstOrDefault(c => c.ClientId == clientId)?.Username ?? "Unknown";

                sessions.Add(new SessionDisplay
                {
                    SessionId = reader.GetInt32(0),
                    ClientName = clientName,
                    SessionTime = reader.GetDateTime(2),
                    DurationMinutes = reader.GetInt32(3),
                    Status = reader.GetString(4)
                });
            }

            return sessions;
        }

        // Session durumunu güncelle
        public static void UpdateSessionStatus(int sessionId, string newStatus)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = "UPDATE virtual_sessions SET status = @status WHERE session_id = @sessionId";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@status", newStatus);
            cmd.Parameters.AddWithValue("@sessionId", sessionId);

            cmd.ExecuteNonQuery();
        }
    }
}
