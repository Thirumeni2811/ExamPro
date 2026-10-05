using Exam.Domain.Enums;
using Exam.Domain.Models;
using Exam.Domain.ResponseFormat;

namespace Exam.Domain.Interfaces
{
    public interface IInvigilatorAvailabilityService
    {
        Task<IServiceResponse<InvigilatorAvailability>> GetById(Guid id);
        Task<IServiceResponse<IEnumerable<InvigilatorAvailability>>> GetAll(
            string? invigilatorName,
            DayOfWeekEnum? dayOfWeek);
        Task<IServiceResponse<IEnumerable<InvigilatorAvailability>>> GetByDate(DateTime date);
        Task<IServiceResponse<IEnumerable<InvigilatorAvailability>>> GetByInvigilator(Guid invigilatorId);
        Task<IServiceResponse<InvigilatorAvailability>> Create(InvigilatorAvailability availability);
        Task<IServiceResponse<InvigilatorAvailability>> Update(InvigilatorAvailability availability);
        Task<IServiceResponse<bool>> Delete(Guid id);
    }
}
