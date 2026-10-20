using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseRealtorSeeding());

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Scoped);
builder.Services.RegisterHandlers(typeof(Program).Assembly);
builder.Services.RegisterSlices(typeof(Program).Assembly);

var app = builder.Build();

app.UseStaticFiles();
var propertiesWebRootPath = app.Environment.WebRootPath
    ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(Path.Combine(propertiesWebRootPath, "images", "properties")))
{
    propertiesWebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
}

app.UseStaticFiles(Program.CreatePropertiesAssetsOptions(propertiesWebRootPath));

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(context.Request.Path.Value ?? "unknown", "An unhandled exception occurred");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred."
        });
    });
});

app.MapSliceEndpoints();

await app.MigrateAsync();

app.Run();

public partial class Program
{
    public static StaticFileOptions CreatePropertiesAssetsOptions(string webRootPath)
    {
        var propertiesDirectory = Path.GetFullPath(Path.Combine(webRootPath, "images", "properties"));

        return new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(propertiesDirectory),
            RequestPath = "/assets/properties"
        };
    }
}
