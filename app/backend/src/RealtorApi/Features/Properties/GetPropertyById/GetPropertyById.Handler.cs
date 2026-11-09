using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdHandler(AppDbContext context) : IHandler
{
    public Task<GetPropertyByIdResponse?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Properties
            .AsNoTracking()
            .Where(property => property.Id == id)
            .Select(property => new GetPropertyByIdResponse(
                property.Id,
                property.Title,
                property.Description,
                property.Address,
                property.Price,
                property.Status,
                property.BedroomCount,
                property.BathroomCount,
                property.AreaSquareMeters,
                property.ImageUrl,
                property.CreatedAt,
                property.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
