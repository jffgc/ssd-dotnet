using System.Net;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RealtorApiTests;

public sealed class OpenApiSmokeTests
{
    [Fact]
    public async Task OpenApiV1_Is_Served_As_Static_Json()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        Assert.StartsWith("3.", document.RootElement.GetProperty("openapi").GetString());
    }

    [Theory]
    [InlineData("/swagger")]
    [InlineData("/swagger/index.html")]
    [InlineData("/redoc")]
    [InlineData("/scalar/v1")]
    public async Task Interactive_Documentation_Routes_Are_Not_Exposed(string path)
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void OpenApiV1_Document_Is_Consumable_By_NSwag()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..", ".."));
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("tool");
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("nswag");
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("support/scripts/openapi-v1.nswag.json");

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        Assert.True(process.WaitForExit(120_000), "NSwag client generation timed out.");
        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        Assert.True(process.ExitCode == 0, $"NSwag failed:{Environment.NewLine}{output}");
        Assert.True(File.Exists(Path.Combine(repoRoot, "artifacts", "openapi-client-smoke", "RealtorApiClient.cs")));
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var contentRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src",
            "RealtorApi"));

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(contentRoot);
            builder.UseEnvironment("OpenApiGeneration");
        });
    }
}
