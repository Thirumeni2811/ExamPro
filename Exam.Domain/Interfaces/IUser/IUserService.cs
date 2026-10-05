using Exam.Domain.Enums;
using Exam.Domain.Models;
using Exam.Domain.ResponseFormat;

namespace Exam.Domain.Interfaces
{
    public interface IUserService
    {
        // Create 
        Task<IServiceResponse<(User User, string Token)>> Create(User user);

        // Login
        Task<IServiceResponse<(User User, string Token)>> LoginAdmin(string email, string password);
        Task<IServiceResponse<(User User, string Token)>> LoginInvigilator(string email, string password);

        // User Management 
        Task<IServiceResponse<User>> Update(User user);     // Fails if target is Admin
        Task<IServiceResponse<bool>> Delete(Guid id);       // Fails if target is Admin

        // Retrieval
        Task<IServiceResponse<IEnumerable<User>>> GetAll();
        Task<IServiceResponse<IEnumerable<User>>> GetAll(
            string? name,
            Guid? id,
            string? email,
            string? contactNumber,
            bool? active);

        Task<IServiceResponse<User>> GetById(Guid id);
    }
}
