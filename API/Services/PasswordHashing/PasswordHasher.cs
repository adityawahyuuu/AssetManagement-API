using System.Security.Cryptography;
using API.Configuration;
using Microsoft.Extensions.Options;

namespace API.Services.PasswordHashing
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHashingOptions _options;

        public PasswordHasher(IOptions<PasswordHashingOptions> options)
        {
            _options = options.Value;
        }

        public string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = RandomNumberGenerator.GetBytes(_options.SaltSize);

            // Derive the hash
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                _options.Iterations,
                new HashAlgorithmName(_options.HashAlgorithm),
                _options.HashSize
            );

            // Combine salt and hash
            byte[] hashBytes = new byte[_options.SaltSize + _options.HashSize];
            Array.Copy(salt, 0, hashBytes, 0, _options.SaltSize);
            Array.Copy(hash, 0, hashBytes, _options.SaltSize, _options.HashSize);

            // Convert to base64 for storage
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Convert the stored hash from base64
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                // Extract the salt from the stored hash
                byte[] salt = new byte[_options.SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, _options.SaltSize);

                // Extract the hash from the stored hash
                byte[] storedHash = new byte[_options.HashSize];
                Array.Copy(hashBytes, _options.SaltSize, storedHash, 0, _options.HashSize);

                // Hash the provided password with the extracted salt
                byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    _options.Iterations,
                    new HashAlgorithmName(_options.HashAlgorithm),
                    _options.HashSize
                );

                // Compare the computed hash with the stored hash
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
            }
            catch
            {
                // If any error occurs (e.g., invalid base64), return false
                return false;
            }
        }
    }
}
