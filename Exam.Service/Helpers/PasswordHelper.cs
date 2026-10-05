using BCrypt.Net;

namespace Exam.Service.Helpers
{
    public class PasswordHelper
    {
        //Hash the password
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        //compare the plain password with the hashed one
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
