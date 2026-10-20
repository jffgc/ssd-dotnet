using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using RealtorApi.Features.Properties.ListProperties;

namespace RealtorApiTests.Features.Properties.ListProperties;

public sealed class ListPropertiesSliceTests
{
    [Fact]
    public void AddEndpoint_Maps_Get_Api_Properties_Route()
    {
        using var app = WebApplication.CreateBuilder().Build();

        new ListPropertiesSlice().AddEndpoint(app);

        var endpoint = Assert.Single(((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>());

        Assert.Equal("/api/properties", endpoint.RoutePattern.RawText);
        Assert.Contains("GET", endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()!.HttpMethods);
    }
}
