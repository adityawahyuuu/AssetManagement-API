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
    }
}
