using System.Text.Json;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence;

public sealed class PropertiesSeed
{
    public static IReadOnlyList<Property> LoadProperties()
    {
        var filePath = ResolveSeedPath("properties.json");
        var json = File.ReadAllText(filePath);
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
            ImageUrl = $"/images/properties/{record.ImageUrl.TrimStart('/')}" ,
            CreatedAt = DateTime.Parse(record.CreatedAt),
            UpdatedAt = string.IsNullOrWhiteSpace(record.UpdatedAt) ? null : DateTime.Parse(record.UpdatedAt)
        }).ToList();
    }

    private static string ResolveSeedPath(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "support", "seed-data", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "support", "seed-data", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "support", "seed-data", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "support", "seed-data", fileName)
        };

        foreach (var candidate in candidates)
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
}
