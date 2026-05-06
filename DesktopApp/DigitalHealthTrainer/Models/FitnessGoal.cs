namespace DigitalHealthTrainer.Models
{
    public class FitnessGoal
    {
        public int GoalId { get; set; }
        public int ClientId { get; set; }
        public string GoalType { get; set; } = string.Empty;       // weight_loss, strength_target, weekly_exercise_frequency
        public decimal TargetValue { get; set; }
        public decimal? CurrentValue { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Status { get; set; }                         // completed, in_progress, missed
    }
}
