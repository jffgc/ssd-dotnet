using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Features.Properties.UpdateProperty;

public sealed class UpdatePropertyHandler(AppDbContext context) : IHandler
{
    public async Task<Property?> HandleAsync(Guid id, UpdatePropertyRequest request, CancellationToken cancellationToken = default)
    {
        var property = await context.Properties.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (property is null)
        {
            return null;
        }

        string? newImageUrl = null;
        if (request.Image is not null && request.Image.Length > 0)
        {
            newImageUrl = await SaveImageIfPresentAsync(request.Image, cancellationToken);
        }

        property.ApplyUpdate(request, newImageUrl);
        await context.SaveChangesAsync(cancellationToken);

        return property;
    }

    private static async Task<string?> SaveImageIfPresentAsync(IFormFile image, CancellationToken cancellationToken)
    {
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
