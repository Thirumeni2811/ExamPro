using Exam.Domain.Models;
using Exam.Domain.ResponseFormat;

namespace Exam.Domain.Interfaces
{
    public interface IVenueService
    {
        Task<IServiceResponse<Venue>> GetById(Guid id);
        Task<IServiceResponse<IEnumerable<Venue>>> GetAll();
        Task<IServiceResponse<IEnumerable<Venue>>> GetAll(string? name, string? hallNumber);
        Task<IServiceResponse<Venue>> Create(Venue venue);
        Task<IServiceResponse<Venue>> Update(Venue venue);
        Task<IServiceResponse<bool>> Delete(Guid id);
    }
}
