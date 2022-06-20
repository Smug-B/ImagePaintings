using ImagePaintings.Content.Items;
using ImagePaintings.Content.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImagePaintings
{
	public class ImagePaintings : Mod
	{
		public enum MessageType : byte
		{
			CreatePainting,
			KillPainting,
		}

		public static ImagePaintings Mod { get; private set; }

		public static IPAddress Google { get; private set; }

		public static Texture2D PlaceholderImage { get; private set; }

		public static IDictionary<ImageIndex, ImageData> AllLoadedImages { get; internal set; } = new Dictionary<ImageIndex, ImageData>();

		public ImagePaintings() => Mod = this;

		public override void Load()
		{
			Google = new IPAddress(new byte[] { 8, 8, 8, 8 });
			PlaceholderImage = ModContent.Request<Texture2D>("ImagePaintings/LoadingPlaceholder", AssetRequestMode.ImmediateLoad).Value;
            On.Terraria.TileObject.DrawPreview += DetourDrawPreview;
		}

        public override void Unload()
		{
			Google = null;
			PlaceholderImage = null;
		}

		private void DetourDrawPreview(On.Terraria.TileObject.orig_DrawPreview orig, SpriteBatch sb, TileObjectPreviewData op, Vector2 position)
		{
			Player player = Main.LocalPlayer;
			Item heldItem = Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
			if (heldItem.type != ModContent.ItemType<ImagePainting>())
			{
				orig.Invoke(sb, op, position);
				return;
			}

			ImagePainting imagePainting = heldItem.ModItem as ImagePainting;
			Color drawColor = imagePainting.CanUseItem(player) ? Color.White * 0.5f : Color.Red * 0.35f;

			Point mouseTilePosition = Main.MouseWorld.ToTileCoordinates();
			Vector2 drawOffset = Main.screenPosition - (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));
			int x = (int)(mouseTilePosition.X * 16f - drawOffset.X);
			int y = (int)(mouseTilePosition.Y * 16f - drawOffset.Y);
			Texture2D image = FetchImage(imagePainting.URL, imagePainting.Size.X, imagePainting.Size.Y);
			if (image != null)
			{
				Main.spriteBatch.Draw(image, new Rectangle(x, y, imagePainting.Size.X * 16, imagePainting.Size.Y * 16), drawColor);
			}
		}

		public static bool PingGoogle()
		{
			Ping ping = new Ping();
			PingReply reply = ping.Send(Google, 1000);
			return reply != null && reply.Status == IPStatus.Success;
		}

		public static Texture2D FetchImage(string url, int sizeX, int sizeY)
		{
			ImageIndex indexer = new ImageIndex(url, sizeX, sizeY);
			ImagePaintingConfigs configs = ModContent.GetInstance<ImagePaintingConfigs>();
			if (AllLoadedImages.TryGetValue(indexer, out ImageData imageData))
			{
				return configs.PlaceholderLoadingTexture ? imageData.GetTexture ?? PlaceholderImage : imageData.GetTexture;
			}

			AllLoadedImages.Add(indexer, new ImageData(null));
			Main.QueueMainThreadAction(() =>
			{
				Texture2D image = DirectFetchImage(url, sizeX, sizeY);
				if (image != null)
				{
					AllLoadedImages[indexer] = new ImageData(image);
				}
			});
			return null;
		}

		public static Texture2D DirectFetchImage(string url, int sizeX, int sizeY)
		{
			if (!PingGoogle())
			{
				Main.NewText("Client appears to be offline...");
				return null;
			}

			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.SystemDefault;

				using (WebClient getImage = new WebClient())
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						getImage.OpenRead(url).CopyTo(memoryStream);
						memoryStream.Position = 0;

						return Texture2D.FromStream(Main.instance.GraphicsDevice, memoryStream, sizeX * 16, sizeY * 16, false);
					}
				}
			}
			catch (Exception exception)
			{
				Mod.Logger.Error(exception);
				Main.NewText("An error seems to have occured when fetching the image from the given URL.");
				Main.NewText("Please check your logs for more details.");
			}
			return null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType type = (MessageType)reader.ReadByte();
			switch (type)
			{
				case MessageType.CreatePainting:
					{
						Point16 position = reader.ReadVector2().ToPoint16();
						string url = reader.ReadString();
						Vector2 size = reader.ReadVector2();
						ImagePaintingTile.PlacePainting(position.X, position.Y, url, size.ToPoint());
					}
					break;

				case MessageType.KillPainting:
					{
						byte sender = reader.ReadByte();
						Point16 position = reader.ReadVector2().ToPoint16();
						Point16 size = reader.ReadVector2().ToPoint16();
						ImagePaintingTile.KillPainting(new Rectangle(position.X, position.Y, size.X, size.Y));

						if (Main.netMode == NetmodeID.Server)
						{
							ModPacket packet = GetPacket();
							packet.Write((byte)MessageType.KillPainting);
							packet.Write(sender);
							packet.WriteVector2(position.ToVector2());
							packet.WriteVector2(size.ToVector2());
							packet.Send(-1, sender);
						}
					}
					break;

				default:
					Logger.WarnFormat("Image Paintings: Unknown Message type: {0}", type);
					break;
			}
		}
	}
}