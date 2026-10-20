using Microsoft.AspNetCore.Http;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed class CreatePropertyHandler(AppDbContext context) : IHandler
{
    public async Task<Property> HandleAsync(CreatePropertyRequest request, CancellationToken cancellationToken = default)
    {
        var imageUrl = await SaveImageIfPresentAsync(request.Image, cancellationToken);
        var property = request.ToEntity(imageUrl);

        context.Properties.Add(property);
        await context.SaveChangesAsync(cancellationToken);

        return property;
    }

    private static async Task<string?> SaveImageIfPresentAsync(IFormFile? image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            return null;
        }

        var assetDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "properties");
        Directory.CreateDirectory(assetDirectory);

        var extension = Path.GetExtension(image.FileName);
        var uniqueName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(assetDirectory, uniqueName);

        await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await image.CopyToAsync(stream, cancellationToken);
        }

        return $"/assets/properties/{uniqueName}";
    }
}
