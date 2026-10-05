using Exam.Domain.Enums;
using Exam.Domain.Exceptions;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Exam.Domain.ResponseFormat;
using Exam.Service.Responses;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Exam.Service.Implementations
{
    public class InvigilatorAvailabilityService : IInvigilatorAvailabilityService
    {
        private readonly IInvigilatorAvailabilityRepository _repository;
        private readonly IServiceResponseFactory _responseFactory;
        private readonly IAllocationRepository _repo;


        public InvigilatorAvailabilityService(IInvigilatorAvailabilityRepository repository, IServiceResponseFactory responseFactory, IAllocationRepository repo)
        {
            _repository = repository;
            _responseFactory = responseFactory;
            _repo = repo;
        }

        // ------------------------------------------------------------------
        //  Get By Id
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<InvigilatorAvailability>> GetById(Guid id)
        {
            try
            {
                var availability = await _repository.GetByIdAsync(id);
                if (availability == null)
                    return _responseFactory.CreateResponse<InvigilatorAvailability>(false, "Availability not found.", ActionType.NotFound);

                return _responseFactory.CreateResponse(true, "Availability retrieved.", ActionType.Retrieved, availability);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving availability.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Get All
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<IEnumerable<InvigilatorAvailability>>> GetAll(
            string? invigilatorName,
            DayOfWeekEnum? dayOfWeek)
        {
            try
            {
                var data = await _repository.GetAllAsync(invigilatorName, dayOfWeek);
                return _responseFactory.CreateResponse(true, "Availabilities retrieved.", ActionType.Retrieved, data);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving filtered availabilities.", ex);
            }
        }

        public async Task<IServiceResponse<IEnumerable<InvigilatorAvailability>>> GetByDate(DateTime date)
        {
            try
            {
                var results = await _repository.GetByDateAsync(date);
                return _responseFactory.CreateResponse(true, "Availabilities retrieved.", ActionType.Retrieved, results);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving filtered availabilities.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Get By Invigilator
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<IEnumerable<InvigilatorAvailability>>> GetByInvigilator(Guid invigilatorId)
        {
            try
            {
                var data = await _repository.GetByInvigilatorAsync(invigilatorId);
                return _responseFactory.CreateResponse(true, "Availabilities retrieved.", ActionType.Retrieved, data);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving invigilator availabilities.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Create
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<InvigilatorAvailability>> Create(InvigilatorAvailability availability)
        {
            try
            {
                if (availability.InvigilatorId == Guid.Empty)
                    return _responseFactory.CreateResponse<InvigilatorAvailability>(false, "InvigilatorId is required.", ActionType.ValidationError);

                // Prevent duplicate
                bool exists = await _repository.ExistsAsync(availability.InvigilatorId, availability.DayOfWeek);
                if (exists)
                    return _responseFactory.CreateResponse<InvigilatorAvailability>(false, "Availability already exists for this day.", ActionType.Conflict);

                availability.CreatedAt = DateTime.UtcNow;

                await _repository.CreateAsync(availability);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(true, "Availability created successfully.", ActionType.Created, availability);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error creating availability.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<InvigilatorAvailability>> Update(InvigilatorAvailability availability)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(availability.Id);
                if (existing == null)
                    return _responseFactory.CreateResponse<InvigilatorAvailability>(false, "Availability not found.", ActionType.NotFound);

                // Prevent duplicate if day or invigilator changed
                if (availability.InvigilatorId != existing.InvigilatorId || availability.DayOfWeek != existing.DayOfWeek)
                {
                    bool exists = await _repository.ExistsAsync(availability.InvigilatorId, availability.DayOfWeek);
                    if (exists)
                        return _responseFactory.CreateResponse<InvigilatorAvailability>(false, "Another record already exists for this invigilator on that day.", ActionType.Conflict);
                }

                availability.CreatedAt = existing.CreatedAt; // preserve original timestamp

                _repository.Update(availability);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(true, "Availability updated successfully.", ActionType.Updated, availability);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error updating availability.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Delete
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<bool>> Delete(Guid id)
        {
            try
            {
                var availability = await _repository.GetByIdAsync(id);
                if (availability == null)
                    return _responseFactory.CreateResponse<bool>(
                        false, "Availability not found.", ActionType.NotFound);

                // 2. See if an allocation exists for this invigilator on that date
                var allocation = await _repo.GetByInvigilatorAndDateAsync(
                    availability.InvigilatorId,
                    availability.Date.Date);

                if (allocation != null &&
                    (allocation.Status == AllocationStatus.Approved ||
                     allocation.Status == AllocationStatus.Published))
                {
                    return _responseFactory.CreateResponse<bool>(
                        false,
                        "Cannot delete: date already allocated (Approved/Published).",
                        ActionType.Forbidden);
                }

                // 3. Delete
                var deleted = await _repository.DeleteAsync(id);
                if (deleted)
                    await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    deleted,
                    deleted ? "Availability deleted." : "Delete failed.",
                    deleted ? ActionType.Deleted : ActionType.Failed,
                    deleted);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error deleting availability.", ex);
            }
        }

    }
}
