namespace DigitalHealthTrainer.Models
{
    public class WorkoutProgram
    {
        public int ProgramId { get; set; }
        public int TrainerId { get; set; }
        public int ClientId { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
