using API.Constants;
using API.Data;
using API.DTOs;
using API.Models;
using API.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace API.Test.Integration
{
    public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidJsonData_ShouldReturnSuccess()
        {
            // Arrange
            var userDto = TestDataBuilder.CreateValidUserRegisterDto($"authtest{Guid.NewGuid()}@example.com");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", userDto);

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
            var userDto = new UserRegisterDto
            {
                Email = "invalidemail",
                Username = "testuser123",
                Password = "Password123!",
                PasswordConfirm = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", userDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Failed);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var email = $"logintest{Guid.NewGuid()}@example.com";
            var password = "Password123!";
            var username = $"loginuser{Guid.NewGuid().ToString().Substring(0, 8)}";

            // Create and activate user with proper password hashing and confirmation
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<API.Services.PasswordHashing.IPasswordHasher>();

                var user = new user_login
                {
                    email = email,
                    username = username,
                    password_hash = passwordHasher.HashPassword(password),
                    created_at = DateTime.Now,
                    is_confirmed = new BitArray(1, true)
                };

                await dbContext.user_logins.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }

            var loginDto = new LoginDto
            {
                Email = email,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be(ResponseMessages.LoginSuccessful);
            jsonDoc.RootElement.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Failed);
        }

        [Fact]
        public async Task Login_WithEmptyEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "",
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
