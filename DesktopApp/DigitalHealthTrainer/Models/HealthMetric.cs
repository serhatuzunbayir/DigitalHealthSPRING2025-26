namespace DigitalHealthTrainer.Models
{
    public class HealthMetric
    {
        public int MetricId { get; set; }
        public int ClientId { get; set; }
        public decimal? WeightKg { get; set; }
        public int? HeartRateBpm { get; set; }
        public decimal? SleepHours { get; set; }
        public decimal? WaterIntakeLiters { get; set; }
        public DateTime RecordDate { get; set; }
    }
}
