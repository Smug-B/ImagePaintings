using ImagePaintings.Core.Net;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Terraria;
using Terraria.ModLoader;

namespace ImagePaintings.Core.Graphics
{
	public class GIFHandler : ImageData
	{
		private List<Texture2D> GIFData;

		public int UsageTimer;

		public override void Update()
		{
			base.Update();
			UsageTimer++;
		}

		public GIFHandler(List<Texture2D> gifData) : base(gifData[0])
		{
			GIFData = gifData;
		}

		public override void Unload() => Main.QueueMainThreadAction(() =>
			{
				foreach (Texture2D texture in GIFData)
				{
					texture.Dispose();
				}

				GIFData.Clear();
			});

		public new Texture2D GetTexture(int frameDuration)
		{
			TimeSinceLastUse = 0;
			int frame = UsageTimer / frameDuration;
			return GIFData[frame % GIFData.Count];
		}

		public static ImageData LoadGIF(ImageIndex imageIndex)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
			{
				Main.NewText("Unfortunately, GIFs -- for the moment ( ? ) -- are only supported on Windows...");
				return null;
			}

			if (!NetUtils.Online(includeChatText: true))
			{
				return null;
			}

			if (!imageIndex.URL.EndsWith(".gif"))
			{
				Main.NewText("Attempted to load a GIF that was not expressly identified as one.");
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
						Image newImage = Image.FromStream(memoryStream);
						if (ModContent.GetInstance<ImagePaintingConfigs>().GIFs)
						{
							List<Texture2D> gifData = new List<Texture2D>();
							int frameCount = newImage.GetFrameCount(FrameDimension.Time);
							for (int frameIndexer = 0; frameIndexer < frameCount; frameIndexer++)
							{
								newImage.SelectActiveFrame(FrameDimension.Time, frameIndexer);
								Stream conversionStream = new MemoryStream();
								newImage.Save(conversionStream, ImageFormat.Png);
								gifData.Add(Texture2D.FromStream(Main.instance.GraphicsDevice, conversionStream, imageIndex.ResolutionSizeX, imageIndex.ResolutionSizeX, false));
								conversionStream.Dispose();
							}
							ImagePaintings.AllLoadedImages[imageIndex] = new GIFHandler(gifData);
						}
						else
						{
							Stream conversionStream = new MemoryStream();
							newImage.Save(conversionStream, ImageFormat.Png);
							ImagePaintings.AllLoadedImages[imageIndex] = new ImageData(Texture2D.FromStream(Main.instance.GraphicsDevice, conversionStream, imageIndex.ResolutionSizeX, imageIndex.ResolutionSizeY, false));
							conversionStream.Dispose();
						}

						newImage.Dispose();
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
