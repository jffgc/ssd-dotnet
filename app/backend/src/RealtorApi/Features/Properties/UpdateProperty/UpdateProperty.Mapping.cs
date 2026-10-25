using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.UpdateProperty;

public static class UpdatePropertyMapping
{
    public static void ApplyUpdate(this Property property, UpdatePropertyRequest request, string? imageUrl = null)
    {
        property.Title = request.Title.Trim();
        property.Description = request.Description.Trim();
        property.Address = request.Address.Trim();
        property.Price = request.Price;
        property.Status = request.Status;
        property.BedroomCount = request.BedroomCount;
        property.BathroomCount = request.BathroomCount;
        property.AreaSquareMeters = request.AreaSquareMeters;
        if (imageUrl is not null)
        {
            property.ImageUrl = imageUrl;
        }
        property.UpdatedAt = DateTime.UtcNow;
    }

    public static UpdatePropertyResponse ToResponse(this Property property)
    {
        return new UpdatePropertyResponse
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Address = property.Address,
            Price = property.Price,
            Status = property.Status,
            BedroomCount = property.BedroomCount,
            BathroomCount = property.BathroomCount,
            AreaSquareMeters = property.AreaSquareMeters,
            ImageUrl = property.ImageUrl,
            CreatedAt = property.CreatedAt,
            UpdatedAt = property.UpdatedAt
        };
    }
}
