using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Features.Properties.UpdatePropertyStatus;

public sealed class UpdatePropertyStatusHandler(
    AppDbContext context,
    ILogger<UpdatePropertyStatusHandler> logger) : IHandler
{
    public async Task<UpdatePropertyStatusResponse?> HandleAsync(
        UpdatePropertyStatusRequest request,
        CancellationToken cancellationToken)
    {
        var propertyId = Guid.ParseExact(request.Id!, "D");
        var property = await context.Properties
            .FirstOrDefaultAsync(item => item.Id == propertyId, cancellationToken);

        if (property is null)
        {
            logger.LogInformation(
                "Property status update for {PropertyId} completed with {Outcome}",
                propertyId,
                "notFound");
            return null;
        }

        property.Status = request.Status!.ToPropertyStatus();
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Property status update for {PropertyId} completed with {Outcome}",
            propertyId,
            "updated");

        return property.ToUpdatePropertyStatusResponse();
    }
}
