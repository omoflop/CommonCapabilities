using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Filesystem;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Atlas;

public class TextureAtlas {
    public readonly int TextureRes;

    private readonly ConcurrentDictionary<Identifier, Entry> _textureUvLookup = [];
    public readonly Entry MissingEntry;
    public readonly Texture2D Texture2D;
    
    public Entry this[Identifier id] => _textureUvLookup.GetValueOrDefault(id, MissingEntry);
    public readonly int Size;
    public readonly float Unit;

    public readonly struct Entry(Rectangle[] frames) {
        public readonly Rectangle[] Frames = frames;
    }

    public TextureAtlas(List<ProtoAtlasTexture> atlasTextures, int res, GraphicsDevice device) {
        int textureCount = atlasTextures.Sum(t => t.Images.Length);
        
        TextureRes = res;

        Console.WriteLine($"Stitching atlas for {textureCount} textures...");
        
        // Calculate the number of columns and rows based on texture count
        int numCols = (int)Math.Ceiling(Math.Sqrt(textureCount));
        int numRows = (int)Math.Ceiling((double)textureCount / numCols);

        // Calculate atlas size
        int atlasSize = 1;
        while (atlasSize < Math.Max(numCols, numRows)) {
            atlasSize *= 2;
        }

        Size = atlasSize * TextureRes;
        Unit = 1f / Size * TextureRes;
        Console.WriteLine($"Creating atlas for size {Size}x{Size}");

        // Create the image that the atlas will be drawn onto
        Image<Rgba32> texture = new(Size, Size);

        int i = 0;
        foreach ((Identifier textureId, Image[] images) in atlasTextures) {

            List<Rectangle> frames = [];
            foreach (Image image in images) {
                int x = (i % atlasSize) * TextureRes;
                int y = (i / atlasSize) * TextureRes;

                // Save the position where the texture is drawn into a dictionary to be referenced later
                frames.Add(new Rectangle(x, y, TextureRes, TextureRes));

                // Draw the texture onto the atlas at the given position
                texture.Mutate(ctx => ctx.DrawImage(image, new SixLabors.ImageSharp.Point(x, y), 1));
                i++;
            }

            // TODO: Implement variable texture resolution 
            _textureUvLookup[textureId] = new Entry(frames.ToArray());
        }

        MissingEntry = _textureUvLookup[atlasTextures[0].Id];

        
        #if DEBUG
        string path = atlasTextures[0].Id.Path;
        texture.SaveAsPng($"ATLAS_DEBUG_{path[..(path.LastIndexOf('/'))].Replace('/', '.')}.png");
        #endif

        Texture2D = new Texture2D(device, texture.Width, texture.Width);

        byte[] rawData = new byte[texture.Width * texture.Height * Unsafe.SizeOf<Rgba32>()];
        texture.CopyPixelDataTo(rawData);
        Texture2D.SetData(rawData);
    }
}