namespace Exam.Domain.Enums
{
    public enum AllocationStatus
    {
        Draft = 1,      // Auto-generated, not confirmed
        Approved = 2,   // Checked & confirmed by admin
        Published = 3   // Visible to Invigilators
    }
}
