using Microsoft.Xna.Framework.Graphics;

namespace ImagePaintings
{
	public struct ImageData
	{
		public ImageData(Texture2D texture)
		{
			Texture = texture;
			TimeSinceLastUse = 0;
		}

		public Texture2D Texture;

		public int TimeSinceLastUse;

		public Texture2D GetTexture
		{
			get
			{
				TimeSinceLastUse = 0;
				return Texture;
			}
		}
	}
}