using ImagePaintings.Content.Items;
using ImagePaintings.Content.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
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

		public static IPAddress Google = new IPAddress(new byte[] { 8, 8, 8, 8 });

		public static IDictionary<ImageIndex, ImageData> AllLoadedImages = new Dictionary<ImageIndex, ImageData>();

		public ImagePaintings() => Mod = this;

		public static bool PingGoogle()
		{
			Ping ping = new Ping();
			PingReply reply = ping.Send(Google, 1000);
			return reply != null && reply.Status == IPStatus.Success;
		}

		public static async Task<Texture2D> FetchImage(string url, int sizeX, int sizeY)
		{
			ImageIndex indexer = new ImageIndex(url, sizeX, sizeY);
			ImagePaintingConfigs configs = ModContent.GetInstance<ImagePaintingConfigs>();
			if (AllLoadedImages.TryGetValue(indexer, out ImageData imageData))
			{
				return configs.PlaceholderLoadingTexture ? imageData.GetTexture ?? ModContent.GetTexture("ImagePaintings/LoadingPlaceholder") : imageData.GetTexture;
			}

			AllLoadedImages.Add(indexer, new ImageData(null));
			Texture2D image = await Task.Run(() => DirectFetchImage(url, sizeX, sizeY));
			if (image is null)
			{
				return configs.PlaceholderLoadingTexture ? ModContent.GetTexture("ImagePaintings/LoadingPlaceholder") : null;
			}
			else
			{
				AllLoadedImages[indexer] = new ImageData(image);
				return image;
			}
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
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

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

		public override void Load() => IL.Terraria.Main.DrawTiles += PatchDrawPreview;

		private void PatchDrawPreview(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);
			ILLabel skipNormalCall = cursor.DefineLabel();
			ILLabel skipPaintingCall = cursor.DefineLabel();
			if (!cursor.TryGotoNext(i => i.MatchLdsfld<Main>("spriteBatch"), i => i.MatchLdsfld<TileObject>("objectPreview"), i => i.MatchLdsfld<Main>("screenPosition")))
			{
				Logger.Error("Failed to find first DrawPreview patch target.");
				return;
			}
			cursor.EmitDelegate<Func<bool>>(() =>
			{
				Item heldItem = Main.mouseItem.IsAir ? Main.LocalPlayer.HeldItem : Main.mouseItem;
				return heldItem.type == ModContent.ItemType<ImagePainting>();
			});
			cursor.Emit(OpCodes.Brfalse, skipPaintingCall);
			cursor.EmitDelegate<Action>(() =>
			{
				Player player = Main.LocalPlayer;
				Item heldItem = Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
				if (heldItem.type != ModContent.ItemType<ImagePainting>())
				{
					return;
				}

				ImagePainting imagePainting = heldItem.modItem as ImagePainting;
				Color drawColor = imagePainting.CanUseItem(player) ? Color.White * 0.5f : Color.Red * 0.35f;

				Point mouseTilePosition = Main.MouseWorld.ToTileCoordinates();
				Vector2 drawOffset = Main.screenPosition - (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));
				int x = (int)(mouseTilePosition.X * 16f - drawOffset.X);
				int y = (int)(mouseTilePosition.Y * 16f - drawOffset.Y);
				Task<Texture2D> imagePaintingTask = FetchImage(imagePainting.URL, imagePainting.Size.X, imagePainting.Size.Y);
				if (imagePaintingTask.IsCompleted && imagePaintingTask.Result != null)
				{
					Main.spriteBatch.Draw(imagePaintingTask.Result, new Rectangle(x, y, imagePainting.Size.X * 16, imagePainting.Size.Y * 16), drawColor);
				}
			});
			cursor.Emit(OpCodes.Br, skipNormalCall);
			cursor.MarkLabel(skipPaintingCall);

			if (!cursor.TryGotoNext(i => i.MatchLdarg(1)))
			{
				Logger.Error("Failed to find second DrawPreview patch target.");
				return;
			}
			cursor.MarkLabel(skipNormalCall);
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

		public override void PostUpdateEverything()
		{
			foreach (ImageIndex imageIndex in AllLoadedImages.Keys)
			{
				if (AllLoadedImages.TryGetValue(imageIndex, out ImageData imageData))
				{
					if (ModContent.GetInstance<ImagePaintingConfigs>().LowMemoryMode || imageData.Texture == null)
					{
						imageData.TimeSinceLastUse += 1;
					}
				}
			}

			AllLoadedImages = AllLoadedImages.Where(indexDataValue => indexDataValue.Value.TimeSinceLastUse <= 300).ToDictionary(index => index.Key, data => data.Value);
		}
	}
}