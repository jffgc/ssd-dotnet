using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace RealtorApiTests;

public sealed class PropertiesStaticFilesConfigurationTests
{
    [Fact]
    public void CreatePropertiesAssetsOptions_Maps_Only_Properties_Alias_To_Existing_Image_Directory()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var webRoot = Path.Combine(root, "wwwroot");
        var imageDirectory = Path.Combine(webRoot, "images", "properties");
        Directory.CreateDirectory(imageDirectory);
        var imagePath = Path.Combine(imageDirectory, "1.png");
        File.WriteAllText(imagePath, "image");

        try
        {
            var options = Program.CreatePropertiesAssetsOptions(webRoot);

            Assert.Equal("/assets/properties", options.RequestPath);
            var provider = Assert.IsType<PhysicalFileProvider>(options.FileProvider);
            Assert.Equal(Path.GetFullPath(imagePath), provider.GetFileInfo("1.png").PhysicalPath);
            Assert.False(provider.GetFileInfo("../appsettings.json").Exists);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
