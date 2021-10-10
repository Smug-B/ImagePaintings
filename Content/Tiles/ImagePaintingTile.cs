using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using ImagePaintings.Content.Items;
using Terraria.ID;
using static ImagePaintings.ImagePaintings;

namespace ImagePaintings.Content.Tiles
{
	public class ImagePaintingTile : ModTile
	{
		public override void SetDefaults()
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
			disableSmartCursor = true;
		}

		public static void PlacePainting(int i, int j, string url, Point size)
		{
			ModTileEntity imagePaintingTileEntity = ModContent.GetInstance<ImagePaintingTileEntity>();
			imagePaintingTileEntity.Hook_AfterPlacement(i, j, imagePaintingTileEntity.type, -1, -1);
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
					newlyPlacedTile.frameX = (short)((x - i) * 16);
					newlyPlacedTile.frameY = (short)((y - j) * 16);
				}
			}

			NetMessage.SendTileRange(-1, i, j, imagePaintingTEInstance.Size.X, imagePaintingTEInstance.Size.Y, TileChangeType.None);
		}

		public override void PlaceInWorld(int i, int j, Item item)
		{
			if (item.type != ModContent.ItemType<ImagePainting>())
			{
				return;
			}

			ImagePainting imagePainting = item.modItem as ImagePainting;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = mod.GetPacket();
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

					if (imagePaintingTile.active() && imagePaintingTile.type == ModContent.TileType<ImagePaintingTile>())
					{
						imagePaintingTile.active(false);
						imagePaintingTile.halfBrick(false);
						imagePaintingTile.frameX = -1;
						imagePaintingTile.frameY = -1;
						imagePaintingTile.color(0);
						imagePaintingTile.frameNumber(0);
						imagePaintingTile.type = 0;
						imagePaintingTile.inActive(false);
					}
				}
			}

			NetMessage.SendTileRange(-1, tileEntityHitbox.X, tileEntityHitbox.Y, tileEntityHitbox.Width, tileEntityHitbox.Height, TileChangeType.None);

			ModContent.GetInstance<ImagePaintingTileEntity>().Kill(tileEntityHitbox.X, tileEntityHitbox.Y);
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			noItem = true;

			if (ImagePaintingTileEntity.FetchTileEntity(new Point(i, j)) is ImagePaintingTileEntity imagePaintingTileEntity)
			{
				int imageIndex = Item.NewItem(new Rectangle((int)imagePaintingTileEntity.WorldPosition.X, (int)imagePaintingTileEntity.WorldPosition.Y, imagePaintingTileEntity.WorldSize.X, imagePaintingTileEntity.WorldSize.Y), ModContent.ItemType<ImagePainting>());
				ImagePainting generatedPainting = Main.item[imageIndex].modItem as ImagePainting;
				generatedPainting.SetData(imagePaintingTileEntity.URL, imagePaintingTileEntity.Size);
				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, imageIndex, 1f);
				}

				KillPainting(imagePaintingTileEntity.Hitbox);

				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					ModPacket packet = mod.GetPacket();
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
			if (!Main.LocalPlayer.HasBuff(ModContent.BuffType<Buffs.ErrorSight>()))
			{
				Tile currentTile = Framing.GetTileSafely(i, j);
				Vector2 drawOffset = Main.screenPosition - (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));

				if (ModContent.GetInstance<ImagePaintingConfigs>().AccurateLighting)
				{
					if (ImagePaintingTileEntity.FetchTileEntity(new Point(i, j)) is ImagePaintingTileEntity imagePaintingTileEntity)
					{
						Task<Texture2D> imagePaintingTask = ImagePaintings.FetchImage(imagePaintingTileEntity.URL, imagePaintingTileEntity.Size.X, imagePaintingTileEntity.Size.Y);
						if (imagePaintingTask.IsCompleted && imagePaintingTask.Result != null)
						{
							int x = (int)(imagePaintingTileEntity.WorldPosition.X - drawOffset.X + currentTile.frameX);
							int y = (int)(imagePaintingTileEntity.WorldPosition.Y - drawOffset.Y + currentTile.frameY);
							spriteBatch.Draw(imagePaintingTask.Result, new Rectangle(x, y, 16, 16), new Rectangle(currentTile.frameX, currentTile.frameY, 16, 16), Lighting.GetColor(i, j));
						}
					}
				}
				else
				{
					if (currentTile.frameX != 0 || currentTile.frameY != 0)
					{
						return Main.LocalPlayer.HasBuff(ModContent.BuffType<Buffs.ErrorSight>());
					}

					if (ImagePaintingTileEntity.FetchTileEntity(new Point(i, j)) is ImagePaintingTileEntity imagePaintingTileEntity)
					{
						Task<Texture2D> imagePaintingTask = ImagePaintings.FetchImage(imagePaintingTileEntity.URL, imagePaintingTileEntity.Size.X, imagePaintingTileEntity.Size.Y);
						if (imagePaintingTask.IsCompleted && imagePaintingTask.Result != null)
						{
							int x = (int)(imagePaintingTileEntity.WorldPosition.X - drawOffset.X);
							int y = (int)(imagePaintingTileEntity.WorldPosition.Y - drawOffset.Y);
							spriteBatch.Draw(imagePaintingTask.Result, new Rectangle(x, y, imagePaintingTileEntity.WorldSize.X, imagePaintingTileEntity.WorldSize.Y), Lighting.GetColor(i + imagePaintingTileEntity.Size.X / 2, j + imagePaintingTileEntity.Size.Y / 2));
						}
					}
				}
			}

			return Main.LocalPlayer.HasBuff(ModContent.BuffType<Buffs.ErrorSight>());
		}
	}
}