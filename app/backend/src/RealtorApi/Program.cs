using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.RegisterHandlers(typeof(Program).Assembly);
builder.Services.RegisterSlices(typeof(Program).Assembly);

var app = builder.Build();

app.UseStaticFiles();

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

public partial class Program { }
