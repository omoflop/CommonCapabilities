using AssetManagementBase;
using Filesystem;
using Microsoft.Xna.Framework.Graphics;
using Shared;
using SpriteFontPlus;

namespace Fonts;

public static class BitmapFontLoader {
    public static SpriteFont LoadBitmapFont(this NamespacedAssetManager manager, Identifier id, GraphicsDevice device) {
        return manager.UseLoader(id, Loader, new ErrorHelper("Bitmap Font", id.ToString()), device)!;
    }

    public static readonly AssetLoader<SpriteFont> Loader = NamespacedAssetManager.CreateLoader<SpriteFont, GraphicsDevice>((manager, path, id, error, device) => {
        if (!manager.Exists($"{path}.fnt"))
            throw error.Create("File does not exist");

        using Stream stream = manager.Open($"{path}.fnt");
        using StreamReader reader = new(stream);
        return BMFontLoader.Load(reader.ReadToEnd(), (str) => manager.Open(Path.Join(Path.GetFullPath(Path.Join(path, "../")), str)), device);
    });
}