using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Features.Properties.ListProperties;

public sealed class ListPropertiesHandlerTests
{
    [Fact]
    public async Task HandleAsync_Returns_Ordered_Page_And_Coherent_Metadata_Without_Tracking_Entities()
    {
        await using var context = CreateContext();
        context.Properties.AddRange(
            CreateProperty("Delta", "private description delta"),
            CreateProperty("Alpha", "private description alpha"),
            CreateProperty("Echo", "private description echo"),
            CreateProperty("Bravo", "private description bravo"),
            CreateProperty("Charlie", "private description charlie"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = new ListPropertiesHandler(context, new CapturingLogger<ListPropertiesHandler>());

        var queryResult = await handler.HandleAsync(new ListPropertiesQuery(Page: 2, PageSize: 2), CancellationToken.None);
        var response = queryResult.ToResponse("https", new HostString("catalog.example:8443"));

        Assert.Equal(new[] { "Charlie", "Delta" }, response.Items.Select(item => item.Title));
        Assert.Equal(2, response.Page);
        Assert.Equal(2, response.PageSize);
        Assert.Equal(5, response.TotalItems);
        Assert.Equal(3, response.TotalPages);
        Assert.True(response.HasNext);
        Assert.True(response.HasPrevious);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task HandleAsync_Returns_Empty_Page_And_Zero_Metadata_For_Empty_Catalog()
    {
        await using var context = CreateContext();
        var handler = new ListPropertiesHandler(context, new CapturingLogger<ListPropertiesHandler>());

        var response = (await handler.HandleAsync(
            new ListPropertiesQuery(Page: 1, PageSize: 6),
            CancellationToken.None)).ToResponse("http", new HostString("catalog.example"));

        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalItems);
        Assert.Equal(0, response.TotalPages);
        Assert.False(response.HasNext);
        Assert.False(response.HasPrevious);
    }

    [Fact]
    public async Task HandleAsync_Returns_Empty_Out_Of_Range_Page_With_Previous_Flag()
    {
        await using var context = CreateContext();
        context.Properties.Add(CreateProperty("Alpha", "description"));
        await context.SaveChangesAsync();
        var handler = new ListPropertiesHandler(context, new CapturingLogger<ListPropertiesHandler>());

        var response = (await handler.HandleAsync(
            new ListPropertiesQuery(Page: 4, PageSize: 1),
            CancellationToken.None)).ToResponse("http", new HostString("catalog.example"));

        Assert.Empty(response.Items);
        Assert.Equal(1, response.TotalItems);
        Assert.Equal(1, response.TotalPages);
        Assert.False(response.HasNext);
        Assert.True(response.HasPrevious);
    }

    [Fact]
    public async Task HandleAsync_Returns_Empty_Page_When_Skip_Would_Exceed_Int32()
    {
        await using var context = CreateContext();
        context.Properties.Add(CreateProperty("Alpha", "description"));
        await context.SaveChangesAsync();
        var handler = new ListPropertiesHandler(context, new CapturingLogger<ListPropertiesHandler>());

        var response = (await handler.HandleAsync(
            new ListPropertiesQuery(Page: int.MaxValue, PageSize: 100),
            CancellationToken.None)).ToResponse("http", new HostString("catalog.example"));

        Assert.Empty(response.Items);
        Assert.Equal(1, response.TotalItems);
        Assert.True(response.HasPrevious);
    }

    [Fact]
    public async Task HandleAsync_Propagates_Cancellation_To_EF_Core()
    {
        await using var context = CreateContext();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var handler = new ListPropertiesHandler(context, new CapturingLogger<ListPropertiesHandler>());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleAsync(new ListPropertiesQuery(Page: 1, PageSize: 6), cancellation.Token));
    }

    [Fact]
    public async Task HandleAsync_Logs_Only_Structured_Pagination_Values()
    {
        await using var context = CreateContext();
        context.Properties.Add(CreateProperty("Private title", "private description must not be logged"));
        await context.SaveChangesAsync();
        var logger = new CapturingLogger<ListPropertiesHandler>();
        var handler = new ListPropertiesHandler(context, logger);

        await handler.HandleAsync(new ListPropertiesQuery(Page: 1, PageSize: 6), CancellationToken.None);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(1, entry.Properties["Page"]);
        Assert.Equal(6, entry.Properties["PageSize"]);
        Assert.Equal(1L, entry.Properties["TotalItems"]);
        Assert.DoesNotContain("private description must not be logged", entry.RenderedMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("Private title", entry.RenderedMessage, StringComparison.Ordinal);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static Property CreateProperty(string title, string description)
    {
        return new Property
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Address = $"{title} address",
            Price = 125000m,
            Status = PropertyStatus.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 85m,
            ImageUrl = $"/images/properties/{title}.png"
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
            Entries.Add(new LogEntry(logLevel, properties, formatter(state, exception)));
        }
    }

    private sealed record LogEntry(
        LogLevel Level,
        IReadOnlyDictionary<string, object?> Properties,
        string RenderedMessage);
}
