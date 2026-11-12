using Microsoft.AspNetCore.Mvc;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApi.Features.Properties.UpdatePropertyStatus;

public sealed class UpdatePropertyStatusEndpoint : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/api/properties/{id}/status",
                async (
                    UpdatePropertyStatusRequest request,
                    UpdatePropertyStatusHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var response = await handler.HandleAsync(request, cancellationToken);
                    if (response is null)
                    {
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "No se encontró la propiedad.",
                            Status = StatusCodes.Status404NotFound,
                            Detail = $"No existe una propiedad con el identificador '{request.Id}'."
                        });
                    }

                    return Results.Ok(response);
                })
            .WithName("UpdatePropertyStatus")
            .WithSummary("Actualizar el estado de una propiedad")
            .WithDescription("Modifica exclusivamente el estado de una propiedad existente.")
            .Accepts<UpdatePropertyStatusBody>("application/json")
            .Produces<UpdatePropertyStatusResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .AllowAnonymous();
    }
}
