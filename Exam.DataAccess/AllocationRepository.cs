using Exam.Domain.Enums;
using Exam.Domain.Exceptions;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Exam.DataAccess.Repositories
{
    public class AllocationRepository : IAllocationRepository
    {
        private readonly ExamDbContext _context;

        public AllocationRepository(ExamDbContext context)
        {
            _context = context;
        }

        public async Task<Allocation?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Allocations
                .Include(a => a.Venue)
                .Include(a => a.Invigilator)
                .FirstOrDefaultAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(
                    $"Error fetching allocation", ex);
            }
        }

        public async Task<IEnumerable<Allocation>> GetAllAsync(
            string? venueName,
            string? hallNumber,
            string? invigilatorName,
            string? invigilatorEmail,
            DateTime? date,
            AllocationStatus? status)
        {
            try
            {
                var q = _context.Allocations
                                .Include(a => a.Venue)
                                .Include(a => a.Invigilator)
                                .AsQueryable();

                if (!string.IsNullOrWhiteSpace(venueName))
                    q = q.Where(a => EF.Functions.Like(a.Venue.Name, $"%{venueName}%"));

                if (!string.IsNullOrWhiteSpace(hallNumber))
                    q = q.Where(a => EF.Functions.Like(a.Venue.HallNumber, $"%{hallNumber}%"));

                if (!string.IsNullOrWhiteSpace(invigilatorName))
                    q = q.Where(a => EF.Functions.Like(a.Invigilator.Name, $"%{invigilatorName}%"));

                if (!string.IsNullOrWhiteSpace(invigilatorEmail))
                    q = q.Where(a => EF.Functions.Like(a.Invigilator.Email, $"%{invigilatorEmail}%"));

                if (date.HasValue)
                    q = q.Where(a => a.Date.Date == date.Value.Date);

                if (status.HasValue)
                    q = q.Where(a => a.Status == status.Value);

                return await q.OrderBy(a => a.Date)
                              .ThenBy(a => a.Venue.Name)
                              .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error fetching allocations", ex);
            }
        }

        public async Task<IEnumerable<Allocation>> GetAllAsync()
        {
            try
            {
                return await _context.Allocations
                    .Include(a => a.Venue)
                    .Include(a => a.Invigilator)
                    .AsNoTracking()
                    .OrderBy(a => a.Date)
                    .ThenBy(a => a.Venue.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error fetching allocations", ex);
            }
        }

        public async Task<Allocation?> GetByInvigilatorAndDateAsync(Guid invigilatorId, DateTime date)
        {
            try
            {
                var d = date.Date;
                return await _context.Allocations
                    .Include(a => a.Venue)
                    .Include(a => a.Invigilator)
                    .FirstOrDefaultAsync(a => a.InvigilatorId == invigilatorId && a.Date == d);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error fetching allocation by invigilator/date.", ex);
            }
        }

        public async Task<Allocation> CreateAsync(Allocation allocation)
        {
            try
            {
                await _context.Allocations.AddAsync(allocation);
                return allocation;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(
                    $"Error fetching allocation", ex);
            }
        }

        public async Task UpdateAsync(Allocation allocation)
        {
            try
            {
                _context.Allocations.Update(allocation);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(
                    $"Error fetching allocation", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await _context.Allocations.FindAsync(id);
                if (entity == null) return false;

                _context.Allocations.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(
                    $"Error fetching allocation", ex);
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
                throw new DatabaseException(
                    $"Error fetching allocation.", ex);
            }
        }
    }
}
