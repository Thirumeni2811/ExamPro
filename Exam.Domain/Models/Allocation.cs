using Exam.Domain.Enums;

namespace Exam.Domain.Models
{
    public class Allocation
    {
        public Guid Id { get; set; }
        public Guid VenueId { get; set; }              // FK to Venue.Id
        public Guid InvigilatorId { get; set; }      // FK to User.Id
        public DateTime Date { get; set; }
        public AllocationStatus Status { get; set; } // Draft, Approved, Published
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Venue Venue { get; set; } = null!;
        public User Invigilator { get; set; } = null!;
    }
}
