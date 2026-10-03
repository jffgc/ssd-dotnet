using Microsoft.EntityFrameworkCore;

namespace RealtorApi.Infrastructure.Persistence;

public static class MigrationExtensions
{
    public static async Task MigrateAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (context.Database.IsRelational())
        {
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.MigrateAsync(context.Database);
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }
    }
}
