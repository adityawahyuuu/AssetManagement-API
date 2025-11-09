using API.Constants;
using API.Data;
using API.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
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

        [Fact]
        public async Task GetAllAssetCategories_ShouldReturnAllCategories()
        {
            // Arrange
            // Seed some categories
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Check if categories already exist, if not add some
                if (!await dbContext.asset_categories.AnyAsync())
                {
                    var categories = new List<AssetCategory>
                    {
                        new AssetCategory { name = "Furniture", created_at = DateTime.Now },
                        new AssetCategory { name = "Electronics", created_at = DateTime.Now },
                        new AssetCategory { name = "Appliances", created_at = DateTime.Now }
                    };

                    await dbContext.asset_categories.AddRangeAsync(categories);
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
            // Clear all categories
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var categories = await dbContext.asset_categories.ToListAsync();
                dbContext.asset_categories.RemoveRange(categories);
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
        public async Task GetAllAssetCategories_ShouldNotRequireAuthentication()
        {
            // Act
            // This endpoint should be public, so no authentication header
            var response = await _client.GetAsync("/api/asset-categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllAssetCategories_ShouldReturnCorrectCategoryStructure()
        {
            // Arrange
            var categoryName = $"TestCategory{Guid.NewGuid().ToString().Substring(0, 8)}";

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var category = new AssetCategory
                {
                    name = categoryName,
                    created_at = DateTime.Now
                };

                await dbContext.asset_categories.AddAsync(category);
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
