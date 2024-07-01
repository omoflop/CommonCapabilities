using System.Text.Json.Nodes;
using Filesystem;
using Filesystem.Loaders;
using Microsoft.Xna.Framework.Graphics;
using Shared;
using SixLabors.ImageSharp;

namespace Atlas;

public static class TextureAtlasLoader {
    internal static readonly List<TextureMetadataProcessor> Processors = [];
    
    
    public static TextureAtlas LoadAtlas(this NamespacedAssetManager manager, Identifier.Factory textureIdFactory, int textureResolution, GraphicsDevice device) {
        AtlasBuilder builder = new(textureResolution);
        ErrorHelper error = new("Texture Atlas", "");
        
        builder.Add(ProtoAtlasTexture.CreateMissingTexture(textureIdFactory, textureResolution));

        foreach (Identifier textureId in manager.LoadAssetList(textureIdFactory, "*.png")) {
            Image texture = manager.LoadImage(textureId);
            error.Path = textureId.ToString();

            JsonNode? meta = manager.LoadJson(textureId, true);
            if (meta == null!) continue;
            
            JsonObject obj = meta.MustBeObject("root", error);
            foreach (TextureMetadataProcessor processor in Processors) {
                processor.Process(textureId, texture, obj, builder, error);
            }
        }
        
        return new TextureAtlas(builder.Build(), textureResolution, device);
    }

    public static void RegisterProcessor(TextureMetadataProcessor processor) {
        Processors.Add(processor);
    }
}

public abstract class TextureMetadataProcessor {
    public abstract void Process(Identifier id, Image texture, JsonObject metadata, AtlasBuilder builder, ErrorHelper error);
}

public class AtlasBuilder(int textureRes) {
    public readonly int TextureRes = textureRes;
    private readonly List<ProtoAtlasTexture> _atlasTextures = [];


    public void Add(ProtoAtlasTexture texture) {
        _atlasTextures.Add(texture);
    }

    public void Add(Identifier id, Image image) {
        _atlasTextures.Add(new ProtoAtlasTexture(id, image));
    }

    public void AddAnimation(Identifier id, Image[] frames) {
        _atlasTextures.Add(new ProtoAtlasTexture(id, frames));
    }

    public List<ProtoAtlasTexture> Build() {
        return _atlasTextures;
    }
}