using RealtorApi.Features.Properties.ListProperties;

namespace RealtorApiTests.Features.Properties.ListProperties;

public sealed class ListPropertiesValidatorTests
{
    [Fact]
    public async Task ValidateAsync_Accepts_Omitted_Values_And_Normalizes_Defaults()
    {
        var request = new ListPropertiesRequest();
        var validator = new ListPropertiesValidator();

        var result = await validator.ValidateAsync(request);
        var normalized = request.ToQuery();

        Assert.True(result.IsValid);
        Assert.Equal(1, normalized.Page);
        Assert.Equal(6, normalized.PageSize);
    }

    [Theory]
    [InlineData("not-a-number", "6", "page")]
    [InlineData("1", "not-a-number", "pageSize")]
    [InlineData("0", "6", "page")]
    [InlineData("-1", "6", "page")]
    [InlineData("1", "0", "pageSize")]
    [InlineData("1", "-1", "pageSize")]
    [InlineData("2147483648", "6", "page")]
    [InlineData("1", "101", "pageSize")]
    public async Task ValidateAsync_Rejects_Invalid_Query_Values_On_Their_Fields(
        string page,
        string pageSize,
        string expectedField)
    {
        var validator = new ListPropertiesValidator();

        var result = await validator.ValidateAsync(new ListPropertiesRequest
        {
            Page = page,
            PageSize = pageSize
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == expectedField);
    }

    [Theory]
    [InlineData("2147483647", "6")]
    [InlineData("1", "100")]
    public async Task ValidateAsync_Accepts_Maximum_Representable_Values(string page, string pageSize)
    {
        var validator = new ListPropertiesValidator();

        var result = await validator.ValidateAsync(new ListPropertiesRequest
        {
            Page = page,
            PageSize = pageSize
        });

        Assert.True(result.IsValid);
    }
}
