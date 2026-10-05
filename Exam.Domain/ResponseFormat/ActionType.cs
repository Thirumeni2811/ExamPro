namespace Exam.Domain.ResponseFormat
{
    public enum ActionType
    {
        None,               // No action performed or default state
        Created,            // 201 Created - Resource successfully created
        Retrieved,          // 200 OK - Successful data retrieval
        Updated,            // 200 OK or 204 No Content - Successful update
        Deleted,            // 200 OK or 204 No Content - Successful deletion
        Failed,             // 400 or 500 - Generic failure
        NotFound,           // 404 - Resource not found
        Unauthorized,       // 401 - Authentication failed
        Forbidden,          // 403 - Authenticated but not allowed
        Conflict,           // 409 - Conflict or duplicate
        ValidationError,    // 400 - Validation error
        ServerError         // 500 - Unexpected server error
    }
}
