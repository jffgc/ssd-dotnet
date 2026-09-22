using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApi.Features.Health;

public sealed class HealthSlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
    }
}
