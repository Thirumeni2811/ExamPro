namespace Exam.Domain.ResponseFormat
{
    public interface IServiceResponseFactory
    {
        IServiceResponse CreateResponse(bool success, string message, ActionType actionType);
        IServiceResponse<T> CreateResponse<T>(bool success, string message, ActionType actionType, T? data = default);
    }
}
