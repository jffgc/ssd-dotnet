using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed class ListPropertiesSlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/properties",
                async (
                    [AsParameters] ListPropertiesRequest request,
                    [FromServices] ListPropertiesHandler handler,
                    HttpRequest httpRequest,
                    CancellationToken cancellationToken) =>
                {
                    var queryResult = await handler.HandleAsync(request.ToQuery(), cancellationToken);
                    return queryResult.ToResponse(httpRequest.Scheme, httpRequest.Host);
                })
            .WithName("ListProperties")
            .WithSummary("List properties with pagination")
            .WithDescription("Returns properties ordered by title with pagination metadata and public image URLs.")
            .Produces<ListPropertiesResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }
}
