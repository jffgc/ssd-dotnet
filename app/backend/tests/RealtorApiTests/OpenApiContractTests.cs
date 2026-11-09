using System.Text.Json;
using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Validation;

namespace RealtorApiTests;

public sealed class OpenApiContractTests
{
    [Fact]
    public void Runtime_Endpoints_Contain_The_Public_Contract_Inventory()
    {
        var app = CreateApp();
        var runtimeOperations = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .SelectMany(endpoint =>
            {
                var route = NormalizeRoute(endpoint.RoutePattern.RawText ?? string.Empty);
                var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [];
                return methods.Select(method => $"{method.ToUpperInvariant()} {route}");
            })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        using var json = JsonDocument.Parse(File.ReadAllText(GetOpenApiDocumentPath()));
        var documentedOperations = json.RootElement
            .GetProperty("paths")
            .EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject()
                .Select(method => $"{method.Name.ToUpperInvariant()} {NormalizeRoute(path.Name)}"))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Equal(
            runtimeOperations.OrderBy(operation => operation),
            documentedOperations.OrderBy(operation => operation));
    }

    [Fact]
    public void OpenApiV1_Document_Is_Present_And_Contains_Required_Endpoints()
    {
        var docPath = GetOpenApiDocumentPath();

        Assert.True(File.Exists(docPath), $"Expected OpenAPI file at: {docPath}");

        using var json = JsonDocument.Parse(File.ReadAllText(docPath));
        var root = json.RootElement;

        Assert.StartsWith("3.", root.GetProperty("openapi").GetString());
        var paths = root.GetProperty("paths");

        Assert.True(paths.GetProperty("/health").TryGetProperty("get", out _));
        Assert.True(paths.GetProperty("/api/properties").TryGetProperty("get", out _));
        Assert.True(paths.GetProperty("/api/properties").TryGetProperty("post", out _));
        Assert.True(paths.GetProperty("/api/properties/{id}").TryGetProperty("get", out _));
        Assert.True(paths.GetProperty("/api/properties/{id}").TryGetProperty("put", out _));

        var schemas = root.GetProperty("components").GetProperty("schemas");
        Assert.True(schemas.TryGetProperty("PropertyStatus", out _));
        Assert.True(schemas.TryGetProperty("ProblemDetails", out _));
        Assert.True(schemas.TryGetProperty("HttpValidationProblemDetails", out _));
    }

    [Fact]
    public void OpenApiV1_Document_Passes_Redocly_Lint()
    {
        var repoRoot = GetRepoRoot();
        var startInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "npx",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        if (OperatingSystem.IsWindows())
        {
            startInfo.ArgumentList.Add("/d");
            startInfo.ArgumentList.Add("/c");
            startInfo.ArgumentList.Add("npx --yes @redocly/cli lint --config .redocly.yaml app/backend/src/RealtorApi/wwwroot/openapi/v1.json");
        }
        else
        {
            startInfo.ArgumentList.Add("--yes");
            startInfo.ArgumentList.Add("@redocly/cli");
            startInfo.ArgumentList.Add("lint");
            startInfo.ArgumentList.Add("--config");
            startInfo.ArgumentList.Add(".redocly.yaml");
            startInfo.ArgumentList.Add("app/backend/src/RealtorApi/wwwroot/openapi/v1.json");
        }

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        Assert.True(process.WaitForExit(120_000), "Redocly lint timed out.");
        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        Assert.True(process.ExitCode == 0, $"Redocly lint failed:{Environment.NewLine}{output}");
    }

    private static WebApplication CreateApp()
    {
        var builder = WebApplication.CreateBuilder();
        var apiAssembly = typeof(RealtorApi.Features.Health.HealthSlice).Assembly;
        builder.Services.RegisterSlices(apiAssembly);
        builder.Services.RegisterHandlers(apiAssembly);
        builder.Services.AddValidatorsFromAssembly(apiAssembly, ServiceLifetime.Scoped);
        var app = builder.Build();
        app.MapSliceEndpoints();
        return app;
    }

    private static string GetOpenApiDocumentPath()
    {
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src",
            "RealtorApi",
            "wwwroot",
            "openapi",
            "v1.json"));
    }

    private static string GetRepoRoot()
    {
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..", ".."));
    }

    private static string NormalizeRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return string.Empty;
        }

        return route
            .Replace("{id:guid}", "{id}")
            .Replace("{id:int}", "{id}")
            .Replace("{id:long}", "{id}");
    }

    private sealed class HealthSliceMarker;
}
