namespace DigitalHealthTrainer.Models
{
    public class ClientDashboardSummary
    {
        public int ClientId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? LastActivity { get; set; }
        public int WeeklyExerciseCount { get; set; }
        public double AdherenceRate { get; set; }
        public int ActiveGoals { get; set; }

        // DataGrid display properties
        public string LastActivityDisplay => LastActivity?.ToString("dd/MM/yyyy") ?? "-";
        public string AdherenceDisplay => $"{AdherenceRate}%";
    }
}
