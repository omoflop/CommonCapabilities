using Filesystem;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Atlas;

public readonly record struct ProtoAtlasTexture(Identifier Id, params Image[] Images) {
    private static readonly Image MissingTexture;

    public bool IsAnimated => Images.Length > 1;

    static ProtoAtlasTexture() {
        
        
    }

    public static ProtoAtlasTexture CreateMissingTexture(Identifier.Factory factory, int size) {
        Image image = new Image<Rgba32>(size, size);
        image.Mutate(ctx => {
            ctx.Fill(Color.Black, new RectangleF(0, 0, size, size))
                .Fill(Color.Magenta, new RectangleF(0, 0, size/2f, size/2f))
                .Fill(Color.Magenta, new RectangleF(size/2f, size/2f, size/2f, size/2f));
        });
        
        return new ProtoAtlasTexture(factory.Create("missing"), [image]);
        
    }
}