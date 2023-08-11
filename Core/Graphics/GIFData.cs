using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace ImagePaintings.Core.Graphics
{
    public class GIFData : ImageData
    {
        private List<GIFFrame> GIFFrames;

        public int CurrentFrame;

        public int UsageTimer;

        public override void Update()
        {
            base.Update();
            UsageTimer++;
        }

        public GIFData(params GIFFrame[] gifFrames) : base(gifFrames[0].Texture) => GIFFrames = gifFrames.ToList();

        public GIFData(List<GIFFrame> gifFrames) : base(gifFrames[0].Texture) => GIFFrames = gifFrames;

        public override void Unload() => Main.QueueMainThreadAction(() =>
        {
            foreach (GIFFrame frame in GIFFrames)
            {
                frame.Texture.Dispose();
            }

            GIFFrames.Clear();
        });

        // Duration are in 10s of milliseconds
        public new Texture2D GetTexture(int frameDuration)
        {
            TimeSinceLastUse = 0;

            float multiplier = frameDuration / 5; // Probably should remove frameDuration in lieu of something else... it's archaic
            if (UsageTimer > (int)(GIFFrames[CurrentFrame].Duration * multiplier))
            {
                UsageTimer = 0;

                CurrentFrame = ++CurrentFrame % GIFFrames.Count;
            }
            return GIFFrames[CurrentFrame].Texture;
        }
    }
}
