using Exam.Domain.Enums;
using Exam.Domain.Models;

namespace Exam.Domain.Interfaces
{
    public interface IAllocationRepository
    {
        Task<Allocation?> GetByIdAsync(Guid id);
        Task<Allocation?> GetByInvigilatorAndDateAsync(Guid invigilatorId, DateTime date);
        Task<IEnumerable<Allocation>> GetAllAsync(
            string? venueName,
            string? hallNumber,
            string? invigilatorName,
            string? invigilatorEmail,
            DateTime? date,
            AllocationStatus? status);
        Task<IEnumerable<Allocation>> GetAllAsync();
        Task<Allocation> CreateAsync(Allocation allocation);
        Task UpdateAsync(Allocation allocation);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
