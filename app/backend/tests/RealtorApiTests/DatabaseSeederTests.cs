using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests;

public class DatabaseSeederTests
{
    [Fact]
    public async Task SeedAsync_Does_Not_Duplicate_Property_Data()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);

        await DatabaseSeeder.SeedAsync(context);
        await DatabaseSeeder.SeedAsync(context);

        Assert.Equal(10, await context.Properties.CountAsync());
    }
}
