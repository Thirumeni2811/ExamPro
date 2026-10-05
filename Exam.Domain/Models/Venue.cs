namespace Exam.Domain.Models
{
    public class Venue
    {
        public Guid Id { get; set; }
        public string Name { get; set; }        // e.g., "IT Block"
        public string HallNumber { get; set; }  // e.g., "Hall 201"
        public int Capacity { get; set; } // 20
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
