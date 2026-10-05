using Exam.Domain.Exceptions;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Exam.Domain.ResponseFormat;

namespace Exam.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IServiceResponseFactory _responseFactory;

        public NotificationService(
            INotificationRepository repository,
            IServiceResponseFactory responseFactory)
        {
            _repository = repository;
            _responseFactory = responseFactory;
        }

        public async Task<IServiceResponse<IEnumerable<Notification>>> GetByUser(Guid userId)
        {
            try
            {
                var items = await _repository.GetByUserAsync(userId);
                return _responseFactory.CreateResponse(
                    true,
                    "Notifications retrieved.",
                    ActionType.Retrieved,
                    items);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving notifications.", ex);
            }
        }

        public async Task<IServiceResponse<Notification>> Create(Guid userId, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return _responseFactory.CreateResponse<Notification>(
                        false,
                        "Message is required.",
                        ActionType.ValidationError);
                }

                var note = new Notification
                {
                    UserId = userId,
                    Message = message.Trim()
                };

                await _repository.CreateAsync(note);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true,
                    "Notification created.",
                    ActionType.Created,
                    note);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error creating notification.", ex);
            }
        }

        public async Task<IServiceResponse<bool>> Delete(Guid id)
        {
            try
            {
                var ok = await _repository.DeleteAsync(id);
                if (!ok)
                {
                    return _responseFactory.CreateResponse<bool>(
                        false,
                        "Notification not found.",
                        ActionType.NotFound,
                        false);
                }

                await _repository.SaveChangesAsync();
                return _responseFactory.CreateResponse(
                    true,
                    "Notification deleted.",
                    ActionType.Deleted,
                    true);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error deleting notification.", ex);
            }
        }
    }
}
