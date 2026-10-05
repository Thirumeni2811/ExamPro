using Exam.Domain.Exceptions;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Exam.Domain.ResponseFormat;

namespace Exam.Service.Implementations
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository _repository;
        private readonly IServiceResponseFactory _responseFactory;

        // Optionally inject config for hall cap later
        private const int MaxVenues = 20;

        public VenueService(IVenueRepository repository, IServiceResponseFactory responseFactory)
        {
            _repository = repository;
            _responseFactory = responseFactory;
        }

        // -------------------------------------------------
        // Get By Id
        // -------------------------------------------------
        public async Task<IServiceResponse<Venue>> GetById(Guid id)
        {
            try
            {
                var venue = await _repository.GetByIdAsync(id);
                if (venue == null)
                    return _responseFactory.CreateResponse<Venue>(false, "Venue not found.", ActionType.NotFound);

                return _responseFactory.CreateResponse(true, "Venue retrieved.", ActionType.Retrieved, venue);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving venue.", ex);
            }
        }

        // -------------------------------------------------
        // Get All
        // -------------------------------------------------
        public async Task<IServiceResponse<IEnumerable<Venue>>> GetAll()
        {
            try
            {
                var venues = await _repository.GetAllAsync();
                return _responseFactory.CreateResponse(true, "Venues retrieved.", ActionType.Retrieved, venues);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving venues.", ex);
            }
        }

        // -------------------------------------------------
        // Get All (Filter)
        // -------------------------------------------------
        public async Task<IServiceResponse<IEnumerable<Venue>>> GetAll(string? name, string? hallNumber)
        {
            try
            {
                var venues = await _repository.GetAllAsync(name, hallNumber);
                return _responseFactory.CreateResponse(true, "Venues retrieved.", ActionType.Retrieved, venues);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving filtered venues.", ex);
            }
        }

        // -------------------------------------------------
        // Create
        // -------------------------------------------------
        public async Task<IServiceResponse<Venue>> Create(Venue venue)
        {
            try
            {
                var count = await _repository.CountAsync();
                var MaxVenues = 20;
                if (count >= MaxVenues)
                    return _responseFactory.CreateResponse<Venue>(false, $"Maximum of {MaxVenues} venues reached.", ActionType.Conflict);

                var exists = await _repository.ExistsAsync(venue.Name, venue.HallNumber);
                if (exists)
                    return _responseFactory.CreateResponse<Venue>(false, "Venue already exists.", ActionType.Conflict);

                venue.CreatedAt = DateTime.UtcNow;

                await _repository.CreateAsync(venue);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(true, "Venue created.", ActionType.Created, venue);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error creating venue.", ex);
            }
        }

        // -------------------------------------------------
        // Update
        // -------------------------------------------------
        public async Task<IServiceResponse<Venue>> Update(Venue venue)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(venue.Id);
                if (existing == null)
                    return _responseFactory.CreateResponse<Venue>(false, "Venue not found.", ActionType.NotFound);

                // Duplicate check only if changed
                if (!string.Equals(existing.Name, venue.Name, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(existing.HallNumber, venue.HallNumber, StringComparison.OrdinalIgnoreCase))
                {
                    var exists = await _repository.ExistsAsync(venue.Name, venue.HallNumber);
                    if (exists)
                        return _responseFactory.CreateResponse<Venue>(false, "Another venue with same name and hall number exists.", ActionType.Conflict);
                }

                // Preserve CreatedAt
                venue.CreatedAt = existing.CreatedAt;

                _repository.Update(venue);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(true, "Venue updated.", ActionType.Updated, venue);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error updating venue.", ex);
            }
        }

        // -------------------------------------------------
        // Delete
        // -------------------------------------------------
        public async Task<IServiceResponse<bool>> Delete(Guid id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return _responseFactory.CreateResponse<bool>(false, "Venue not found.", ActionType.NotFound);

                var ok = await _repository.DeleteAsync(id);
                if (ok)
                    await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(ok, ok ? "Venue deleted." : "Delete failed.", ok ? ActionType.Deleted : ActionType.Failed, ok);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error deleting venue.", ex);
            }
        }
    }
}
