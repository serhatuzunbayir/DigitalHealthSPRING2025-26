namespace DigitalHealthTrainer.Models
{
    public class Trainer
    {
        public int TrainerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
