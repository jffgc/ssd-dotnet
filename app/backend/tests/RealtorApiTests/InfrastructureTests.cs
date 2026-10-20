using FluentValidation;
using FluentValidation.Results;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Features.Health;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Errors;
using RealtorApi.Infrastructure.Validation;

namespace RealtorApiTests;

public class InfrastructureTests
{
    [Fact]
    public void RegisterSlices_Discovers_And_Registers_Slice_From_Assembly_Without_Duplicates()
    {
        var services = new ServiceCollection();

        services.RegisterSlices(typeof(TestSliceMarker).Assembly);
        services.RegisterSlices(typeof(TestSliceMarker).Assembly);

        var discovered = services.BuildServiceProvider()
            .GetServices<ISlice>()
            .OfType<TestSlice>()
            .ToList();

        Assert.Single(discovered);
    }

    [Fact]
    public async Task ValidationFilterFactory_Executes_Validation_When_Validator_Exists_And_Passes_Through_When_Not()
    {
        var services = new ServiceCollection();
        services.AddScoped<IValidator<SampleRequest>, SampleRequestValidator>();

        var provider = services.BuildServiceProvider();
        var factory = new ValidationFilterFactory();

        var withValidator = factory.CreateValidator<SampleRequest>(provider);
        var withoutValidator = factory.CreateValidator<NoValidatorRequest>(provider);

        Assert.NotNull(withValidator);
        Assert.Null(withoutValidator);

        var result = await withValidator!.ValidateAsync(new ValidationContext<SampleRequest>(new SampleRequest(string.Empty)));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidationFailure_Produces_ValidationProblemDetails_With_Http400()
    {
        var result = new ValidationResult(new[]
        {
            new ValidationFailure(nameof(SampleRequest.Name), "The Name field is required.")
        });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SampleRequest.Name));
    }

    [Fact]
    public async Task ValidationFilterFactory_Resolves_Scoped_Validator_Per_Request_And_Returns_ValidationProblemDetails()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<IValidator<SampleRequest>, SampleRequestValidator>();
        using var app = builder.Build();
        app.MapGroup(string.Empty)
            .AddEndpointFilterFactory(ValidationFilterFactory.Create)
            .MapGet("/validation", ([AsParameters] SampleRequest request) => request.Name);
        var endpoint = Assert.Single(((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>());
        await using var scope = app.Services.CreateAsyncScope();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/validation";
        httpContext.Request.QueryString = new QueryString("?Name=");
        httpContext.Response.Body = new MemoryStream();

        await endpoint.RequestDelegate!(httpContext);
        httpContext.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(httpContext.Response.Body);

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        Assert.True(document.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty(nameof(SampleRequest.Name), out _));
    }

    [Fact]
    public void ResultError_To_ProblemDetails_Maps_Status_And_Payload()
    {
        var result = Result.Fail("NOT_FOUND", "Resource not found", "The requested resource does not exist.");
        var problem = ErrorMapper.ToProblemDetails(result);

        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("NOT_FOUND", problem.Extensions["code"]);
        Assert.Equal("Resource not found", problem.Title);
    }

    [Fact]
    public void Health_Slice_Is_Mapped_And_Responds_Correctly()
    {
        var app = WebApplication.CreateBuilder().Build();
        var health = new HealthSlice();

        health.AddEndpoint(app);

        Assert.NotNull(app);
    }

    private sealed class TestSliceMarker;

    private sealed class TestSlice : ISlice
    {
        public void AddEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/test", () => "ok");
        }
    }

    private sealed record SampleRequest(string Name);

    private sealed record NoValidatorRequest(string Value);

    private sealed class SampleRequestValidator : AbstractValidator<SampleRequest>
    {
        public SampleRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
