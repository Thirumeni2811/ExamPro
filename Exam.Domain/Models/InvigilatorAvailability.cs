using Exam.Domain.Enums;

namespace Exam.Domain.Models
{
    public class InvigilatorAvailability
    {
        public Guid Id { get; set; }
        public Guid InvigilatorId { get; set; }       // FK to User.Id
        public DayOfWeekEnum DayOfWeek { get; set; }  // Stored as int
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime Date { get; set; }
        public User Invigilator { get; set; } = null!;
    }
}
