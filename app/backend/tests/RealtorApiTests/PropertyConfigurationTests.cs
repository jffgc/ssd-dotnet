using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests;

public class PropertyConfigurationTests
{
    [Fact]
    public void Property_Status_Is_Configured_As_String_At_Model_Level()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var propertyType = context.Model.FindEntityType(typeof(Property));
        var statusProperty = propertyType?.FindProperty(nameof(Property.Status));

        Assert.NotNull(propertyType);
        Assert.NotNull(statusProperty);
        Assert.Equal(typeof(string), statusProperty!.GetProviderClrType());
    }
}
