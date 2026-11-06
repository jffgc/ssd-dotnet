using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.GetPropertyById;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_Projects_Existing_Property_Without_Tracking_And_Logs_Found()
    {
        await using var context = CreateContext();
        var property = CreateProperty();
        context.Properties.Add(property);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var logger = new CapturingLogger<GetPropertyByIdHandler>();
        var handler = new GetPropertyByIdHandler(context, logger);

        var result = await handler.HandleAsync(new GetPropertyByIdQuery(property.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(property.Id, result.Id);
        Assert.Equal(property.Title, result.Title);
        Assert.Equal(property.Description, result.Description);
        Assert.Equal(property.Address, result.Address);
        Assert.Equal(property.Price, result.Price);
        Assert.Equal(property.Status, result.Status);
        Assert.Equal(property.BedroomCount, result.BedroomCount);
        Assert.Equal(property.BathroomCount, result.BathroomCount);
        Assert.Equal(property.AreaSquareMeters, result.AreaSquareMeters);
        Assert.Equal(property.ImageUrl, result.ImageUrl);
        Assert.Empty(context.ChangeTracker.Entries());

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(property.Id, entry.Properties["PropertyId"]);
        Assert.Equal("found", entry.Properties["Outcome"]);
        Assert.DoesNotContain(property.Title, entry.RenderedMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(property.Description, entry.RenderedMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_Propagates_Cancellation_To_EF_Core()
    {
        await using var context = CreateContext();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var handler = new GetPropertyByIdHandler(context, new CapturingLogger<GetPropertyByIdHandler>());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleAsync(new GetPropertyByIdQuery(Guid.NewGuid()), cancellation.Token));
    }

    [Fact]
    public async Task HandleAsync_Returns_Null_And_Logs_NotFound_When_Property_Is_Missing()
    {
        await using var context = CreateContext();
        var logger = new CapturingLogger<GetPropertyByIdHandler>();
        var handler = new GetPropertyByIdHandler(context, logger);
        var propertyId = Guid.NewGuid();

        var result = await handler.HandleAsync(new GetPropertyByIdQuery(propertyId), CancellationToken.None);

        Assert.Null(result);
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(propertyId, entry.Properties["PropertyId"]);
        Assert.Equal("notFound", entry.Properties["Outcome"]);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static Property CreateProperty()
    {
        return new Property
        {
            Id = Guid.NewGuid(),
            Title = "House",
            Description = "Private description",
            Address = "42 Harbor Road",
            Price = 420000m,
            Status = PropertyStatus.Rented,
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 142.5m,
            ImageUrl = "/images/properties/house.jpg"
        };
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var properties = state is IEnumerable<KeyValuePair<string, object?>> values
                ? values.ToDictionary(pair => pair.Key, pair => pair.Value)
                : new Dictionary<string, object?>();
            Entries.Add(new LogEntry(properties, formatter(state, exception)));
        }
    }

    private sealed record LogEntry(
        IReadOnlyDictionary<string, object?> Properties,
        string RenderedMessage);
}