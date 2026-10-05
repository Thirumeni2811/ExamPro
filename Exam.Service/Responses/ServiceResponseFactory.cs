using Exam.Domain.ResponseFormat;

namespace Exam.Service.Responses
{
    // Implements the Domain IServiceResponseFactory
    public class ServiceResponseFactory : IServiceResponseFactory
    {
        public IServiceResponse CreateResponse(bool success, string message, ActionType actionType)
        {
            return new ServiceResponse(success, message, actionType);
        }

        public IServiceResponse<T> CreateResponse<T>(bool success, string message, ActionType actionType, T? data = default)
        {
            return new ServiceResponse<T>(success, message, actionType, data);
        }
    }

    // Implements the Domain IServiceResponse
    public class ServiceResponse : IServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ActionType ActionType { get; set; }

        public ServiceResponse(bool success, string message, ActionType actionType)
        {
            Success = success;
            Message = message;
            ActionType = actionType;
        }
    }

    // Implements the Domain IServiceResponse<T>
    public class ServiceResponse<T> : ServiceResponse, IServiceResponse<T>
    {
        public T? Data { get; set; }

        public ServiceResponse(bool success, string message, ActionType actionType, T? data = default)
            : base(success, message, actionType)
        {
            Data = data;
        }
    }
}
