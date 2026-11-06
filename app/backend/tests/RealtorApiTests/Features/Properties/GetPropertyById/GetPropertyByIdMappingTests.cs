using Microsoft.AspNetCore.Http;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.GetPropertyById;

namespace RealtorApiTests.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdMappingTests
{
    [Fact]
    public void ToResponse_Maps_All_Public_Fields_And_Uses_Absolute_Image_Url()
    {
        var data = CreateData(@"C:\private\images\properties\harbor house.png");

        var response = data.ToResponse("https", new HostString("catalog.example:8443"));

        Assert.Equal(data.Id, response.Id);
        Assert.Equal(data.Title, response.Title);
        Assert.Equal(data.Description, response.Description);
        Assert.Equal(data.Address, response.Address);
        Assert.Equal(data.Price, response.Price);
        Assert.Equal(nameof(PropertyStatus.Rented), response.Status);
        Assert.Equal(data.BedroomCount, response.BedroomCount);
        Assert.Equal(data.BathroomCount, response.BathroomCount);
        Assert.Equal(data.AreaSquareMeters, response.AreaSquareMeters);
        Assert.Equal("https://catalog.example:8443/assets/properties/harbor%20house.png", response.ImageUrl);
        Assert.Equal(
            new[]
            {
                nameof(GetPropertyByIdResponse.Id),
                nameof(GetPropertyByIdResponse.Title),
                nameof(GetPropertyByIdResponse.Description),
                nameof(GetPropertyByIdResponse.Address),
                nameof(GetPropertyByIdResponse.Price),
                nameof(GetPropertyByIdResponse.Status),
                nameof(GetPropertyByIdResponse.BedroomCount),
                nameof(GetPropertyByIdResponse.BathroomCount),
                nameof(GetPropertyByIdResponse.AreaSquareMeters),
                nameof(GetPropertyByIdResponse.ImageUrl)
            }.OrderBy(name => name),
            typeof(GetPropertyByIdResponse).GetProperties().Select(property => property.Name).OrderBy(name => name));
    }

    [Fact]
    public void ToResponse_Preserves_Null_Image_Url()
    {
        var response = CreateData(null).ToResponse("http", new HostString("catalog.example"));

        Assert.Null(response.ImageUrl);
    }

    [Theory]
    [InlineData("http", "localhost:5116", "http://localhost:5116/assets/properties/house.jpg")]
    [InlineData("https", "catalog.example:8443", "https://catalog.example:8443/assets/properties/house.jpg")]
    public void ToResponse_Builds_Image_Url_From_Current_Scheme_And_Host(
        string scheme,
        string host,
        string expectedImageUrl)
    {
        var response = CreateData("/images/properties/house.jpg")
            .ToResponse(scheme, new HostString(host));

        Assert.Equal(expectedImageUrl, response.ImageUrl);
    }

    private static GetPropertyByIdData CreateData(string? imageUrl)
    {
        return new GetPropertyByIdData(
            Id: Guid.Parse("8e099de1-1a78-4fdd-a42f-1b687cb08f45"),
            Title: "Harbor House",
            Description: "Bright home near the water",
            Address: "42 Harbor Road",
            Price: 420000m,
            Status: PropertyStatus.Rented,
            BedroomCount: 3,
            BathroomCount: 2,
            AreaSquareMeters: 142.5m,
            ImageUrl: imageUrl);
    }
}