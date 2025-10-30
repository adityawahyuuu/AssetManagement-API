using API.Constants;
using API.Data;
using API.DTOs;
using API.Models;
using API.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace API.Test.Integration
{
    public class UserControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public UserControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var userDto = TestDataBuilder.CreateValidUserRegisterDto("integration@example.com");
            var formData = new MultipartFormDataContent
            {
                { new StringContent(userDto.Email), "Email" },
                { new StringContent(userDto.Username), "Username" },
                { new StringContent(userDto.Password), "Password" },
                { new StringContent(userDto.PasswordConfirm), "PasswordConfirm" }
            };

            // Act
            var response = await _client.PostAsync("/user/register", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("data").GetProperty("email").GetString()
                .Should().Be(userDto.Email);
        }

        [Fact]
        public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var formData = new MultipartFormDataContent
            {
                { new StringContent("invalidemail"), "Email" },
                { new StringContent("testuser123"), "Username" },
                { new StringContent("Password123!"), "Password" },
                { new StringContent("Password123!"), "PasswordConfirm" }
            };

            // Act
            var response = await _client.PostAsync("/user/register", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Failed);
        }

        [Fact]
        public async Task Register_WithPasswordMismatch_ShouldReturnBadRequest()
        {
            // Arrange
            var formData = new MultipartFormDataContent
            {
                { new StringContent("test@example.com"), "Email" },
                { new StringContent("testuser123"), "Username" },
                { new StringContent("Password123!"), "Password" },
                { new StringContent("DifferentPassword123!"), "PasswordConfirm" }
            };

            // Act
            var response = await _client.PostAsync("/user/register", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain(ResponseMessages.PasswordsDoNotMatch);
        }

        [Fact]
        public async Task VerifyOtp_WithValidOtp_ShouldReturnSuccess()
        {
            // Arrange
            var email = "verify@example.com";
            var otpCode = "123456";

            // Seed database with pending user and OTP
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var pendingUser = TestDataBuilder.CreatePendingUser(email);
                pendingUser.expires_at = DateTime.Now.AddMinutes(20);
                await dbContext.pending_users.AddAsync(pendingUser);

                var otp = TestDataBuilder.CreateOtpCode(email, otpCode);
                otp.expires_at = DateTime.Now.AddMinutes(10);
                await dbContext.otp_codes.AddAsync(otp);

                await dbContext.SaveChangesAsync();
            }

            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" },
                { new StringContent(otpCode), "OtpCode" }
            };

            // Act
            var response = await _client.PostAsync("/user/verify", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString()
                .Should().Be(OtpMessages.UserActivatedSuccessfully);

            // Verify user was created in database
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await dbContext.user_logins.FirstOrDefaultAsync(u => u.email == email);
                user.Should().NotBeNull();

                var pendingUser = await dbContext.pending_users.FirstOrDefaultAsync(p => p.email == email);
                pendingUser.Should().BeNull();
            }
        }

        [Fact]
        public async Task VerifyOtp_WithInvalidOtp_ShouldReturnBadRequest()
        {
            // Arrange
            var email = "invalidotp@example.com";
            var correctOtp = "123456";
            var incorrectOtp = "999999";

            // Seed database
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var pendingUser = TestDataBuilder.CreatePendingUser(email);
                await dbContext.pending_users.AddAsync(pendingUser);

                var otp = TestDataBuilder.CreateOtpCode(email, correctOtp);
                otp.expires_at = DateTime.Now.AddMinutes(10);
                await dbContext.otp_codes.AddAsync(otp);

                await dbContext.SaveChangesAsync();
            }

            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" },
                { new StringContent(incorrectOtp), "OtpCode" }
            };

            // Act
            var response = await _client.PostAsync("/user/verify", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain(OtpMessages.OtpInvalid);
        }

        [Fact]
        public async Task ResendOtp_WithValidPendingUser_ShouldReturnSuccess()
        {
            // Arrange
            var email = "resend@example.com";

            // Seed database
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var pendingUser = TestDataBuilder.CreatePendingUser(email);
                pendingUser.expires_at = DateTime.Now.AddMinutes(20);
                await dbContext.pending_users.AddAsync(pendingUser);

                await dbContext.SaveChangesAsync();
            }

            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" }
            };

            // Act
            var response = await _client.PostAsync("/user/resend-otp", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString()
                .Should().Be(OtpMessages.OtpResentSuccessfully);
        }

        [Fact]
        public async Task ResendOtp_WithNonExistentUser_ShouldReturnBadRequest()
        {
            // Arrange
            var formData = new MultipartFormDataContent
            {
                { new StringContent("nonexistent@example.com"), "Email" }
            };

            // Act
            var response = await _client.PostAsync("/user/resend-otp", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain(OtpMessages.PendingUserNotFound);
        }
    }
}
