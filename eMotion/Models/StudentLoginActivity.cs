namespace eMotion.Models
{
    public class StudentLoginActivity
    {
        public int Id { get; set; }

        public string UserId { get; set; } = "";

        public DateTime LastLoginAt { get; set; }

        public DateTime? LastReminderSentAt { get; set; }
    }
}