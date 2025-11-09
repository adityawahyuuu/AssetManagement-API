using API.Constants;
using API.Data;
using API.DTOs;
using API.Models;
using API.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace API.Test.Integration
{
    public class UserControllerExtendedTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public UserControllerExtendedTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<(string token, string email, int userId)> CreateAuthenticatedUserAsync()
        {
            var email = $"extendedtest{Guid.NewGuid()}@example.com";
            var username = $"extenduser{Guid.NewGuid().ToString().Substring(0, 8)}";
            var password = "TestPassword123!";

            int userId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<API.Services.PasswordHashing.IPasswordHasher>();

                var user = new UserLogin
                {
                    email = email,
                    username = username,
                    password = passwordHasher.HashPassword(password),
                    full_name = "Extended Test User",
                    created_at = DateTime.Now
                };

                await dbContext.user_logins.AddAsync(user);
                await dbContext.SaveChangesAsync();
                userId = user.id;
            }

            var loginForm = new MultipartFormDataContent
            {
                { new StringContent(username), "Username" },
                { new StringContent(password), "Password" }
            };

            var response = await _client.PostAsync("/api/user/login", loginForm);
            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var token = jsonDoc.RootElement.GetProperty("data").GetProperty("token").GetString()!;

            return (token, email, userId);
        }

        [Fact]
        public async Task Login_WithFormData_ShouldReturnSuccess()
        {
            // Arrange
            var email = $"loginformtest{Guid.NewGuid()}@example.com";
            var username = $"loginform{Guid.NewGuid().ToString().Substring(0, 8)}";
            var password = "TestPassword123!";

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<API.Services.PasswordHashing.IPasswordHasher>();

                var user = new UserLogin
                {
                    email = email,
                    username = username,
                    password = passwordHasher.HashPassword(password),
                    full_name = "Login Form Test User",
                    created_at = DateTime.Now
                };

                await dbContext.user_logins.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }

            var formData = new MultipartFormDataContent
            {
                { new StringContent(username), "Username" },
                { new StringContent(password), "Password" }
            };

            // Act
            var response = await _client.PostAsync("/api/user/login", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be(ResponseMessages.LoginSuccessful);
            jsonDoc.RootElement.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetCurrentUser_WithValidToken_ShouldReturnUserInfo()
        {
            // Arrange
            var (token, email, userId) = await CreateAuthenticatedUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/user/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("User information retrieved successfully");

            var data = jsonDoc.RootElement.GetProperty("data");
            data.GetProperty("email").GetString().Should().Be(email);
        }

        [Fact]
        public async Task GetCurrentUser_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/user/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCurrentUser_WithInvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await _client.GetAsync("/api/user/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Logout_WithValidToken_ShouldReturnSuccess()
        {
            // Arrange
            var (token, _, _) = await CreateAuthenticatedUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsync("/api/user/logout", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Logout successful");
        }

        [Fact]
        public async Task Logout_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.PostAsync("/api/user/logout", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ForgotPassword_WithValidEmail_ShouldReturnSuccess()
        {
            // Arrange
            var email = $"forgotpw{Guid.NewGuid()}@example.com";
            var username = $"forgotuser{Guid.NewGuid().ToString().Substring(0, 8)}";

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<API.Services.PasswordHashing.IPasswordHasher>();

                var user = new UserLogin
                {
                    email = email,
                    username = username,
                    password = passwordHasher.HashPassword("OldPassword123!"),
                    full_name = "Forgot Password Test User",
                    created_at = DateTime.Now
                };

                await dbContext.user_logins.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }

            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" }
            };

            // Act
            var response = await _client.PostAsync("/api/user/forgot-password", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be(ResponseMessages.PasswordResetOtpSent);
        }

        [Fact]
        public async Task ForgotPassword_WithNonExistentEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var formData = new MultipartFormDataContent
            {
                { new StringContent("nonexistent@example.com"), "Email" }
            };

            // Act
            var response = await _client.PostAsync("/api/user/forgot-password", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ForgotPassword_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var formData = new MultipartFormDataContent
            {
                { new StringContent("invalidemail"), "Email" }
            };

            // Act
            var response = await _client.PostAsync("/api/user/forgot-password", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ResetPassword_WithValidOtp_ShouldReturnSuccess()
        {
            // Arrange
            var email = $"resetpw{Guid.NewGuid()}@example.com";
            var username = $"resetuser{Guid.NewGuid().ToString().Substring(0, 8)}";
            var otpCode = "123456";
            var newPassword = "NewPassword123!";

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<API.Services.PasswordHashing.IPasswordHasher>();

                var user = new UserLogin
                {
                    email = email,
                    username = username,
                    password = passwordHasher.HashPassword("OldPassword123!"),
                    full_name = "Reset Password Test User",
                    created_at = DateTime.Now
                };

                await dbContext.user_logins.AddAsync(user);

                var passwordResetOtp = new PasswordResetOtp
                {
                    email = email,
                    otp_code = otpCode,
                    expires_at = DateTime.Now.AddMinutes(15),
                    is_used = false,
                    created_at = DateTime.Now
                };

                await dbContext.password_reset_otps.AddAsync(passwordResetOtp);
                await dbContext.SaveChangesAsync();
            }

            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" },
                { new StringContent(otpCode), "OtpCode" },
                { new StringContent(newPassword), "NewPassword" },
                { new StringContent(newPassword), "NewPasswordConfirm" }
            };

            // Act
            var response = await _client.PostAsync("/api/user/reset-password", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be(ResponseMessages.PasswordResetSuccessful);
        }

        [Fact]
        public async Task ResetPassword_WithInvalidOtp_ShouldReturnBadRequest()
        {
            // Arrange
            var email = $"invalidotp{Guid.NewGuid()}@example.com";

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<API.Services.PasswordHashing.IPasswordHasher>();

                var user = new UserLogin
                {
                    email = email,
                    username = $"invuser{Guid.NewGuid().ToString().Substring(0, 8)}",
                    password = passwordHasher.HashPassword("OldPassword123!"),
                    full_name = "Invalid OTP Test User",
                    created_at = DateTime.Now
                };

                await dbContext.user_logins.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }

            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" },
                { new StringContent("999999"), "OtpCode" },
                { new StringContent("NewPassword123!"), "NewPassword" },
                { new StringContent("NewPassword123!"), "NewPasswordConfirm" }
            };

            // Act
            var response = await _client.PostAsync("/api/user/reset-password", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ResetPassword_WithPasswordMismatch_ShouldReturnBadRequest()
        {
            // Arrange
            var email = $"pwmismatch{Guid.NewGuid()}@example.com";
            var otpCode = "123456";

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<API.Services.PasswordHashing.IPasswordHasher>();

                var user = new UserLogin
                {
                    email = email,
                    username = $"mismatch{Guid.NewGuid().ToString().Substring(0, 8)}",
                    password = passwordHasher.HashPassword("OldPassword123!"),
                    full_name = "Mismatch Test User",
                    created_at = DateTime.Now
                };

                await dbContext.user_logins.AddAsync(user);

                var passwordResetOtp = new PasswordResetOtp
                {
                    email = email,
                    otp_code = otpCode,
                    expires_at = DateTime.Now.AddMinutes(15),
                    is_used = false,
                    created_at = DateTime.Now
                };

                await dbContext.password_reset_otps.AddAsync(passwordResetOtp);
                await dbContext.SaveChangesAsync();
            }

            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" },
                { new StringContent(otpCode), "OtpCode" },
                { new StringContent("NewPassword123!"), "NewPassword" },
                { new StringContent("DifferentPassword123!"), "NewPasswordConfirm" }
            };

            // Act
            var response = await _client.PostAsync("/api/user/reset-password", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain(ResponseMessages.PasswordsDoNotMatch);
        }
    }
}
