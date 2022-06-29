using ImagePaintings.Content.Items;
using ImagePaintings.Content.Tiles;
using ImagePaintings.Core.Graphics;
using ImagePaintings.Core.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
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

		public static Texture2D PlaceholderImage { get; private set; }

		public static IDictionary<ImageIndex, ImageData> AllLoadedImages { get; internal set; } = new Dictionary<ImageIndex, ImageData>();

		public ImagePaintings() => Mod = this;

		public override void Load()
		{
			PlaceholderImage = ModContent.Request<Texture2D>("ImagePaintings/LoadingPlaceholder", AssetRequestMode.ImmediateLoad).Value;
            On.Terraria.TileObject.DrawPreview += DetourDrawPreview;
		}

        public override void Unload()
		{
			PlaceholderImage = null;
			NetUtils.Unload();
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
			Texture2D image = FetchImage(imagePainting.ImageIndex);
			if (image != null)
			{
				Main.spriteBatch.Draw(image, new Rectangle(x, y, imagePainting.ImageIndex.SizeX * 16, imagePainting.ImageIndex.SizeY * 16), drawColor);
			}
		}

		public static Texture2D FetchImage(ImageIndex imageIndex)
		{
			ImagePaintingConfigs configs = ModContent.GetInstance<ImagePaintingConfigs>();
			if (AllLoadedImages.TryGetValue(imageIndex, out ImageData imageData))
			{
				return configs.PlaceholderLoadingTexture ? imageData.GetTexture ?? PlaceholderImage : imageData.GetTexture;
			}

			AllLoadedImages.Add(imageIndex, new ImageData(null));
			Task.Run(() =>
			{
				ImageData imageData = imageIndex.URL.EndsWith(".gif") ? GIFHandler.LoadGIF(imageIndex) : ImageData.LoadTexture(imageIndex);
				if (imageData != null)
				{
					AllLoadedImages[imageIndex] = imageData;
				}
			});
			return configs.PlaceholderLoadingTexture ? PlaceholderImage : null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType type = (MessageType)reader.ReadByte();
			switch (type)
			{
				case MessageType.CreatePainting:
					{
						Point16 position = reader.ReadVector2().ToPoint16();
						ImageIndex imageIndex = new ImageIndex();
						imageIndex.NetReceive(reader);
						ImagePaintingTile.PlacePainting(position.X, position.Y, imageIndex);
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