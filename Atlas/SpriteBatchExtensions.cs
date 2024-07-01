using System.Runtime.CompilerServices;
using Filesystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Atlas;

public static class SpriteBatchExtensions {
    
    public static void Draw(this SpriteBatch batch, TextureAtlas atlas, Identifier textureId, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, int layerDepth = 0, int frame = 0) {
       Draw(batch, atlas, atlas[textureId], position, color, rotation, origin, scale, effects, layerDepth, frame);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Draw(this SpriteBatch batch, TextureAtlas atlas, TextureAtlas.Entry entry, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, int layerDepth = 0, int frame = 0) {
       batch.Draw(atlas.Texture2D, position, entry.Frames[frame], color, rotation, origin, scale, effects, layerDepth);
    }
}