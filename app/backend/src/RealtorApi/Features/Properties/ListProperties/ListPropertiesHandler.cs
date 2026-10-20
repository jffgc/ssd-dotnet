using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed class ListPropertiesHandler(
    AppDbContext context,
    ILogger<ListPropertiesHandler> logger) : IHandler
{
    public async Task<ListPropertiesQueryResult> HandleAsync(
        ListPropertiesQuery query,
        CancellationToken cancellationToken)
    {
        var properties = context.Properties.AsNoTracking();
        var totalItems = await properties.LongCountAsync(cancellationToken);
        var skip = ((long)query.Page - 1) * query.PageSize;

        var items = skip > int.MaxValue
            ? []
            : await properties
                .OrderBy(property => property.Title)
                .Skip((int)skip)
                .Take(query.PageSize)
                .Select(property => new ListPropertyData(
                    property.Id,
                    property.Title,
                    property.Description,
                    property.Address,
                    property.Price,
                    property.Status,
                    property.BedroomCount,
                    property.BathroomCount,
                    property.AreaSquareMeters,
                    property.ImageUrl))
                .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Listed properties for page {Page} with page size {PageSize}; total items {TotalItems}",
            query.Page,
            query.PageSize,
            totalItems);

        return new ListPropertiesQueryResult(items, query.Page, query.PageSize, totalItems);
    }
}
