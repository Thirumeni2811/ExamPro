namespace Exam.Domain.ResponseFormat
{
    public interface IServiceResponse
    {
        bool Success { get; set; }
        string Message { get; set; }
        ActionType ActionType { get; set; }
    }
}
