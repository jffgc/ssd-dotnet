using Microsoft.AspNetCore.Http;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.ListProperties;

namespace RealtorApiTests.Features.Properties.ListProperties;

public sealed class ListPropertiesMappingTests
{
    [Fact]
    public void ToResponse_Maps_All_Fields_And_Builds_Absolute_Image_Url_From_Safe_File_Name()
    {
        var item = new ListPropertyData(
            Id: Guid.Parse("8e099de1-1a78-4fdd-a42f-1b687cb08f45"),
            Title: "Harbor House",
            Description: "Bright home near the water",
            Address: "42 Harbor Road",
            Price: 420000m,
            Status: PropertyStatus.Rented,
            BedroomCount: 3,
            BathroomCount: 2,
            AreaSquareMeters: 142.5m,
            ImageUrl: @"C:\private\images\properties\harbor house.png");
        var result = new ListPropertiesQueryResult([item], Page: 2, PageSize: 3, TotalItems: 8);

        var response = result.ToResponse("https", new HostString("catalog.example:8443"));

        var mapped = Assert.Single(response.Items);
        Assert.Equal(item.Id, mapped.Id);
        Assert.Equal(item.Title, mapped.Title);
        Assert.Equal(item.Description, mapped.Description);
        Assert.Equal(item.Address, mapped.Address);
        Assert.Equal(item.Price, mapped.Price);
        Assert.Equal(nameof(PropertyStatus.Rented), mapped.Status);
        Assert.Equal(item.BedroomCount, mapped.BedroomCount);
        Assert.Equal(item.BathroomCount, mapped.BathroomCount);
        Assert.Equal(item.AreaSquareMeters, mapped.AreaSquareMeters);
        Assert.Equal("https://catalog.example:8443/assets/properties/harbor%20house.png", mapped.ImageUrl);
        Assert.Equal(2, response.Page);
        Assert.Equal(3, response.PageSize);
        Assert.Equal(8, response.TotalItems);
        Assert.Equal(3, response.TotalPages);
        Assert.True(response.HasNext);
        Assert.True(response.HasPrevious);
    }

    [Fact]
    public void ToResponse_Uses_Zero_Pages_And_Disables_Navigation_For_Empty_Catalog()
    {
        var result = new ListPropertiesQueryResult([], Page: 1, PageSize: 6, TotalItems: 0);

        var response = result.ToResponse("http", new HostString("localhost:5116"));

        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalPages);
        Assert.False(response.HasNext);
        Assert.False(response.HasPrevious);
    }

    [Fact]
    public void ToResponse_Uses_Previous_Flag_For_Out_Of_Range_Page()
    {
        var result = new ListPropertiesQueryResult([], Page: 9, PageSize: 3, TotalItems: 8);

        var response = result.ToResponse("http", new HostString("localhost:5116"));

        Assert.Equal(3, response.TotalPages);
        Assert.False(response.HasNext);
        Assert.True(response.HasPrevious);
    }
}
