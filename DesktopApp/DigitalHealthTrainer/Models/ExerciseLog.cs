namespace DigitalHealthTrainer.Models
{
    public class ExerciseLog
    {
        public int LogId { get; set; }
        public int ClientId { get; set; }
        public string ExerciseType { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public int? CaloriesBurned { get; set; }
        public DateTime LogDate { get; set; }
    }
}
