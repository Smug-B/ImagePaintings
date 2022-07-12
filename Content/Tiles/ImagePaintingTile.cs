using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using ImagePaintings.Content.Items;
using Terraria.ID;
using static ImagePaintings.ImagePaintings;
using Terraria.Chat;
using Terraria.Localization;
using ImagePaintings.Common.ModPlayers;

namespace ImagePaintings.Content.Tiles
{
	public class ImagePaintingTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ImagePaintingTileEntity>().Hook_AfterPlacement, -1, 0, true);
			TileObjectData.newTile.AnchorWall = true;
			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.CoordinateHeights = new[] { 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 0;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Image Painting");
			AddMapEntry(new Color(200, 170, 130), name);
		}

		public static void PlacePainting(int i, int j, PaintingData paintingData)
		{
			ModTileEntity imagePaintingTileEntity = ModContent.GetInstance<ImagePaintingTileEntity>();
			imagePaintingTileEntity.Hook_AfterPlacement(i, j, imagePaintingTileEntity.type, -1, -1, -1);
			ImagePaintingTileEntity imagePaintingTEInstance = TileEntity.ByPosition[new Point16(i, j)] as ImagePaintingTileEntity;
			imagePaintingTEInstance.SetData(paintingData);
			NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, imagePaintingTEInstance.ID, i, j);

			for (int x = 0; x < imagePaintingTEInstance.PaintingData.SizeX; x++)
			{
				for (int y = 0; y < imagePaintingTEInstance.PaintingData.SizeY; y++)
				{
					WorldGen.PlaceTile(i + x, j + y, ModContent.TileType<ImagePaintingTile>(), true);
					Tile newlyPlacedTile = Framing.GetTileSafely(i + x, j + y);
					newlyPlacedTile.TileFrameX = (short)(x * 16);
					newlyPlacedTile.TileFrameY = (short)(y * 16);
				}
			}

			NetMessage.SendTileSquare(-1, i, j, imagePaintingTEInstance.PaintingData.SizeX, imagePaintingTEInstance.PaintingData.SizeY, TileChangeType.None);
		}

		public override void PlaceInWorld(int i, int j, Item item)
		{
			if (item.type != ModContent.ItemType<ImagePainting>())
			{
				return;
			}

			ImagePainting imagePainting = item.ModItem as ImagePainting;

			Point placePoint = Main.LocalPlayer.GetModPlayer<OriginPlayer>().AdjustPointForPlaceOrigin(new Point(i, j));
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)MessageType.CreateLegacyPainting);
				packet.WriteVector2(placePoint.ToVector2());
				imagePainting.PaintingData.NetSend(packet);
				packet.Send();
			}
			else
			{
				PlacePainting(placePoint.X, placePoint.Y, imagePainting.PaintingData);
			}
		}

		public static void KillPainting(Rectangle tileEntityHitbox)
		{
			for (int x = tileEntityHitbox.Left; x < tileEntityHitbox.Right; x++)
			{
				for (int y = tileEntityHitbox.Top; y < tileEntityHitbox.Bottom; y++)
				{
					if (!WorldGen.InWorld(x, y))
					{
						continue;
					}

					Tile imagePaintingTile = Framing.GetTileSafely(x, y);

					if (imagePaintingTile.HasTile && imagePaintingTile.TileType == ModContent.TileType<ImagePaintingTile>())
					{
						imagePaintingTile.HasTile = false;
						imagePaintingTile.IsHalfBlock = false;
						imagePaintingTile.TileFrameX = -1;
						imagePaintingTile.TileFrameY = -1;
						imagePaintingTile.TileColor = 0;
						imagePaintingTile.TileFrameNumber = 0;
						imagePaintingTile.TileType = 0;
						imagePaintingTile.IsActuated = false;
					}
				}
			}

			NetMessage.SendTileSquare(-1, tileEntityHitbox.X, tileEntityHitbox.Y, tileEntityHitbox.Width, tileEntityHitbox.Height, TileChangeType.None);

			ModContent.GetInstance<ImagePaintingTileEntity>().Kill(tileEntityHitbox.X, tileEntityHitbox.Y);
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			noItem = true;

			if (ImagePaintingTileEntity.FetchTileEntity(new Point(i, j)) is ImagePaintingTileEntity imagePaintingTileEntity)
			{
				int imageIndex = Item.NewItem(Item.GetSource_NaturalSpawn(), new Rectangle((int)imagePaintingTileEntity.WorldPosition.X, (int)imagePaintingTileEntity.WorldPosition.Y, imagePaintingTileEntity.WorldSize.X, imagePaintingTileEntity.WorldSize.Y), ModContent.ItemType<ImagePainting>());
				ImagePainting generatedPainting = Main.item[imageIndex].ModItem as ImagePainting;
				generatedPainting.PaintingData = imagePaintingTileEntity.PaintingData;
				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, imageIndex, 1f);
				}

				KillPainting(imagePaintingTileEntity.Hitbox);

				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					ModPacket packet = Mod.GetPacket();
					packet.Write((byte)MessageType.KillLegacyPainting);
					packet.Write((byte)Main.myPlayer);
					packet.WriteVector2(imagePaintingTileEntity.Position.ToVector2());
					packet.WriteVector2(imagePaintingTileEntity.Hitbox.Size());
					packet.Send();
				}
			}
		}

		public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

		public override bool CreateDust(int i, int j, ref int type) => false;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			bool hasErrorSight = Main.LocalPlayer.HasBuff(ModContent.BuffType<Buffs.ErrorSight>());
			if (hasErrorSight)
			{
				return true;
			}

			Tile currentTile = Framing.GetTileSafely(i, j);
			Vector2 drawOffset = Main.screenPosition - (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));

			if (ModContent.GetInstance<ImagePaintingConfigs>().AccurateLighting)
			{
				if (ImagePaintingTileEntity.FetchTileEntity(new Point(i, j)) is ImagePaintingTileEntity imagePaintingTileEntity)
				{
					Texture2D image = FetchImage(imagePaintingTileEntity.PaintingData);
					if (image != null)
					{
						int x = (int)(i * 16 - drawOffset.X);
						int y = (int)(j * 16 - drawOffset.Y);
						float sourceWidth = image.Width / (float)imagePaintingTileEntity.PaintingData.SizeX;
						float widthScale = sourceWidth / 16f;
						float sourceHeight = image.Height / (float)imagePaintingTileEntity.PaintingData.SizeY;
						float heightScale = sourceHeight / 16f;
						Rectangle sourceRect = new Rectangle((int)(currentTile.TileFrameX * widthScale), (int)(currentTile.TileFrameY * heightScale), (int)sourceWidth, (int)sourceHeight);
						Color drawColor = imagePaintingTileEntity.PaintingData.Brightness > 0 ? new Color(new Vector3(imagePaintingTileEntity.PaintingData.Brightness)) : Lighting.GetColor(i, j);
						spriteBatch.Draw(image, new Rectangle(x, y, 16, 16), sourceRect, drawColor);
					}
				}
			}
			else
			{
				if (currentTile.TileFrameX != 0 || currentTile.TileFrameY != 0)
				{
					return Main.LocalPlayer.HasBuff(ModContent.BuffType<Buffs.ErrorSight>());
				}

				if (ImagePaintingTileEntity.FetchTileEntity(new Point(i, j)) is ImagePaintingTileEntity imagePaintingTileEntity)
				{
					Texture2D image = FetchImage(imagePaintingTileEntity.PaintingData);
					if (image != null)
					{
						int x = (int)(imagePaintingTileEntity.WorldPosition.X - drawOffset.X);
						int y = (int)(imagePaintingTileEntity.WorldPosition.Y - drawOffset.Y);
						Color drawColor = imagePaintingTileEntity.PaintingData.Brightness > 0 ? new Color(new Vector3(imagePaintingTileEntity.PaintingData.Brightness)) : Lighting.GetColor(i + imagePaintingTileEntity.PaintingData.SizeX / 2, j + imagePaintingTileEntity.PaintingData.SizeY / 2);
						spriteBatch.Draw(image, new Rectangle(x, y, imagePaintingTileEntity.WorldSize.X, imagePaintingTileEntity.WorldSize.Y), drawColor);
					}
				}
			}

			return false;
		}
	}
}