using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using ImagePaintings.Content.Items;
using Terraria.ID;
using static ImagePaintings.ImagePaintings;

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

		public static void PlacePainting(int i, int j, string url, Point size)
		{
			ModTileEntity imagePaintingTileEntity = ModContent.GetInstance<ImagePaintingTileEntity>();
			imagePaintingTileEntity.Hook_AfterPlacement(i, j, imagePaintingTileEntity.type, -1, -1, -1);
			ImagePaintingTileEntity imagePaintingTEInstance = TileEntity.ByPosition[new Point16(i, j)] as ImagePaintingTileEntity;
			imagePaintingTEInstance.SetData(url, size);
			NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, imagePaintingTEInstance.ID, i, j);

			for (int x = i; x < i + imagePaintingTEInstance.Size.X; x++)
			{
				for (int y = j; y < j + imagePaintingTEInstance.Size.Y; y++)
				{
					if (x == i && y == j)
					{
						continue;
					}

					WorldGen.PlaceTile(x, y, ModContent.TileType<ImagePaintingTile>(), true);
					Tile newlyPlacedTile = Framing.GetTileSafely(x, y);
					newlyPlacedTile.TileFrameX = (short)((x - i) * 16);
					newlyPlacedTile.TileFrameY = (short)((y - j) * 16);
				}
			}

			NetMessage.SendTileSquare(-1, i, j, imagePaintingTEInstance.Size.X, imagePaintingTEInstance.Size.Y, TileChangeType.None);
		}

		public override void PlaceInWorld(int i, int j, Item item)
		{
			if (item.type != ModContent.ItemType<ImagePainting>())
			{
				return;
			}

			ImagePainting imagePainting = item.ModItem as ImagePainting;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)MessageType.CreatePainting);
				packet.WriteVector2(new Vector2(i, j));
				packet.Write(imagePainting.URL);
				packet.WriteVector2(imagePainting.Size.ToVector2());
				packet.Send();
			}
			else
			{
				PlacePainting(i, j, imagePainting.URL, imagePainting.Size);
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
				generatedPainting.SetData(imagePaintingTileEntity.URL, imagePaintingTileEntity.Size);
				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, imageIndex, 1f);
				}

				KillPainting(imagePaintingTileEntity.Hitbox);

				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					ModPacket packet = Mod.GetPacket();
					packet.Write((byte)MessageType.KillPainting);
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
					Texture2D image = FetchImage(imagePaintingTileEntity.URL, imagePaintingTileEntity.Size.X, imagePaintingTileEntity.Size.Y);
					if (image != null)
					{
						int x = (int)(i * 16 - drawOffset.X);
						int y = (int)(j * 16 - drawOffset.Y);
						float sourceWidth = image.Width / (float)imagePaintingTileEntity.Size.X;
						float widthScale = sourceWidth / 16f;
						float sourceHeight = image.Height / (float)imagePaintingTileEntity.Size.Y;
						float heightScale = sourceHeight / 16f;
						Rectangle sourceRect = new Rectangle((int)(currentTile.TileFrameX * widthScale), (int)(currentTile.TileFrameY * heightScale), (int)sourceWidth, (int)sourceHeight);
						spriteBatch.Draw(image, new Rectangle(x, y, 16, 16), sourceRect, Lighting.GetColor(i, j));
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
					Texture2D image = FetchImage(imagePaintingTileEntity.URL, imagePaintingTileEntity.Size.X, imagePaintingTileEntity.Size.Y);
					if (image != null)
					{
						int x = (int)(imagePaintingTileEntity.WorldPosition.X - drawOffset.X);
						int y = (int)(imagePaintingTileEntity.WorldPosition.Y - drawOffset.Y);
						spriteBatch.Draw(image, new Rectangle(x, y, imagePaintingTileEntity.WorldSize.X, imagePaintingTileEntity.WorldSize.Y), Lighting.GetColor(i + imagePaintingTileEntity.Size.X / 2, j + imagePaintingTileEntity.Size.Y / 2));
					}
				}
			}

			return false;
		}
	}
}