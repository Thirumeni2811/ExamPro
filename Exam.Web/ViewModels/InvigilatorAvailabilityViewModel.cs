using Exam.Domain.Models;

namespace Exam.Web.ViewModels
{
    public class InvigilatorAvailabilityViewModel
    {
        public DateTime? SelectedDate { get; set; }
        public List<InvigilatorAvailability> Existing { get; set; } = new();
    }
}
