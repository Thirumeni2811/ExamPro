using Exam.Domain.Models;

namespace Exam.Domain.Interfaces
{
    public interface IVenueRepository
    {
        Task<Venue?> GetByIdAsync(Guid id);
        Task<IEnumerable<Venue>> GetAllAsync();
        Task<IEnumerable<Venue>> GetAllAsync(string? name, string? hallNumber);
        Task<Venue> CreateAsync(Venue venue);
        void Update(Venue venue);
        Task<bool> DeleteAsync(Guid id);
        Task<int> CountAsync(); // Max 20
        Task<bool> ExistsAsync(string name, string hallNumber); // duplicate check
        Task SaveChangesAsync();
    }
}
