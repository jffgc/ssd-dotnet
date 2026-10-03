using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext context)
    {
        SeedProperties(context);
    }

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedPropertiesAsync(context, cancellationToken);
    }

    private static void SeedProperties(AppDbContext context)
    {
        var properties = LoadPropertiesSeed();

        foreach (var property in properties)
        {
            if (context.Properties.Any(p => p.Id == property.Id))
            {
                continue;
            }

            context.Properties.Add(property);
        }

        context.SaveChanges();
    }

    private static async Task SeedPropertiesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var properties = LoadPropertiesSeed();

        foreach (var property in properties)
        {
            if (await context.Properties.AnyAsync(p => p.Id == property.Id, cancellationToken))
            {
                continue;
            }

            context.Properties.Add(property);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<Property> LoadPropertiesSeed()
    {
        var path = ResolveSeedFilePath("properties.json");
        var json = File.ReadAllText(path);
        var records = JsonSerializer.Deserialize<List<PropertySeedRecord>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];

        return records.Select(record => new Property
        {
            Id = Guid.Parse(record.Id),
            Title = record.Title,
            Description = record.Description,
            Address = record.Address,
            Price = record.Price,
            Status = Enum.Parse<PropertyStatus>(record.Status, ignoreCase: true),
            BedroomCount = record.BedroomCount,
            BathroomCount = record.BathroomCount,
            AreaSquareMeters = record.AreaSquareMeters,
            ImageUrl = ResolvePublicImageUrl(record.ImageUrl),
            CreatedAt = DateTime.SpecifyKind(DateTime.Parse(record.CreatedAt), DateTimeKind.Utc),
            UpdatedAt = string.IsNullOrWhiteSpace(record.UpdatedAt)
                ? null
                : DateTime.SpecifyKind(DateTime.Parse(record.UpdatedAt), DateTimeKind.Utc)
        }).ToList();
    }

    private static string ResolvePublicImageUrl(string imageName)
    {
        var manifest = LoadManifest();
        var relative = string.IsNullOrWhiteSpace(imageName) ? string.Empty : imageName.Trim();
        return string.IsNullOrWhiteSpace(relative)
            ? string.Empty
            : $"{manifest.PublicImageRoot.TrimEnd('/')}/{relative.TrimStart('/')}";
    }

    private static SeedManifest LoadManifest()
    {
        var path = ResolveSeedFilePath("seed-manifest.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<SeedManifest>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new SeedManifest();
    }

    private static string ResolveSeedFilePath(string fileName)
    {
        var repoRoot = Directory.GetCurrentDirectory();
        var candidatePaths = new[]
        {
            Path.Combine(repoRoot, "support", "seed-data", fileName),
            Path.Combine(repoRoot, "..", "..", "..", "..", "support", "seed-data", fileName),
            Path.Combine(repoRoot, "..", "support", "seed-data", fileName),
            Path.Combine(AppContext.BaseDirectory, "support", "seed-data", fileName)
        };

        foreach (var candidate in candidatePaths)
        {
            var full = Path.GetFullPath(candidate);
            if (File.Exists(full))
            {
                return full;
            }
        }

        throw new FileNotFoundException($"Seed file not found: {fileName}", fileName);
    }

    private sealed class PropertySeedRecord
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public int BedroomCount { get; set; }
        public int BathroomCount { get; set; }
        public decimal AreaSquareMeters { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string? UpdatedAt { get; set; }
    }

    private sealed class SeedManifest
    {
        public string PublicImageRoot { get; set; } = "/images/properties";
    }
}
