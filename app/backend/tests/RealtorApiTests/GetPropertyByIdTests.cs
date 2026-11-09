using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.GetPropertyById;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests;

public sealed class GetPropertyByIdTests
{
    [Fact]
    public async Task Handler_Returns_The_Property_When_Id_Exists()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Casa central",
            Description = "Casa luminosa",
            Address = "Calle 1",
            Price = 250000m,
            Status = PropertyStatus.Available,
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 110m,
            ImageUrl = "/assets/properties/casa.jpg",
            CreatedAt = DateTime.UtcNow
        };
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        var handler = new GetPropertyByIdHandler(context);

        var result = await handler.HandleAsync(property.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(property.Id, result.Id);
        Assert.Equal(property.Title, result.Title);
        Assert.Equal(property.Status, result.Status);
        Assert.Equal(property.ImageUrl, result.ImageUrl);
        Assert.Equal(property.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task Handler_Returns_Null_When_Id_Does_Not_Exist()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var handler = new GetPropertyByIdHandler(context);

        var result = await handler.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }
}
