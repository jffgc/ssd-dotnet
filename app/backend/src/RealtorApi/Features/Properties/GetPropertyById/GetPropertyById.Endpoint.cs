using Microsoft.AspNetCore.Mvc;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApi.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdEndpoint : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/properties/{id:guid}", async (
                Guid id,
                GetPropertyByIdHandler handler,
                CancellationToken cancellationToken) =>
            {
                var property = await handler.HandleAsync(id, cancellationToken);
                if (property is null)
                {
                    return Results.NotFound(new ProblemDetails
                    {
                        Title = "Property not found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = $"Property with id '{id}' was not found."
                    });
                }

                return Results.Ok(property);
            })
            .WithName("GetPropertyById")
            .WithSummary("Get a property by id")
            .WithDescription("Returns the property matching the specified identifier.")
            .Produces<GetPropertyByIdResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
