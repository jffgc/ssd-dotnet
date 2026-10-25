using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.UpdateProperty;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests;

public class UpdatePropertyFeatureTests
{
    [Fact]
    public async Task UpdatePropertyHandler_Updates_Existing_Property_Without_Changing_ImageUrl_When_No_Image_Is_Provided()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var existing = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Casa inicial",
            Description = "Descripcion inicial",
            Address = "Calle inicial 1",
            Price = 150000m,
            Status = PropertyStatus.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 80m,
            ImageUrl = "/assets/properties/old-image.jpg",
            CreatedAt = DateTime.UtcNow
        };
        context.Properties.Add(existing);
        await context.SaveChangesAsync();

        var handler = new UpdatePropertyHandler(context);
        var request = new UpdatePropertyRequest
        {
            Title = "Casa actualizada",
            Description = "Descripcion actualizada",
            Address = "Calle nueva 2",
            Price = 170000m,
            Status = PropertyStatus.Rented,
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 95m,
            Image = null
        };

        var property = await handler.HandleAsync(existing.Id, request, CancellationToken.None);

        Assert.NotNull(property);
        Assert.Equal("Casa actualizada", property.Title);
        Assert.Equal("/assets/properties/old-image.jpg", property.ImageUrl);
        Assert.Equal(PropertyStatus.Rented, property.Status);
    }

    [Fact]
    public async Task UpdatePropertyHandler_Updates_ImageUrl_When_Image_Is_Valid()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var existing = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Casa con imagen",
            Description = "Descripcion",
            Address = "Direccion",
            Price = 500000m,
            Status = PropertyStatus.Available,
            BedroomCount = 4,
            BathroomCount = 3,
            AreaSquareMeters = 200m,
            ImageUrl = "/assets/properties/old-image.jpg",
            CreatedAt = DateTime.UtcNow
        };
        context.Properties.Add(existing);
        await context.SaveChangesAsync();

        var handler = new UpdatePropertyHandler(context);
        var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var file = new FormFile(stream, 0, stream.Length, "Image", "new-house.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var request = new UpdatePropertyRequest
        {
            Title = "Casa nueva imagen",
            Description = "Descripcion nueva",
            Address = "Direccion nueva",
            Price = 560000m,
            Status = PropertyStatus.Maintenance,
            BedroomCount = 5,
            BathroomCount = 4,
            AreaSquareMeters = 240m,
            Image = file
        };

        var property = await handler.HandleAsync(existing.Id, request, CancellationToken.None);

        Assert.NotNull(property);
        Assert.NotNull(property.ImageUrl);
        Assert.StartsWith("/assets/properties/", property.ImageUrl!);
        Assert.Contains(".jpg", property.ImageUrl!, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual("/assets/properties/old-image.jpg", property.ImageUrl);
    }

    [Fact]
    public async Task UpdatePropertyHandler_Returns_Null_For_Inexistent_Id()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var handler = new UpdatePropertyHandler(context);

        var request = new UpdatePropertyRequest
        {
            Title = "No existe",
            Description = "Desc",
            Address = "Dir",
            Price = 100m,
            Status = PropertyStatus.Available,
            BedroomCount = 1,
            BathroomCount = 1,
            AreaSquareMeters = 50m,
            Image = null
        };

        var property = await handler.HandleAsync(Guid.NewGuid(), request, CancellationToken.None);

        Assert.Null(property);
    }

    [Fact]
    public void UpdatePropertyValidator_Rejects_Unsupported_Image_Type()
    {
        var validator = new UpdatePropertyValidator();
        var request = new UpdatePropertyRequest
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
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(UpdatePropertyRequest.Image));
    }

    [Fact]
    public void UpdatePropertyValidator_Rejects_File_Over_5MB()
    {
        var validator = new UpdatePropertyValidator();
        var request = new UpdatePropertyRequest
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
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(UpdatePropertyRequest.Image));
    }
}
