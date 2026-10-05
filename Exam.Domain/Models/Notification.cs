using System;

namespace Exam.Domain.Models
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();   // PK

        public Guid UserId { get; set; }                  // FK to User.Id
        public string Message { get; set; } = null!;      // The notification text
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
    }
}
