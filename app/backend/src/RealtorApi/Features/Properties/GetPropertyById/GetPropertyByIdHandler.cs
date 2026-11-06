using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdHandler(
    AppDbContext context,
    ILogger<GetPropertyByIdHandler> logger) : IHandler
{
    public Task<GetPropertyByIdData?> HandleAsync(
        GetPropertyByIdQuery query,
        CancellationToken cancellationToken)
    {
        return HandleCoreAsync(query, cancellationToken);
    }

    private async Task<GetPropertyByIdData?> HandleCoreAsync(
        GetPropertyByIdQuery query,
        CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .AsNoTracking()
            .Where(item => item.Id == query.Id)
            .Select(item => new GetPropertyByIdData(
                item.Id,
                item.Title,
                item.Description,
                item.Address,
                item.Price,
                item.Status,
                item.BedroomCount,
                item.BathroomCount,
                item.AreaSquareMeters,
                item.ImageUrl))
            .FirstOrDefaultAsync(cancellationToken);

        var outcome = property is null ? "notFound" : "found";
        logger.LogInformation(
            "Property lookup for {PropertyId} completed with {Outcome}",
            query.Id,
            outcome);

        return property;
    }
}