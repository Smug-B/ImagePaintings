using ImagePaintings.Core.Net;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using Terraria;
using Terraria.ModLoader;

namespace ImagePaintings.Core.Graphics
{
    public class GIFData : ImageData
    {
        private List<GIFFrame> GIFFrames;

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

        public new Texture2D GetTexture(int frameDuration)
        {
            TimeSinceLastUse = 0;
            int frame = UsageTimer / frameDuration;
            return GIFFrames[frame % GIFFrames.Count].Texture;
        }
    }
}
