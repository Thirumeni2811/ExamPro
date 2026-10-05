using Exam.Domain.Enums;
using Exam.Domain.Models;

namespace Exam.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);

        /// <summary>Returns all users (no filters, sorted by Name).</summary>
        Task<IEnumerable<User>> GetAllAsync();

        Task<User> CreateAsync(User user);
        void Update(User user);  
        Task<bool> DeleteAsync(Guid id);      // false if not found

        /// <summary>
        /// Filtered query (any param null/empty = ignored).
        /// Uses EF.Functions.Like for string matches.
        /// </summary>
        Task<IEnumerable<User>> GetAllAsync(
            string? name,
            Guid? id,
            string? email,
            string? contactNumber,
            bool? active);
        Task SaveChangesAsync();

    }
}
