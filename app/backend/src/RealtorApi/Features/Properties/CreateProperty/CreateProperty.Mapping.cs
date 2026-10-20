using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.CreateProperty;

public static class CreatePropertyMapping
{
    public static Property ToEntity(this CreatePropertyRequest request, string? imageUrl = null)
    {
        return new Property
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Address = request.Address.Trim(),
            Price = request.Price,
            Status = request.Status,
            BedroomCount = request.BedroomCount,
            BathroomCount = request.BathroomCount,
            AreaSquareMeters = request.AreaSquareMeters,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    public static CreatePropertyResponse ToResponse(this Property property)
    {
        return new CreatePropertyResponse
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
