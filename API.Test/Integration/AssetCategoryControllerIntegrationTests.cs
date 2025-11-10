using API.Constants;
using API.Data;
using API.DTOs;
using API.Models;
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
    public class AssetCategoryControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AssetCategoryControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var email = $"categorytest{Guid.NewGuid()}@example.com";
            var username = $"catuser{Guid.NewGuid().ToString().Substring(0, 8)}";
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
        public async Task GetAllAssetCategories_ShouldReturnAllCategories()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Seed some categories
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Check if categories already exist, if not add some
                if (!await dbContext.AssetCategories.AnyAsync())
                {
                    var categories = new List<AssetCategory>
                    {
                        new AssetCategory { Name = "Furniture", CreatedAt = DateTime.Now },
                        new AssetCategory { Name = "Electronics", CreatedAt = DateTime.Now },
                        new AssetCategory { Name = "Appliances", CreatedAt = DateTime.Now }
                    };

                    await dbContext.AssetCategories.AddRangeAsync(categories);
                    await dbContext.SaveChangesAsync();
                }
            }

            // Act
            var response = await _client.GetAsync("/api/asset-categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);
            jsonDoc.RootElement.GetProperty("message").GetString().Should().Be("Asset categories retrieved successfully");

            var data = jsonDoc.RootElement.GetProperty("data");
            data.GetArrayLength().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllAssetCategories_WithEmptyDatabase_ShouldReturnEmptyArray()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Clear all categories
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var categories = await dbContext.AssetCategories.ToListAsync();
                dbContext.AssetCategories.RemoveRange(categories);
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync("/api/asset-categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            jsonDoc.RootElement.GetProperty("type").GetString().Should().Be(ResponseMessages.Success);

            var data = jsonDoc.RootElement.GetProperty("data");
            data.GetArrayLength().Should().Be(0);
        }

        [Fact]
        public async Task GetAllAssetCategories_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            // This endpoint requires authentication, so no authentication header should return 401
            var response = await _client.GetAsync("/api/asset-categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllAssetCategories_ShouldReturnCorrectCategoryStructure()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var categoryName = $"TestCategory{Guid.NewGuid().ToString().Substring(0, 8)}";

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var category = new AssetCategory
                {
                    Name = categoryName,
                    CreatedAt = DateTime.Now
                };

                await dbContext.AssetCategories.AddAsync(category);
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync("/api/asset-categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            var data = jsonDoc.RootElement.GetProperty("data");
            var categories = data.EnumerateArray().ToList();

            categories.Should().NotBeEmpty();

            // Check that at least one category has the required structure
            var firstCategory = categories.First();
            firstCategory.TryGetProperty("id", out _).Should().BeTrue();
            firstCategory.TryGetProperty("name", out _).Should().BeTrue();
        }
    }
}
