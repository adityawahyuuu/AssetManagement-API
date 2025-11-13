using API.Configuration;
using API.Data;
using API.Services.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace API.Test.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Set the environment to Test to load appsettings.Test.json
            builder.UseEnvironment("Test");

            // Ensure the configuration loads appsettings.Test.json
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Clear existing configuration sources and rebuild with Test environment
                context.HostingEnvironment.EnvironmentName = "Test";
            });

            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Register DatabaseOptions for ApplicationDbContext
                services.Configure<DatabaseOptions>(options =>
                {
                    options.SchemaName = "kosan";
                });

                // Mock the EmailService for testing (avoid real SMTP calls)
                var emailServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEmailService));

                if (emailServiceDescriptor != null)
                {
                    services.Remove(emailServiceDescriptor);
                }

                var mockEmailService = new Mock<IEmailService>();
                mockEmailService
                    .Setup(es => es.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

                mockEmailService
                    .Setup(es => es.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

                services.AddScoped<IEmailService>(_ => mockEmailService.Object);

                // Add InMemory database for testing
                services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                // Build service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                    db.Database.EnsureCreated();
                }
            });
        }
    }
}
