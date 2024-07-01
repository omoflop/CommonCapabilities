using AssetManagementBase;
using Filesystem;
using Shared;
using SixLabors.ImageSharp;

namespace Atlas;

internal static class ImageLoader {
    public static Image LoadImage(this NamespacedAssetManager manager, Identifier id) {
        return manager.UseLoader(id, "png", Loader, new ErrorHelper("Image", id.ToString()))!;
    }

    public static readonly AssetLoader<Image> Loader = NamespacedAssetManager.CreateLoader<Image>((manager, path, id, error) => {
        if (!manager.Exists(path))
            throw error.Create("File does not exist");
        
        Stream stream = manager.Open(path);
        return Image.Load(stream);
    });
}