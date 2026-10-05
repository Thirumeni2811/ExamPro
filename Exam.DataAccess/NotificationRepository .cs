using Exam.Domain.Exceptions;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Exam.DataAccess.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ExamDbContext _context;

        public NotificationRepository(ExamDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetByUserAsync(Guid userId)
        {
            try
            {
                return await _context.Notifications
                    .AsNoTracking()
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error fetching notifications for user {userId}.", ex);
            }
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Notifications
                    .AsNoTracking()
                    .FirstOrDefaultAsync(n => n.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error fetching notification {id}.", ex);
            }
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            try
            {
                await _context.Notifications.AddAsync(notification);
                return notification; 
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error creating notification.", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var n = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
                if (n == null) return false;
                _context.Notifications.Remove(n);
                return true;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error deleting notification {id}.", ex);
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
                throw new DatabaseException("Error saving Notification changes.", ex);
            }
        }
    }
}
