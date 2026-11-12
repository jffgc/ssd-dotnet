using RealtorApi.Features.Properties.UpdatePropertyStatus;

namespace RealtorApiTests.Features.Properties.UpdatePropertyStatus;

public sealed class UpdatePropertyStatusValidatorTests
{
    private static readonly IReadOnlyDictionary<string, string[]> NoBindingErrors =
        new Dictionary<string, string[]>();

    [Theory]
    [InlineData("9c2fcbcb-f94a-4c70-b04f-b24488b89ad1", "Available")]
    [InlineData("9C2FCBCB-F94A-4C70-B04F-B24488B89AD1", "Rented")]
    [InlineData("9c2fcbcb-f94a-4c70-b04f-b24488b89ad1", "Maintenance")]
    public async Task ValidateAsync_Accepts_Canonical_Id_And_Allowed_Status(string id, string status)
    {
        var validator = new UpdatePropertyStatusValidator();

        var result = await validator.ValidateAsync(
            new UpdatePropertyStatusRequest(id, status, NoBindingErrors));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("9c2fcbcbf94a4c70b04fb24488b89ad1")]
    [InlineData("{9c2fcbcb-f94a-4c70-b04f-b24488b89ad1}")]
    public async Task ValidateAsync_Rejects_Noncanonical_Id(string id)
    {
        var validator = new UpdatePropertyStatusValidator();

        var result = await validator.ValidateAsync(
            new UpdatePropertyStatusRequest(id, "Available", NoBindingErrors));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "id");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("available")]
    [InlineData("Sold")]
    public async Task ValidateAsync_Rejects_Missing_Or_Unrecognized_Status(string? status)
    {
        var validator = new UpdatePropertyStatusValidator();

        var result = await validator.ValidateAsync(
            new UpdatePropertyStatusRequest(
                "9c2fcbcb-f94a-4c70-b04f-b24488b89ad1",
                status,
                NoBindingErrors));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "status");
    }

    [Fact]
    public async Task ValidateAsync_Propagates_Binding_Errors_With_Their_Keys()
    {
        var validator = new UpdatePropertyStatusValidator();
        var bindingErrors = new Dictionary<string, string[]>
        {
            ["body"] = ["El body no contiene JSON válido."]
        };

        var result = await validator.ValidateAsync(
            new UpdatePropertyStatusRequest(
                "9c2fcbcb-f94a-4c70-b04f-b24488b89ad1",
                "Rented",
                bindingErrors));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "body");
    }
}
