using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.UpdatePropertyStatus;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Features.Properties.UpdatePropertyStatus;

public sealed class UpdatePropertyStatusHandlerTests
{
    [Fact]
    public async Task HandleAsync_Changes_Only_Status_And_Logs_Updated()
    {
        await using var context = CreateContext();
        var property = CreateProperty();
        context.Properties.Add(property);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var logger = new CapturingLogger<UpdatePropertyStatusHandler>();
        var handler = new UpdatePropertyStatusHandler(context, logger);
        var original = CreatePropertySnapshot(property);

        var response = await handler.HandleAsync(
            CreateRequest(property.Id, "Rented"),
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(property.Id, response.Id);
        Assert.Equal("Rented", response.Status);

        context.ChangeTracker.Clear();
        var persisted = await context.Properties.SingleAsync(item => item.Id == property.Id);
        Assert.Equal(original.Id, persisted.Id);
        Assert.Equal(original.Title, persisted.Title);
        Assert.Equal(original.Description, persisted.Description);
        Assert.Equal(original.Address, persisted.Address);
        Assert.Equal(original.Price, persisted.Price);
        Assert.Equal(PropertyStatus.Rented, persisted.Status);
        Assert.Equal(original.BedroomCount, persisted.BedroomCount);
        Assert.Equal(original.BathroomCount, persisted.BathroomCount);
        Assert.Equal(original.AreaSquareMeters, persisted.AreaSquareMeters);
        Assert.Equal(original.ImageUrl, persisted.ImageUrl);
        Assert.Equal(original.CreatedAt, persisted.CreatedAt);
        Assert.Equal(original.UpdatedAt, persisted.UpdatedAt);

        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(property.Id, logEntry.Properties["PropertyId"]);
        Assert.Equal("updated", logEntry.Properties["Outcome"]);
        Assert.DoesNotContain(property.Title, logEntry.RenderedMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(property.Description, logEntry.RenderedMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_Returns_Null_And_Logs_NotFound_When_Property_Is_Missing()
    {
        await using var context = CreateContext();
        var logger = new CapturingLogger<UpdatePropertyStatusHandler>();
        var handler = new UpdatePropertyStatusHandler(context, logger);
        var propertyId = Guid.NewGuid();

        var response = await handler.HandleAsync(
            CreateRequest(propertyId, "Rented"),
            CancellationToken.None);

        Assert.Null(response);
        Assert.Empty(context.Properties);
        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(propertyId, logEntry.Properties["PropertyId"]);
        Assert.Equal("notFound", logEntry.Properties["Outcome"]);
    }

    [Fact]
    public async Task HandleAsync_Propagates_Cancellation_To_EF_Core()
    {
        await using var context = CreateContext();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var handler = new UpdatePropertyStatusHandler(
            context,
            new CapturingLogger<UpdatePropertyStatusHandler>());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleAsync(
                CreateRequest(Guid.NewGuid(), "Available"),
                cancellation.Token));
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
            Status = PropertyStatus.Available,
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 142.5m,
            ImageUrl = "/assets/properties/house.jpg",
            CreatedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 2, 3, 4, 5, 6, DateTimeKind.Utc)
        };
    }

    private static UpdatePropertyStatusRequest CreateRequest(Guid id, string status)
    {
        return new UpdatePropertyStatusRequest(
            id.ToString("D"),
            status,
            new Dictionary<string, string[]>());
    }

    private static PropertySnapshot CreatePropertySnapshot(Property property)
    {
        return new PropertySnapshot(
            property.Id,
            property.Title,
            property.Description,
            property.Address,
            property.Price,
            property.BedroomCount,
            property.BathroomCount,
            property.AreaSquareMeters,
            property.ImageUrl,
            property.CreatedAt,
            property.UpdatedAt);
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

    private sealed record PropertySnapshot(
        Guid Id,
        string Title,
        string Description,
        string Address,
        decimal Price,
        int BedroomCount,
        int BathroomCount,
        decimal AreaSquareMeters,
        string? ImageUrl,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}