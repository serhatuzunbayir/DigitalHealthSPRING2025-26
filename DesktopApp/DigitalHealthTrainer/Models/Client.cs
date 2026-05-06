namespace DigitalHealthTrainer.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
