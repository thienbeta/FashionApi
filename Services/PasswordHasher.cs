using System.Security.Cryptography;
using System.Text;

namespace FashionApi.Services
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Fallback for old SHA256 hashes if necessary, but best to migrate.
            // For now, assuming fresh DB or migration, we just verify using BCrypt.
            // If the stored hash doesn't look like a BCrypt hash, we could fall back, 
            // but let's stick to secure default first.
            try 
            {
                return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
            }
            catch (Exception)
            {
                // If verify fails (e.g. invalid salt format because it's SHA256), return false
                return false;
            }
        }
    }
}