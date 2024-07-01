using AssetManagementBase;
using Shared;

namespace Filesystem.Loaders;

public static class StringLoader {
    public static string LoadText(this NamespacedAssetManager manager, Identifier id, string fileExtension, bool failSilently = false) {
        return manager.UseLoader(id, fileExtension, Loader, new ErrorHelper("Text File", id.ToString()), new Ctx(fileExtension, failSilently))!;
    }

    public static readonly AssetLoader<string> Loader = NamespacedAssetManager.CreateLoader<string, Ctx>((manager, path, id, error, ctx) => {
        path = Path.ChangeExtension(path, ctx.FileExtension);

        if (manager.Exists(path)) return manager.ReadAsString(path);
        
        if (ctx.FailSilently) return null!;
        throw error.Create("File does not exist");

    });

    private record Ctx(string FileExtension, bool FailSilently);
}