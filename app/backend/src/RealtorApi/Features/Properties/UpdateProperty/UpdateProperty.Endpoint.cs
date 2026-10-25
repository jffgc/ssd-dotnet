using Microsoft.AspNetCore.Mvc;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Validation;

namespace RealtorApi.Features.Properties.UpdateProperty;

public sealed class UpdatePropertyEndpoint : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/properties/{id:guid}", async (
                Guid id,
                [FromForm] UpdatePropertyRequest request,
                UpdatePropertyHandler handler,
                CancellationToken cancellationToken) =>
            {
                var property = await handler.HandleAsync(id, request, cancellationToken);
                if (property is null)
                {
                    return Results.NotFound(new ProblemDetails
                    {
                        Title = "Property not found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = $"Property with id '{id}' was not found."
                    });
                }

                return Results.Ok(property.ToResponse());
            })
            .WithName("UpdateProperty")
            .WithSummary("Update an existing property")
            .WithDescription("Updates editable property data and optionally stores a new public image.")
            .Produces<UpdatePropertyResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }
}
