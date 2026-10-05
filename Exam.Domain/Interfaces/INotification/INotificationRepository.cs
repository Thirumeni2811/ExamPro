using Exam.Domain.Models;

namespace Exam.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserAsync(Guid userId);
        Task<Notification?> GetByIdAsync(Guid id);
        Task<Notification> CreateAsync(Notification notification);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
