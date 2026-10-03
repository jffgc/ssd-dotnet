using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Property> Properties => Set<Property>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

public static class AppDbContextBuilderExtensions
{
    public static DbContextOptionsBuilder UseRealtorSeeding(this DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder
            .UseSeeding((context, _) => DatabaseSeeder.Seed((AppDbContext)context))
            .UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                await DatabaseSeeder.SeedAsync((AppDbContext)context, cancellationToken);
            });
    }
}
