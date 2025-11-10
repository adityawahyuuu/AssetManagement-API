using API.Constants;
using API.Data;
using API.DTOs;
using API.Models;
using API.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace API.Test.Integration
{
    public class RoomControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public RoomControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var email = $"roomtest{Guid.NewGuid()}@example.com";
            var username = $"roomuser{Guid.NewGuid().ToString().Substring(0, 8)}";
            var password = "TestPassword123!";

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

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            return jsonDoc.RootElement.GetProperty("data").GetProperty("token").GetString()!;
        }

        [Fact]
        public async Task AddRoom_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var addRoomDto = new AddRoomDto
            {
                Name = $"Room{Guid.NewGuid().ToString().Substring(0, 8)}",
                LengthM = 5.0m,
                WidthM = 4.0m,
                Notes = "Test room description"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/rooms", addRoomDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Room added successfully");
        }

        [Fact]
        public async Task AddRoom_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var addRoomDto = new AddRoomDto
            {
                Name = "Test Room",
                LengthM = 5.0m,
                WidthM = 4.0m,
                Notes = "Test description"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/rooms", addRoomDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllUserRooms_WithAuth_ShouldReturnRooms()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/rooms");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Rooms retrieved successfully");
        }

        [Fact]
        public async Task GetAllUserRooms_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/rooms");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetRoomById_WithValidId_ShouldReturnRoom()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create a room first
            var addRoomDto = new AddRoomDto
            {
                Name = $"GetTest{Guid.NewGuid().ToString().Substring(0, 8)}",
                LengthM = 5.0m,
                WidthM = 4.0m,
                Notes = "Get test room"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/rooms", addRoomDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createDoc = JsonDocument.Parse(createContent);
            var roomId = createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            // Act
            var response = await _client.GetAsync($"/api/rooms/{roomId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Room retrieved successfully");
        }

        [Fact]
        public async Task GetRoomById_WithInvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/rooms/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateRoom_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create a room first
            var addRoomDto = new AddRoomDto
            {
                Name = $"UpdateTest{Guid.NewGuid().ToString().Substring(0, 8)}",
                LengthM = 5.0m,
                WidthM = 4.0m,
                Notes = "Update test room"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/rooms", addRoomDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createDoc = JsonDocument.Parse(createContent);
            var roomId = createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            var updateRoomDto = new UpdateRoomDto
            {
                Name = "Updated Room Number",
                Notes = "Updated description"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/rooms/{roomId}", updateRoomDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Room updated successfully");
        }

        [Fact]
        public async Task UpdateRoom_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var updateRoomDto = new UpdateRoomDto
            {
                Name = "Test Room",
                Notes = "Test description"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/rooms/1", updateRoomDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteRoom_WithValidId_ShouldReturnSuccess()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create a room first
            var addRoomDto = new AddRoomDto
            {
                Name = $"DeleteTest{Guid.NewGuid().ToString().Substring(0, 8)}",
                LengthM = 5.0m,
                WidthM = 4.0m,
                Notes = "Delete test room"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/rooms", addRoomDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createDoc = JsonDocument.Parse(createContent);
            var roomId = createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            // Act
            var response = await _client.DeleteAsync($"/api/rooms/{roomId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Room and all its assets deleted successfully");
        }

        [Fact]
        public async Task DeleteRoom_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.DeleteAsync("/api/rooms/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteRoom_WithInvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync("/api/rooms/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
