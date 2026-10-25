using Microsoft.AspNetCore.Http;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.UpdateProperty;

public sealed class UpdatePropertyRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public PropertyStatus Status { get; set; }

    public int BedroomCount { get; set; }

    public int BathroomCount { get; set; }

    public decimal AreaSquareMeters { get; set; }

    public IFormFile? Image { get; set; }
}
