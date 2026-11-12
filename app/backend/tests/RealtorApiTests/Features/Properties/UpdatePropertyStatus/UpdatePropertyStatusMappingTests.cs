using System.Text.Json;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.UpdatePropertyStatus;

namespace RealtorApiTests.Features.Properties.UpdatePropertyStatus;

public sealed class UpdatePropertyStatusMappingTests
{
    [Theory]
    [InlineData("Available", PropertyStatus.Available)]
    [InlineData("Rented", PropertyStatus.Rented)]
    [InlineData("Maintenance", PropertyStatus.Maintenance)]
    public void ToPropertyStatus_Maps_Canonical_Status(string status, PropertyStatus expected)
    {
        Assert.Equal(expected, status.ToPropertyStatus());
    }

    [Theory]
    [InlineData(PropertyStatus.Available, "Available")]
    [InlineData(PropertyStatus.Rented, "Rented")]
    [InlineData(PropertyStatus.Maintenance, "Maintenance")]
    public void ToUpdatePropertyStatusResponse_Contains_Only_Id_And_Status(
        PropertyStatus status,
        string expectedStatus)
    {
        var property = new Property
        {
            Id = Guid.Parse("9c2fcbcb-f94a-4c70-b04f-b24488b89ad1"),
            Status = status
        };

        var response = property.ToUpdatePropertyStatusResponse();
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var properties = document.RootElement.EnumerateObject().ToArray();
        Assert.Equal(2, properties.Length);
        Assert.Equal("id", properties[0].Name);
        Assert.Equal(property.Id, properties[0].Value.GetGuid());
        Assert.Equal("status", properties[1].Name);
        Assert.Equal(expectedStatus, properties[1].Value.GetString());
    }
}