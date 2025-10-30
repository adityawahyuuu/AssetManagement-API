using API.Configuration;
using API.Services.PasswordHashing;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace API.Test.Services
{
    public class PasswordHasherTests
    {
        private readonly IPasswordHasher _passwordHasher;

        public PasswordHasherTests()
        {
            var options = Options.Create(new PasswordHashingOptions
            {
                SaltSize = 16,
                HashSize = 32,
                Iterations = 10000,
                HashAlgorithm = "SHA256"
            });

            _passwordHasher = new PasswordHasher(options);
        }

        [Fact]
        public void HashPassword_ShouldReturnNonEmptyString()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var hashedPassword = _passwordHasher.HashPassword(password);

            // Assert
            hashedPassword.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void HashPassword_ShouldReturnBase64EncodedString()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var hashedPassword = _passwordHasher.HashPassword(password);

            // Assert
            var isValidBase64 = IsBase64String(hashedPassword);
            isValidBase64.Should().BeTrue();
        }

        [Fact]
        public void HashPassword_ShouldGenerateDifferentHashesForSamePassword()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var hash1 = _passwordHasher.HashPassword(password);
            var hash2 = _passwordHasher.HashPassword(password);

            // Assert
            hash1.Should().NotBe(hash2, "because each hash should use a unique salt");
        }

        [Fact]
        public void HashPassword_ShouldGenerateDifferentHashesForDifferentPasswords()
        {
            // Arrange
            var password1 = "TestPassword123!";
            var password2 = "DifferentPassword456!";

            // Act
            var hash1 = _passwordHasher.HashPassword(password1);
            var hash2 = _passwordHasher.HashPassword(password2);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void HashPassword_ShouldHaveCorrectLength()
        {
            // Arrange
            var password = "TestPassword123!";
            var expectedLength = (int)Math.Ceiling((16 + 32) * 4.0 / 3.0); // Base64 encoding

            // Act
            var hashedPassword = _passwordHasher.HashPassword(password);

            // Assert
            hashedPassword.Length.Should().BeGreaterOrEqualTo(expectedLength - 2)
                .And.BeLessOrEqualTo(expectedLength + 2);
        }

        private bool IsBase64String(string base64)
        {
            try
            {
                Convert.FromBase64String(base64);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
