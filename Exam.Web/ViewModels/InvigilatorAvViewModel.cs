using Exam.Domain.Models;

namespace Exam.Web.ViewModels
{
    public class InvigilatorAvViewModel
    {
        public IEnumerable<InvigilatorAvailability> Availabilities { get; set; } = new List<InvigilatorAvailability>();
        public IEnumerable<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
