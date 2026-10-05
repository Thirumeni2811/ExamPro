using Exam.Domain.Enums;
using Exam.Domain.Exceptions;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Exam.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ExamDbContext _context;

        public UserRepository(ExamDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking() // read-only
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error fetching user by Id: {id}", ex);
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking() // read-only
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error fetching user by Email: {email}", ex);
            }
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Role == role)
                    .OrderBy(u => u.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error fetching users by Role: {role}", ex);
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Role != UserRole.Admin)
                    .OrderBy(u => u.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error fetching all users", ex);
            }
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                return user; // SaveChangesAsync handled outside
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error creating user with email: {user.Email}", ex);
            }
        }

        public void Update(User user)
        {
            try
            {
                _context.Users.Update(user);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error updating user with Id: {user.Id}", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (entity == null) return false;

                _context.Users.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error deleting user with Id: {id}", ex);
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync(
            string? name,
            Guid? id,
            string? email,
            string? contactNumber,
            bool? active)
        {
            try
            {
                var q = _context.Users.AsQueryable();

                if (id.HasValue)
                    q = q.Where(u => u.Id == id.Value);

                if (!string.IsNullOrWhiteSpace(name))
                    q = q.Where(u => EF.Functions.Like(u.Name, $"%{name}%"));

                if (!string.IsNullOrWhiteSpace(email))
                    q = q.Where(u => EF.Functions.Like(u.Email, $"%{email}%"));

                if (!string.IsNullOrWhiteSpace(contactNumber))
                    q = q.Where(u => u.ContactNumber != null && EF.Functions.Like(u.ContactNumber, $"%{contactNumber}%"));

                if (active.HasValue)
                    q = q.Where(u => u.Active == active.Value);

                return await q.AsNoTracking().Where(u => u.Role != UserRole.Admin).OrderBy(u => u.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error fetching filtered users", ex);
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error saving changes to database", ex);
            }
        }
    }
}
