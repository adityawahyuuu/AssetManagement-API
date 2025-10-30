using API.Configuration;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Test.Helpers
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext CreateInMemoryContext(string databaseName = "TestDb")
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            var databaseOptions = Options.Create(new DatabaseOptions
            {
                SchemaName = "kosan"
            });

            return new ApplicationDbContext(options, databaseOptions);
        }
    }
}
