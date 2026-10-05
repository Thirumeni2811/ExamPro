using Exam.Domain.Exceptions;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Exam.DataAccess.Repositories
{
    public class VenueRepository : IVenueRepository
    {
        private readonly ExamDbContext _context;

        public VenueRepository(ExamDbContext context)
        {
            _context = context;
        }

        public async Task<Venue?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Venues
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error fetching venue by Id: {id}.", ex);
            }
        }

        public async Task<IEnumerable<Venue>> GetAllAsync()
        {
            try
            {
                return await _context.Venues
                    .AsNoTracking()
                    .OrderBy(v => v.Name)
                    .ThenBy(v => v.HallNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error fetching venues.", ex);
            }
        }

        public async Task<IEnumerable<Venue>> GetAllAsync(string? name, string? hallNumber)
        {
            try
            {
                var q = _context.Venues.AsQueryable();

                if (!string.IsNullOrWhiteSpace(name))
                    q = q.Where(v => EF.Functions.Like(v.Name, $"%{name}%"));

                if (!string.IsNullOrWhiteSpace(hallNumber))
                    q = q.Where(v => EF.Functions.Like(v.HallNumber, $"%{hallNumber}%"));

                return await q
                    .AsNoTracking()
                    .OrderBy(v => v.Name)
                    .ThenBy(v => v.HallNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error fetching filtered venues.", ex);
            }
        }

        public async Task<Venue> CreateAsync(Venue venue)
        {
            try
            {
                await _context.Venues.AddAsync(venue);
                return venue; // persisted on SaveChangesAsync
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error creating venue.", ex);
            }
        }

        public void Update(Venue venue)
        {
            try
            {
                _context.Venues.Update(venue);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error updating venue Id: {venue.Id}.", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await _context.Venues.FirstOrDefaultAsync(v => v.Id == id);
                if (entity == null) return false;
                _context.Venues.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error deleting venue Id: {id}.", ex);
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Venues.CountAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error counting venues.", ex);
            }
        }

        public async Task<bool> ExistsAsync(string name, string hallNumber)
        {
            try
            {
                return await _context.Venues.AnyAsync(v =>
                    v.Name == name && v.HallNumber == hallNumber);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error checking venue existence.", ex);
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
                throw new DatabaseException("Error saving venue changes.", ex);
            }
        }
    }
}
