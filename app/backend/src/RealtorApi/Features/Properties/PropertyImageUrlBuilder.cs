using Microsoft.AspNetCore.Http.Extensions;

namespace RealtorApi.Features.Properties;

internal static class PropertyImageUrlBuilder
{
    internal static string BuildAbsoluteUrl(string imageUrl, string scheme, HostString host)
    {
        var path = Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)
            ? uri.AbsolutePath
            : imageUrl;
        path = path.Split('?', '#')[0].Replace('\\', '/');
        var encodedFileName = path[(path.LastIndexOf('/') + 1)..];
        var fileName = encodedFileName;
        while (true)
        {
            if (fileName.Contains("%2f", StringComparison.OrdinalIgnoreCase)
                || fileName.Contains("%5c", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The persisted property image path does not contain a safe file name.");
            }

            var decodedFileName = Uri.UnescapeDataString(fileName);
            if (decodedFileName == fileName)
            {
                break;
            }

            fileName = decodedFileName;
        }

        if (string.IsNullOrWhiteSpace(fileName)
            || fileName is "." or ".."
            || fileName.IndexOfAny(['/', '\\']) >= 0
            || fileName.Any(char.IsControl))
        {
            throw new InvalidOperationException("The persisted property image path does not contain a safe file name.");
        }

        return UriHelper.BuildAbsolute(
            scheme,
            host,
            path: $"/assets/properties/{fileName}");
    }
}
