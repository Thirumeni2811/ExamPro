using Exam.Domain.Enums;

namespace Exam.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; } // Enum stored as int
        public string Password { get; set; }
        public string? ProfileImage { get; set; }
        public string ContactNumber { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
