using RealtorApi.Features.Properties.GetPropertyById;

namespace RealtorApiTests.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdValidatorTests
{
    [Theory]
    [InlineData("9c2fcbcb-f94a-4c70-b04f-b24488b89ad1")]
    [InlineData("9C2FCBCB-F94A-4C70-B04F-B24488B89AD1")]
    public async Task ValidateAsync_Accepts_Canonical_Guid(string id)
    {
        var validator = new GetPropertyByIdValidator();

        var result = await validator.ValidateAsync(new GetPropertyByIdRequest { Id = id });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("9c2fcbcbf94a4c70b04fb24488b89ad1")]
    [InlineData("{9c2fcbcb-f94a-4c70-b04f-b24488b89ad1}")]
    [InlineData("(9c2fcbcb-f94a-4c70-b04f-b24488b89ad1)")]
    public async Task ValidateAsync_Rejects_Noncanonical_Guid_Formats(string id)
    {
        var validator = new GetPropertyByIdValidator();

        var result = await validator.ValidateAsync(new GetPropertyByIdRequest { Id = id });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "id");
    }
}