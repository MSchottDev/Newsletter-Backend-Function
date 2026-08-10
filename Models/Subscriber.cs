namespace Newsletter_Backend_Function.Models
{
    public class Subscriber
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public bool IsConfirmed { get; set; }

        public string? ConfirmationToken { get; set; }

        public string UnsubscribeToken { get; set; } = string.Empty;

        public DateTime? TokenCreatedAt { get; set; }
    }
}