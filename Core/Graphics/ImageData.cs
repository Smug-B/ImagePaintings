using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace ImagePaintings.Core.Graphics
{
	public class ImageData
	{
		private Texture2D Texture;

		public int TimeSinceLastUse;

		public virtual Texture2D GetTexture
		{
			get
			{
				TimeSinceLastUse = 0;
				return Texture;
			}
		}

		public ImageData(Texture2D texture)
		{
			Texture = texture;
			TimeSinceLastUse = 0;
		}

		public virtual void Update() => TimeSinceLastUse++;

		public virtual void Unload()
		{
			Main.QueueMainThreadAction(() => Texture?.Dispose());
		}
    }
}