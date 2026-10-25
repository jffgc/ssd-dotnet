using Microsoft.AspNetCore.Mvc;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Validation;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed class CreatePropertyEndpoint : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/properties", async ([FromForm] CreatePropertyRequest request,
            CreatePropertyHandler handler,
            CancellationToken cancellationToken) =>
        {
            var property = await handler.HandleAsync(request, cancellationToken);
            var response = property.ToResponse();

            return Results.Created($"/api/properties/{property.Id}", response);
        });
    }
}
