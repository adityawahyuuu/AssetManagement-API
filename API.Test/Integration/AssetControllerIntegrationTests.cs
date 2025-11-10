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
    public class AssetControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AssetControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<(string token, int userId, int roomId, int categoryId)> SetupTestDataAsync()
        {
            var email = $"assettest{Guid.NewGuid()}@example.com";
            var username = $"assetuser{Guid.NewGuid().ToString().Substring(0, 8)}";
            var password = "TestPassword123!";

            int userId, roomId, categoryId;

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
                userId = user.userid;

                var room = new Room
                {
                    UserId = userId,
                    Name = $"AssetRoom{Guid.NewGuid().ToString().Substring(0, 8)}",
                    LengthM = 5.0m,
                    WidthM = 4.0m,
                    CreatedAt = DateTime.Now
                };

                await dbContext.Rooms.AddAsync(room);
                await dbContext.SaveChangesAsync();
                roomId = room.Id;

                // Check if category exists, if not create one
                var category = await dbContext.AssetCategories.FirstOrDefaultAsync();
                if (category == null)
                {
                    category = new AssetCategory
                    {
                        Name = "Test Category",
                        CreatedAt = DateTime.Now
                    };
                    await dbContext.AssetCategories.AddAsync(category);
                    await dbContext.SaveChangesAsync();
                }
                categoryId = category.Id;
            }

            var loginDto = new LoginDto
            {
                Email = email,
                Password = password
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var token = jsonDoc.RootElement.GetProperty("data").GetProperty("token").GetString()!;

            return (token, userId, roomId, categoryId);
        }

        [Fact]
        public async Task GetAllAssets_WithAuth_ShouldReturnPaginatedAssets()
        {
            // Arrange
            var (token, _, _, _) = await SetupTestDataAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/assets?page=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Assets retrieved successfully");
            jsonDoc.RootElement.TryGetProperty("page", out _).Should().BeTrue();
            jsonDoc.RootElement.TryGetProperty("totalCount", out _).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAssets_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/assets");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AddAsset_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var (token, _, roomId, categoryId) = await SetupTestDataAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var addAssetDto = new AddAssetDto
            {
                RoomId = roomId,
                Category = "Test Category",
                Name = "Test Asset",
                LengthCm = 100,
                WidthCm = 50,
                HeightCm = 75,
                PurchaseDate = DateTime.Now,
                PurchasePrice = 1000.00m,
                Condition = "Good",
                Notes = "Test notes"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/assets", addAssetDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Asset added successfully");
        }

        [Fact]
        public async Task AddAsset_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var addAssetDto = new AddAssetDto
            {
                RoomId = 1,
                Category = "Test Category",
                Name = "Test Asset",
                LengthCm = 100,
                WidthCm = 50,
                HeightCm = 75,
                PurchaseDate = DateTime.Now,
                PurchasePrice = 1000.00m,
                Condition = "Good"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/assets", addAssetDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAssetById_WithValidId_ShouldReturnAsset()
        {
            // Arrange
            var (token, _, roomId, categoryId) = await SetupTestDataAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create an asset first
            var addAssetDto = new AddAssetDto
            {
                RoomId = roomId,
                Category = "Test Category",
                Name = "Get Test Asset",
                LengthCm = 120,
                WidthCm = 60,
                HeightCm = 80,
                PurchaseDate = DateTime.Now,
                PurchasePrice = 1500.00m,
                Condition = "Excellent"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/assets", addAssetDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createDoc = JsonDocument.Parse(createContent);
            var assetId = createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            // Act
            var response = await _client.GetAsync($"/api/assets/{assetId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Asset retrieved successfully");
        }

        [Fact]
        public async Task GetAssetsByRoomId_WithValidRoomId_ShouldReturnAssets()
        {
            // Arrange
            var (token, _, roomId, categoryId) = await SetupTestDataAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create an asset in the room
            var addAssetDto = new AddAssetDto
            {
                RoomId = roomId,
                Category = "Test Category",
                Name = "Room Asset",
                LengthCm = 150,
                WidthCm = 80,
                HeightCm = 90,
                PurchaseDate = DateTime.Now,
                PurchasePrice = 2000.00m,
                Condition = "Good"
            };

            await _client.PostAsJsonAsync("/api/assets", addAssetDto);

            // Act
            var response = await _client.GetAsync($"/api/assets/room/{roomId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Assets retrieved successfully");
        }

        [Fact]
        public async Task UpdateAsset_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var (token, _, roomId, categoryId) = await SetupTestDataAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create an asset first
            var addAssetDto = new AddAssetDto
            {
                RoomId = roomId,
                Category = "Test Category",
                Name = "Update Test Asset",
                LengthCm = 100,
                WidthCm = 50,
                HeightCm = 75,
                PurchaseDate = DateTime.Now,
                PurchasePrice = 1000.00m,
                Condition = "Good"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/assets", addAssetDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createDoc = JsonDocument.Parse(createContent);
            var assetId = createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            var updateAssetDto = new UpdateAssetDto
            {
                Name = "Updated Asset Name",
                PurchaseDate = DateTime.Now.AddDays(-10),
                PurchasePrice = 1200.00m,
                Condition = "Excellent",
                Notes = "Updated notes"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/assets/{assetId}", updateAssetDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Asset updated successfully");
        }

        [Fact]
        public async Task UpdateAsset_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var updateAssetDto = new UpdateAssetDto
            {
                Name = "Test Asset",
                PurchaseDate = DateTime.Now,
                PurchasePrice = 1000.00m,
                Condition = "Good"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/assets/1", updateAssetDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteAsset_WithValidId_ShouldReturnSuccess()
        {
            // Arrange
            var (token, _, roomId, categoryId) = await SetupTestDataAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create an asset first
            var addAssetDto = new AddAssetDto
            {
                RoomId = roomId,
                Category = "Test Category",
                Name = "Delete Test Asset",
                LengthCm = 100,
                WidthCm = 50,
                HeightCm = 75,
                PurchaseDate = DateTime.Now,
                PurchasePrice = 1000.00m,
                Condition = "Good"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/assets", addAssetDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createDoc = JsonDocument.Parse(createContent);
            var assetId = createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            // Act
            var response = await _client.DeleteAsync($"/api/assets/{assetId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Asset deleted successfully");
        }

        [Fact]
        public async Task DeleteAsset_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.DeleteAsync("/api/assets/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteAsset_WithInvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            var (token, _, _, _) = await SetupTestDataAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync("/api/assets/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
