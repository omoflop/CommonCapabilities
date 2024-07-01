using System.Text.Json.Nodes;
using Atlas;
using Filesystem;
using Microsoft.Xna.Framework;
using Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace AtlasExtras;

public static class AtlasExtras {
    public static void Load() {
        TextureAtlasLoader.RegisterProcessor(new AutotileProcessor());
        TextureAtlasLoader.RegisterProcessor(new ScrollingTextureProcessor());
        TextureAtlasLoader.RegisterProcessor(new StripProcessor());
    }
}

public class StripProcessor : TextureMetadataProcessor {

    public override void Process(Identifier id, Image texture, JsonObject metadata, AtlasBuilder builder, ErrorHelper error) {
        JsonNode? n = metadata["strip"];
        if (n is not JsonValue) return;

        string mode = n.GetValue<string>();
        if (mode is not ("vertical" or "horizontal")) throw error.Create($"Unsupported strip mode: {mode} (valid options include: 'horiontal', 'vertical')");
        bool horizontal = mode is "horizontal";
        int size = horizontal ? texture.Width : texture.Height;
        Vector2 axis = horizontal ? Vector2.UnitX : Vector2.UnitY;

        Image[] frames = new Image[size / builder.TextureRes];
        for (int i = 0; i < frames.Length; i++) {
            Vector2 pos = axis * (i * builder.TextureRes);
            Rectangle area = new((int)pos.X, (int)pos.Y, builder.TextureRes, builder.TextureRes);
            frames[i] = texture.Clone(a => a.Crop(area));
        }
        builder.AddAnimation(id, frames);
    }
}

public class AutotileProcessor : TextureMetadataProcessor {
    private static readonly int[] Offsets = [0, 10, 0, 10, 0, 11, 0, 11, 4, 5];
    
    public override void Process(Identifier id, Image texture, JsonObject metadata, AtlasBuilder builder, ErrorHelper error) {
        JsonNode? n = metadata["autotile"];
        if (n is not JsonValue) return;

        string mode = n.GetValue<string>();
        if (mode != "blob") throw error.Create($"Unsupported autotile type: {mode}");

        Image[] frames = new Image[47];
        int i = 0;
        for (int y = 0; y < 5; y++) {
            int xShift = Offsets[y * 2];
            int count = Offsets[y * 2 + 1];
            for (int x = 0; x < count; x++) {
                Rectangle area = new((x + xShift) * builder.TextureRes, y * builder.TextureRes, builder.TextureRes, builder.TextureRes);
                frames[i] = texture.Clone(a => {
                    a.Crop(area);
                });
                i++;
            }
        }
        
        
        builder.AddAnimation(id, frames);
    }
}

public class ScrollingTextureProcessor : TextureMetadataProcessor {
    
    public override void Process(Identifier id, Image tex, JsonObject metadata, AtlasBuilder builder, ErrorHelper error) {
        JsonNode? n = metadata.MustBeObject("autoscroll", error);
        if (n is not JsonObject obj || obj["frames"] is not JsonValue || obj["increment_x"] is not JsonValue || (obj["increment_y"] is not JsonValue)) return;
        
        
        
        int frames = n["frames"]!.GetValue<int>();
        int incrementX = n["increment_x"]!.GetValue<int>();
        int incrementY = n["increment_y"]!.GetValue<int>();

        // Array that will contain each frame of the animation
        Image[] anim = new Image[frames];

        // Prodedurally create a scrolling animation
        for (int i = 0; i < frames; i++) {
            
            Rectangle area = new(incrementX * i, incrementY * i, tex.Width, tex.Height);
            
            anim[i] = tex.Clone(ctx => {
                ctx.Fill(Color.Transparent);
                ctx.DrawImage(tex, area, 1f);
                Point pos2 = new(area.X - Math.Sign(incrementX)*tex.Width, area.Y - Math.Sign(incrementY)*tex.Height);
                ctx.DrawImage(tex, new Rectangle(pos2, area.Size), 1f);
            });

        }
        
        builder.AddAnimation(id, anim);
    }
}