using Exam.Domain.Enums;
using Exam.Domain.Exceptions;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Exam.DataAccess.Repositories
{
    public class InvigilatorAvailabilityRepository : IInvigilatorAvailabilityRepository
    {
        private readonly ExamDbContext _context;

        public InvigilatorAvailabilityRepository(ExamDbContext context)
        {
            _context = context;
        }

        public async Task<InvigilatorAvailability?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.InvigilatorAvailabilities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error fetching availability Id: {id}.", ex);
            }
        }

        public async Task<IEnumerable<InvigilatorAvailability>> GetAllAsync(
            string? invigilatorName,
            DayOfWeekEnum? dayOfWeek)
        {
            try
            {
                var q = _context.InvigilatorAvailabilities
                                .Include(a => a.Invigilator) 
                                .AsQueryable();

                if (dayOfWeek.HasValue)
                    q = q.Where(a => a.DayOfWeek == dayOfWeek.Value);

                if (!string.IsNullOrWhiteSpace(invigilatorName))
                {
                    var pattern = $"%{invigilatorName}%";
                    q = q.Where(a => EF.Functions.Like(a.Invigilator.Name, pattern));
                }

                return await q
                    .AsNoTracking()
                    .OrderBy(a => a.Invigilator.Name)
                    .ThenBy(a => a.DayOfWeek)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error fetching filtered availabilities.", ex);
            }
        }

        public async Task<IEnumerable<InvigilatorAvailability>> GetByDateAsync(DateTime dateUtcOrLocal)
        {
            try
            {
                // Normalize to date range [00:00, +1day)
                var dayStart = dateUtcOrLocal.Date;
                var dayEnd = dayStart.AddDays(1);

                return await _context.InvigilatorAvailabilities
                    .Where(a => a.Date >= dayStart && a.Date < dayEnd)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error fetching filtered availabilities.", ex);
            }
        }

        public async Task<IEnumerable<InvigilatorAvailability>> GetByInvigilatorAsync(Guid invigilatorId)
        {
            try
            {
                return await _context.InvigilatorAvailabilities
                    .AsNoTracking()
                    .Where(a => a.InvigilatorId == invigilatorId)
                    .Include(a => a.Invigilator)                
                    .ThenInclude(u => u.Notifications)
                    .OrderBy(a => a.DayOfWeek)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error fetching availabilities for InvigilatorId: {invigilatorId}.", ex);
            }
        }

        public async Task<bool> ExistsAsync(Guid invigilatorId, DayOfWeekEnum dayOfWeek)
        {
            try
            {
                return await _context.InvigilatorAvailabilities
                    .AnyAsync(a => a.InvigilatorId == invigilatorId && a.DayOfWeek == dayOfWeek);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error checking availability existence.", ex);
            }
        }

        public async Task<InvigilatorAvailability> CreateAsync(InvigilatorAvailability availability)
        {
            try
            {
                await _context.InvigilatorAvailabilities.AddAsync(availability);
                return availability;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error creating availability.", ex);
            }
        }

        public void Update(InvigilatorAvailability availability)
        {
            try
            {
                _context.InvigilatorAvailabilities.Update(availability);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error updating availability Id: {availability.Id}.", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await _context.InvigilatorAvailabilities.FirstOrDefaultAsync(a => a.Id == id);
                if (entity == null) return false;

                _context.InvigilatorAvailabilities.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error deleting availability Id: {id}.", ex);
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
                throw new DatabaseException("Error saving availability changes.", ex);
            }
        }
    }
}
