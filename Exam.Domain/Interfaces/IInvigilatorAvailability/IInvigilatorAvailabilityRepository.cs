using Exam.Domain.Enums;
using Exam.Domain.Models;

namespace Exam.Domain.Interfaces
{
    public interface IInvigilatorAvailabilityRepository
    {
        Task<InvigilatorAvailability?> GetByIdAsync(Guid id);
        Task<IEnumerable<InvigilatorAvailability>> GetAllAsync(
           string? invigilatorName,
           DayOfWeekEnum? dayOfWeek);
        Task<IEnumerable<InvigilatorAvailability>> GetByDateAsync(DateTime dateUtcOrLocal);
        Task<IEnumerable<InvigilatorAvailability>> GetByInvigilatorAsync(Guid invigilatorId);
        Task<bool> ExistsAsync(Guid invigilatorId, DayOfWeekEnum dayOfWeek);
        Task<InvigilatorAvailability> CreateAsync(InvigilatorAvailability availability);
        void Update(InvigilatorAvailability availability);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
