using Exam.Domain.Enums;
using Exam.Domain.Models;
using Exam.Domain.ResponseFormat;

namespace Exam.Domain.Interfaces
{
    public interface IAllocationService
    {
        Task<IServiceResponse<IEnumerable<Allocation>>> GetAll(
            string? venueName,
            string? hallNumber,
            string? invigilatorName,
            string? invigilatorEmail,
            DateTime? date,
            AllocationStatus? status);
        Task<IServiceResponse<IEnumerable<Allocation>>> GetAll();
        Task<IServiceResponse<Allocation>> GetById(Guid id);
        Task<IServiceResponse<Allocation>> Create(Allocation allocation);
        Task<IServiceResponse<Allocation>> Update(Allocation allocation);
        Task<IServiceResponse<bool>> Delete(Guid id);

        Task<IServiceResponse<bool>> GenerateDraft(DateTime date);
        Task<IServiceResponse<bool>> ApproveDraft(DateTime date);
        Task<IServiceResponse<bool>> Publish(DateTime date);
    }
}
