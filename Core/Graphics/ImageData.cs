using ImagePaintings.Core.Net;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Net;
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

		public static ImageData LoadTexture(ImageIndex imageIndex)
		{
			if (!NetUtils.Online(includeChatText: true))
			{
				return null;
			}

			if (!imageIndex.URL.EndsWith(".png") && !imageIndex.URL.EndsWith(".jpeg") && !imageIndex.URL.EndsWith(".jpg"))
			{
				Main.NewText("Unfortunately, image paintings currently only supports the common image formats of PNGs, JPGs, JPEGs, and GIFs");
				return null;
			}

			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.SystemDefault;

				using (WebClient getImage = new WebClient())
				{
					MemoryStream memoryStream = new MemoryStream();
					getImage.OpenRead(imageIndex.URL).CopyTo(memoryStream);
					memoryStream.Position = 0;
					Main.QueueMainThreadAction(() =>
					{
						ImagePaintings.AllLoadedImages[imageIndex] = new ImageData(Texture2D.FromStream(Main.instance.GraphicsDevice, memoryStream, imageIndex.ResolutionSizeX, imageIndex.ResolutionSizeY, false));
						memoryStream.Dispose();
					});
				}
			}
			catch (Exception exception)
			{
				ImagePaintings.Mod.Logger.Error(exception);
				Main.NewText("An error seems to have occured when fetching the image from the given URL.");
				Main.NewText("Please check your logs for more details.");
			}
			return null;
		}
    }
}