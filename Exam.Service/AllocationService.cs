using Exam.Domain.Enums;
using Exam.Domain.Exceptions;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Exam.Domain.ResponseFormat;

namespace Exam.Service
{
    public class AllocationService : IAllocationService
    {
        private readonly IAllocationRepository _repository;
        private readonly IVenueRepository _venueRepository;
        private readonly IUserRepository _userRepository;
        private readonly IInvigilatorAvailabilityRepository _availabilityRepository;
        private readonly IServiceResponseFactory _responseFactory;
        private readonly INotificationService _notificationService;

        public AllocationService(
            IAllocationRepository repository,
            IVenueRepository venueRepository,
            IUserRepository userRepository,
            IInvigilatorAvailabilityRepository availabilityRepository,
            IServiceResponseFactory responseFactory,
            INotificationService notificationService)
        {
            _repository = repository;
            _venueRepository = venueRepository;
            _userRepository = userRepository;
            _availabilityRepository = availabilityRepository;
            _responseFactory = responseFactory;
            _notificationService = notificationService;
        }

        // -------------------------------------------------
        // Get All (with filter)
        // -------------------------------------------------
        public async Task<IServiceResponse<IEnumerable<Allocation>>> GetAll(
            string? venueName,
            string? hallNumber,
            string? invigilatorName,
            string? invigilatorEmail,
            DateTime? date,
            AllocationStatus? status)
        {
            try
            {
                var allocations = await _repository.GetAllAsync(venueName, hallNumber, invigilatorName, invigilatorEmail, date, status);
                return _responseFactory.CreateResponse(true, "Allocations retrieved successfully.", ActionType.Retrieved, allocations);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving allocations.", ex);
            }
        }

        // -------------------------------------------------
        // Get All (without filter)
        // -------------------------------------------------
        public async Task<IServiceResponse<IEnumerable<Allocation>>> GetAll()
        {
            try
            {
                var allocations = await _repository.GetAllAsync();
                return _responseFactory.CreateResponse(true, "Allocations retrieved successfully.", ActionType.Retrieved, allocations);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving allocations.", ex);
            }
        }

        // -------------------------------------------------
        // Get By Id
        // -------------------------------------------------
        public async Task<IServiceResponse<Allocation>> GetById(Guid id)
        {
            try
            {
                var allocation = await _repository.GetByIdAsync(id);
                if (allocation == null)
                    return _responseFactory.CreateResponse<Allocation>(false, "Allocation not found.", ActionType.NotFound);

                return _responseFactory.CreateResponse(true, "Allocation retrieved.", ActionType.Retrieved, allocation);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving allocation by Id.", ex);
            }
        }

        // -------------------------------------------------
        // Create
        // -------------------------------------------------
        public async Task<IServiceResponse<Allocation>> Create(Allocation allocation)
        {
            try
            {
                allocation.CreatedAt = DateTime.UtcNow;
                allocation.Status = AllocationStatus.Draft;

                await _repository.CreateAsync(allocation);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(true, "Allocation created successfully.", ActionType.Created, allocation);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error creating allocation.", ex);
            }
        }

        // -------------------------------------------------
        // Update
        // -------------------------------------------------
        public async Task<IServiceResponse<Allocation>> Update(Allocation allocation)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(allocation.Id);
                if (existing == null)
                    return _responseFactory.CreateResponse<Allocation>(false, "Allocation not found.", ActionType.NotFound);

                if (existing.Status == AllocationStatus.Published)
                {
                    var dayEnum = (DayOfWeekEnum)allocation.Date.DayOfWeek;

                    var availability = await _availabilityRepository.ExistsAsync(
                        allocation.InvigilatorId,
                        dayEnum);

                    if (!availability)
                        return _responseFactory.CreateResponse<Allocation>(
                            false,
                            "Invigilator not available on this day.",
                            ActionType.ValidationError);
                }

                existing.VenueId = allocation.VenueId;
                existing.InvigilatorId = allocation.InvigilatorId;
                existing.Status = allocation.Status;

                await _repository.UpdateAsync(existing);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(true, "Allocation updated successfully.", ActionType.Updated, existing);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error updating allocation.", ex);
            }
        }

        // -------------------------------------------------
        // Delete
        // -------------------------------------------------
        public async Task<IServiceResponse<bool>> Delete(Guid id)
        {
            try
            {
                var allocation = await _repository.GetByIdAsync(id);
                if (allocation == null)
                {
                    return _responseFactory.CreateResponse<bool>(
                        false,
                        "Allocation not found.",
                        ActionType.NotFound,
                        false);
                }

                // Delete
                var deleted = await _repository.DeleteAsync(id);
                if (!deleted)
                {
                    return _responseFactory.CreateResponse<bool>(
                        false,
                        "Delete failed.",
                        ActionType.Failed,
                        false);
                }

                await _repository.SaveChangesAsync();

                try
                {
                    var venue = allocation.Venue
                                ?? await _venueRepository.GetByIdAsync(allocation.VenueId);
                    var invigilator = allocation.Invigilator
                                      ?? await _userRepository.GetByIdAsync(allocation.InvigilatorId);

                    var venueName = venue?.Name ?? "your assigned venue";
                    var msg = $"Your allocation for {allocation.Date:dd-MMM-yyyy} at {venueName} has been cancelled.";

                    await _notificationService.Create(allocation.InvigilatorId, msg);
                }
                catch
                {
                    // Swallow per-notification failure; deletion already succeeded.
                }

                return _responseFactory.CreateResponse(
                    true,
                    "Allocation deleted successfully.",
                    ActionType.Deleted,
                    true);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error deleting allocation.", ex);
            }
        }

        // -------------------------------------------------
        // Generate Draft
        // -------------------------------------------------
        public async Task<IServiceResponse<bool>> GenerateDraft(DateTime date)
        {
            try
            {
                // Get all existing allocations for the date
                var existingAllocations = await _repository.GetAllAsync(null, null, null, null, date, null);

                // Get all venues
                var venues = await _venueRepository.GetAllAsync(null, null);

                // Identify venues that are not allocated yet
                var allocatedVenueIds = existingAllocations.Select(a => a.VenueId).ToHashSet();
                var unallocatedVenues = venues.Where(v => !allocatedVenueIds.Contains(v.Id)).ToList();

                if (!unallocatedVenues.Any())
                {
                    return _responseFactory.CreateResponse(false,
                        "All venues already have allocations for that date.",
                        ActionType.Conflict,
                        false);
                }

                // Get available invigilators for that day
                var dayOfWeek = (DayOfWeekEnum)date.DayOfWeek;
                var availabilities = await _availabilityRepository.GetAllAsync(null, dayOfWeek);

                // Get invigilators already allocated for this date
                var allocatedInvigilatorIds = existingAllocations.Select(a => a.InvigilatorId).ToHashSet();

                // Filter only unallocated invigilators
                var availableInvigilators = availabilities
                    .Select(a => a.InvigilatorId)
                    .Distinct()
                    .Where(id => !allocatedInvigilatorIds.Contains(id))
                    .ToList();

                if (!availableInvigilators.Any())
                {
                    return _responseFactory.CreateResponse(false,
                        "No available invigilators for remaining venues.",
                        ActionType.Conflict,
                        false);
                }

                // Assign invigilators to remaining venues
                int index = 0;
                foreach (var venue in unallocatedVenues)
                {
                    if (index >= availableInvigilators.Count) break;

                    var allocation = new Allocation
                    {
                        VenueId = venue.Id,
                        InvigilatorId = availableInvigilators[index],
                        Date = date,
                        Status = AllocationStatus.Draft
                    };

                    await _repository.CreateAsync(allocation);
                    index++;
                }

                await _repository.SaveChangesAsync();
                return _responseFactory.CreateResponse(true,
                    "Draft allocation updated for remaining venues.",
                    ActionType.Created,
                    true);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error generating draft allocations.", ex);
            }
        }

        // -------------------------------------------------
        // Approve Draft
        // -------------------------------------------------
        public async Task<IServiceResponse<bool>> ApproveDraft(DateTime date)
        {
            try
            {
                var allocations = await _repository.GetAllAsync(null, null, null, null, date, AllocationStatus.Draft);
                foreach (var allocation in allocations)
                    allocation.Status = AllocationStatus.Approved;

                foreach (var allocation in allocations)
                    await _repository.UpdateAsync(allocation);

                await _repository.SaveChangesAsync();
                return _responseFactory.CreateResponse(true, "Draft allocations approved.", ActionType.Updated, true);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error approving draft allocations.", ex);
            }
        }

        // -------------------------------------------------
        // Publish
        // -------------------------------------------------
        public async Task<IServiceResponse<bool>> Publish(DateTime date)
        {
            try
            {
                var allocations = await _repository.GetAllAsync(
                    venueName: null,
                    hallNumber: null,
                    invigilatorName: null,
                    invigilatorEmail: null,
                    date: date,
                    status: AllocationStatus.Approved);

                var list = allocations?.ToList() ?? new List<Allocation>();
                if (list.Count == 0)
                {
                    return _responseFactory.CreateResponse<bool>(
                        false,
                        "No approved allocations found for the selected date.",
                        ActionType.NotFound,
                        false);
                }

                foreach (var allocation in list)
                {
                    allocation.Status = AllocationStatus.Published;
                    await _repository.UpdateAsync(allocation);
                }

                await _repository.SaveChangesAsync();

                int notified = 0;
                foreach (var allocation in list)
                {
                    try
                    {
                        var venue = allocation.Venue
                                    ?? await _venueRepository.GetByIdAsync(allocation.VenueId);
                        var invigilator = allocation.Invigilator
                                          ?? await _userRepository.GetByIdAsync(allocation.InvigilatorId);

                        var venueName = venue?.Name ?? "your assigned venue";
                        var msg = $"Your allocation for {allocation.Date:dd-MMM-yyyy} at {venueName} has been published.";
                        var notifResp = await _notificationService.Create(
                            allocation.InvigilatorId,
                            msg);

                        if (notifResp.Success) notified++;
                    }
                    catch
                    {
                        // ignore per-notification error; could log
                    }
                }

                var finalMessage = notified == list.Count
                    ? "Allocations published and notifications sent."
                    : $"Allocations published. Notifications sent: {notified}/{list.Count}.";

                return _responseFactory.CreateResponse(
                    true,
                    finalMessage,
                    ActionType.Updated,
                    true);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error publishing allocations.", ex);
            }
        }

    }
}
