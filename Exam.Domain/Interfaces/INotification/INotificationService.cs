using Exam.Domain.Models;
using Exam.Domain.ResponseFormat;

namespace Exam.Domain.Interfaces
{
    public interface INotificationService
    {
        Task<IServiceResponse<IEnumerable<Notification>>> GetByUser(Guid userId);
        Task<IServiceResponse<Notification>> Create(Guid userId, string message);
        Task<IServiceResponse<bool>> Delete(Guid id);
    }
}
