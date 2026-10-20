using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.CreateProperty;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests;

public class CreatePropertyFeatureTests
{
    [Fact]
    public async Task CreatePropertyHandler_Creates_Property_With_Null_ImageUrl_When_No_Image_Is_Provided()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var handler = new CreatePropertyHandler(context);

        var request = new CreatePropertyRequest
        {
            Title = "Casa de prueba",
            Description = "Propiedad de ejemplo",
            Address = "Calle Falsa 123",
            Price = 250000m,
            Status = PropertyStatus.Available,
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 120m,
            Image = null
        };

        var property = await handler.HandleAsync(request, CancellationToken.None);

        Assert.NotNull(property);
        Assert.Equal("Casa de prueba", property.Title);
        Assert.Null(property.ImageUrl);
        Assert.Equal(1, await context.Properties.CountAsync());
    }

    [Fact]
    public async Task CreatePropertyHandler_Creates_Property_With_Public_Image_Url_When_Image_Is_Valid()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var handler = new CreatePropertyHandler(context);

        var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var file = new FormFile(stream, 0, stream.Length, "Image", "house.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var request = new CreatePropertyRequest
        {
            Title = "Casa con imagen",
            Description = "Propiedad con foto",
            Address = "Avenida Siempre Viva 742",
            Price = 340000m,
            Status = PropertyStatus.Available,
            BedroomCount = 4,
            BathroomCount = 3,
            AreaSquareMeters = 180m,
            Image = file
        };

        var property = await handler.HandleAsync(request, CancellationToken.None);

        Assert.NotNull(property);
        Assert.NotNull(property.ImageUrl);
        Assert.StartsWith("/assets/properties/", property.ImageUrl!);
        Assert.Contains(".jpg", property.ImageUrl!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreatePropertyValidator_Rejects_Unsupported_Image_Type()
    {
        var validator = new CreatePropertyValidator();
        var request = new CreatePropertyRequest
        {
            Title = "Casa",
            Description = "Desc",
            Address = "Address",
            Price = 100m,
            Status = PropertyStatus.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 60m,
            Image = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "Image", "house.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            }
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreatePropertyRequest.Image));
    }

    [Fact]
    public void CreatePropertyValidator_Rejects_File_Over_5MB()
    {
        var validator = new CreatePropertyValidator();
        var request = new CreatePropertyRequest
        {
            Title = "Casa",
            Description = "Desc",
            Address = "Address",
            Price = 100m,
            Status = PropertyStatus.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 60m,
            Image = new FormFile(new MemoryStream(new byte[6 * 1024 * 1024]), 0, 6 * 1024 * 1024, "Image", "big.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            }
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreatePropertyRequest.Image));
    }
}
