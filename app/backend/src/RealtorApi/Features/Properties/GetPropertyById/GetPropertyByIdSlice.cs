using Microsoft.AspNetCore.Mvc;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApi.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdSlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/properties/{id}",
                async (
                    [AsParameters] GetPropertyByIdRequest request,
                    [FromServices] GetPropertyByIdHandler handler,
                    HttpRequest httpRequest,
                    CancellationToken cancellationToken) =>
                {
                    var property = await handler.HandleAsync(request.ToQuery(), cancellationToken);
                    if (property is null)
                    {
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "No se encontró la propiedad.",
                            Status = StatusCodes.Status404NotFound,
                            Detail = "No existe una propiedad con el identificador solicitado."
                        });
                    }

                    return Results.Ok(property.ToResponse(httpRequest.Scheme, httpRequest.Host));
                })
            .WithName("GetPropertyById")
            .WithSummary("Consultar una propiedad por identificador")
            .WithDescription("Devuelve el detalle público de una propiedad identificada por un UUID canónico.")
            .Produces<GetPropertyByIdResponse>()
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}