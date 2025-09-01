using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Terraria;

namespace ImagePaintings.Core.Graphics
{
    public class GIFData : ImageData
    {
        private List<GIFFrame> GIFFrames;

        private Dictionary<long, GIFFrame> GIFIndexer;

        public long TotalDuration;

        public long UsageTimer;

        public override void Update()
        {
            base.Update();
            UsageTimer++;
        }

        public GIFData(params GIFFrame[] gifFrames) : base(gifFrames[0].Texture)
        {
            GIFFrames = gifFrames.ToList();
            GIFIndexer = new Dictionary<long, GIFFrame>();
            foreach (GIFFrame frame in GIFFrames)
            {
                GIFIndexer.Add(TotalDuration, frame);
                TotalDuration += frame.Duration;
            }
        }

        public GIFData(List<GIFFrame> gifFrames) : base(gifFrames[0].Texture)
        {
            GIFFrames = gifFrames;
            GIFIndexer = new Dictionary<long, GIFFrame>();
            foreach (GIFFrame frame in GIFFrames)
            {
                GIFIndexer.Add(TotalDuration, frame);
                TotalDuration += frame.Duration;
            }
        }

        public GIFFrame Index(long duration)
        {
            long encounteredDuration = 0;
            long wrappedDuration = duration % TotalDuration;
            GIFFrame output = GIFIndexer[0];
            foreach (KeyValuePair<long, GIFFrame> data in GIFIndexer)
            {
                GIFFrame gifFrame = data.Value;
                long gifDuration = gifFrame.Duration;
                encounteredDuration += gifDuration;
                if (encounteredDuration > wrappedDuration)
                {
                    return output;
                }
                output = gifFrame;
            }
            return output;
        }

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
            float multiplier = frameDuration / 5f;
            if (multiplier < 1)
            {
                multiplier = 1;
            }
            return Index((long)(UsageTimer / multiplier)).Texture;
        }
    }
}
