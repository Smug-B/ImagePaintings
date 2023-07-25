using Microsoft.Xna.Framework.Graphics;

namespace ImagePaintings.Core.Graphics
{
    public class GIFFrame
    {
        public Texture2D Texture { get; }

        public int Duration { get; }

        public GIFFrame(Texture2D texture, int duration)
        {
            Texture = texture;
            Duration = duration;
        }
    }
}
