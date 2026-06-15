using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Helpers
{
    public class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrWhiteSpace(inputPassword) || string.IsNullOrWhiteSpace(storedPassword))
            {
                return false;
            }

            string hashedInput = HashPassword(inputPassword);
            return hashedInput == storedPassword;
        }
    }
}