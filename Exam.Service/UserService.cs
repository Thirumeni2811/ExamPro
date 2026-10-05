using Exam.Domain.Enums;
using Exam.Domain.Exceptions;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Exam.Domain.ResponseFormat;
using Exam.Service.Helpers;

namespace Exam.Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IServiceResponseFactory _responseFactory;
        private readonly JwtHelper _jwtHelper;

        public UserService(
            IUserRepository repository,
            IServiceResponseFactory responseFactory,
            JwtHelper jwtHelper)
        {
            _repository = repository;
            _responseFactory = responseFactory;
            _jwtHelper = jwtHelper;
        }

        // ------------------------------------------------------------------
        // Create (Admin or Invigilator, based on user.Role)
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<(User User, string Token)>> Create(User user)
        {
            try
            {
                var exists = await _repository.GetByEmailAsync(user.Email);
                if (exists != null)
                    return _responseFactory.CreateResponse<(User User, string Token)>(
                        false, "Email already exists.", ActionType.Conflict, default);

                // Normalize role (default to Invigilator if invalid)
                if (!Enum.IsDefined(typeof(UserRole), user.Role))
                    user.Role = UserRole.Invigilator;

                if (user.Id == Guid.Empty)
                    user.Id = Guid.NewGuid();

                // Hash password
                user.Password = PasswordHelper.HashPassword(user.Password);

                user.Active = true;
                user.CreatedAt = DateTime.UtcNow;

                // Persist
                await _repository.CreateAsync(user);
                await _repository.SaveChangesAsync(); 

                // Generate JWT ONLY AFTER SUCCESSFUL SAVE
                string token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role.ToString());

                var msg = user.Role == UserRole.Admin
                    ? "Admin created successfully."
                    : "Invigilator created successfully.";

                return _responseFactory.CreateResponse(
                    true,
                    msg,
                    ActionType.Created,
                    (user, token)
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error creating user profile.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Login - Admin
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<(User User, string Token)>> LoginAdmin(string email, string password)
        {
            try
            {
                var user = await _repository.GetByEmailAsync(email);
                if (user == null || user.Role != UserRole.Admin)
                    return _responseFactory.CreateResponse<(User User, string Token)>(
                        false, "Admin account not found.", ActionType.NotFound, default);

                if (!user.Active)
                    return _responseFactory.CreateResponse<(User User, string Token)>(
                        false, "Account inactive.", ActionType.Forbidden, default);

                if (!PasswordHelper.VerifyPassword(password, user.Password))
                    return _responseFactory.CreateResponse<(User User, string Token)>(
                        false, "Invalid credentials.", ActionType.Unauthorized, default);

                string token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role.ToString());

                return _responseFactory.CreateResponse(
                    true,
                    "Login successful.",
                    ActionType.Retrieved,
                    (user, token));
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error during admin login.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Login - Invigilator 
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<(User User, string Token)>> LoginInvigilator(string email, string password)
        {
            try
            {
                var user = await _repository.GetByEmailAsync(email);
                if (user == null || user.Role != UserRole.Invigilator)
                    return _responseFactory.CreateResponse<(User User, string Token)>(
                        false, "Invigilator account not found.", ActionType.NotFound, default);

                if (!user.Active)
                    return _responseFactory.CreateResponse<(User User, string Token)>(
                        false, "Account inactive.", ActionType.Forbidden, default);

                if (!PasswordHelper.VerifyPassword(password, user.Password))
                    return _responseFactory.CreateResponse<(User User, string Token)>(
                        false, "Invalid credentials.", ActionType.Unauthorized, default);

                string token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role.ToString());

                return _responseFactory.CreateResponse(
                    true,
                    "Login successful.",
                    ActionType.Retrieved,
                    (user, token));
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error during invigilator login.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Update (fails if Admin)
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<User>> Update(User updatedUser)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(updatedUser.Id);
                if (existing == null)
                    return _responseFactory.CreateResponse<User>(false, "User not found.", ActionType.NotFound);

                if (existing.Role == UserRole.Admin)
                    return _responseFactory.CreateResponse<User>(false, "Admin cannot be updated.", ActionType.Forbidden);

                // Preserve fields that shouldn't change
                updatedUser.Role = existing.Role;
                updatedUser.Password = existing.Password;
                updatedUser.CreatedAt = existing.CreatedAt;

                if (string.IsNullOrWhiteSpace(updatedUser.ProfileImage))
                    updatedUser.ProfileImage = existing.ProfileImage;

                _repository.Update(updatedUser);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(true, "User updated successfully.", ActionType.Updated, updatedUser);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error updating user profile.", ex);
            }
        }


        // ------------------------------------------------------------------
        // Delete (fails if Admin)
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<bool>> Delete(Guid id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return _responseFactory.CreateResponse<bool>(false, "User not found.", ActionType.NotFound);

                if (existing.Role == UserRole.Admin)
                    return _responseFactory.CreateResponse<bool>(false, "Admin cannot be deleted.", ActionType.Forbidden);

                var ok = await _repository.DeleteAsync(id);
                if (ok) await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(ok, ok ? "User deleted successfully." : "Delete failed.", ok ? ActionType.Deleted : ActionType.Failed, ok);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error deleting user.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Get All (no filters)
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<IEnumerable<User>>> GetAll()
        {
            try
            {
                var users = await _repository.GetAllAsync();
                return _responseFactory.CreateResponse(true, "Users retrieved successfully.", ActionType.Retrieved, users);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving users.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Get All (with filters)
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<IEnumerable<User>>> GetAll(
            string? name,
            Guid? id,
            string? email,
            string? contactNumber,
            bool? active)
        {
            try
            {
                var users = await _repository.GetAllAsync(name, id, email, contactNumber, active);
                return _responseFactory.CreateResponse(true, "Filtered users retrieved successfully.", ActionType.Retrieved, users);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving filtered users.", ex);
            }
        }

        // ------------------------------------------------------------------
        // Get By Id
        // ------------------------------------------------------------------
        public async Task<IServiceResponse<User>> GetById(Guid id)
        {
            try
            {
                var user = await _repository.GetByIdAsync(id);
                if (user == null)
                    return _responseFactory.CreateResponse<User>(false, "User not found.", ActionType.NotFound);

                return _responseFactory.CreateResponse(true, "User retrieved successfully.", ActionType.Retrieved, user);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving user.", ex);
            }
        }
    }
}
