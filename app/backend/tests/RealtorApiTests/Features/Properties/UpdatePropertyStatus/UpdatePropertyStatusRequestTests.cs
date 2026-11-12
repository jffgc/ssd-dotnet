using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using RealtorApi.Features.Properties.UpdatePropertyStatus;

namespace RealtorApiTests.Features.Properties.UpdatePropertyStatus;

public sealed class UpdatePropertyStatusRequestTests
{
    private static readonly ParameterInfo BindingParameter =
        typeof(UpdatePropertyStatusRequestTests)
            .GetMethod(nameof(BindEndpoint), BindingFlags.NonPublic | BindingFlags.Static)!
            .GetParameters()[0];

    [Theory]
    [InlineData("9c2fcbcb-f94a-4c70-b04f-b24488b89ad1", "Available")]
    [InlineData("9C2FCBCB-F94A-4C70-B04F-B24488B89AD1", "Rented")]
    [InlineData("9c2fcbcb-f94a-4c70-b04f-b24488b89ad1", "Maintenance")]
    public async Task BindAsync_Reads_Id_And_Status_From_Request(string id, string status)
    {
        var context = new DefaultHttpContext();
        context.Request.RouteValues["id"] = id;
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes($"{{\"status\":\"{status}\"}}"));

        var request = await UpdatePropertyStatusRequest.BindAsync(context, BindingParameter);

        Assert.NotNull(request);
        Assert.Equal(id, request.Id);
        Assert.Equal(status, request.Status);
        Assert.Empty(request.BindingErrors);
    }

    [Theory]
    [InlineData("application/json", "", "body")]
    [InlineData("application/json", "{", "body")]
    [InlineData("text/plain", "{\"status\":\"Rented\"}", "body")]
    [InlineData("application/json", "{}", "status")]
    [InlineData("application/json", "{\"status\":\"Rented\",\"title\":\"Changed\"}", "body")]
    [InlineData("application/json", "{\"status\":\"Rented\",\"status\":\"Available\"}", "status")]
    [InlineData("application/json", "{\"status\":42}", "status")]
    public async Task BindAsync_Records_Invalid_Body_Shape(
        string contentType,
        string body,
        string expectedErrorKey)
    {
        var context = new DefaultHttpContext();
        context.Request.RouteValues["id"] = "9c2fcbcb-f94a-4c70-b04f-b24488b89ad1";
        context.Request.ContentType = contentType;
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

        var request = await UpdatePropertyStatusRequest.BindAsync(context, BindingParameter);

        Assert.NotNull(request);
        Assert.Contains(expectedErrorKey, request.BindingErrors.Keys);
    }

    private static void BindEndpoint(UpdatePropertyStatusRequest request)
    {
    }
}
