namespace DigitalHealthTrainer.Models
{
    public class VirtualSession
    {
        public int SessionId { get; set; }
        public int ClientId { get; set; }
        public int TrainerId { get; set; }
        public DateTime SessionTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;         // scheduled, canceled, completed
    }
}
