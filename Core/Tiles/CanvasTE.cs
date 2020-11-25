using ImagePaintings.Core.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ImagePaintings.Core.Tiles
{
	public class CanvasTE : ModTileEntity
	{
		public int Timer = 0;
		//public List<Vector2> Children = new List<Vector2>();
		public string ImageURL;
		public Vector2 ImageDimensions = Vector2.Zero;
		public bool NeedsSyncing;

		public bool CheckForInaccuracies()
		{
			for (int X = Position.X; X < Position.X + ImageDimensions.X; X++)
			{
				for (int Y = Position.Y; Y < Position.Y + ImageDimensions.Y; Y++)
				{
					if (X <= 0 || X >= Main.maxTilesX || Y <= 0 || Y >= Main.maxTilesY)
					{
						return true;
					}

					Tile tile = Framing.GetTileSafely(X, Y);
					if (tile.type != ModContent.TileType<BlankCanvas>())
					{
						return true;
					}

					if (tile.wall <= 0)
					{
						return true;
					}
				}
			}
			return false;
        }

		public void GeneratePaintingLoot()
        {
			int Index = Item.NewItem(Position.ToWorldCoordinates() + new Vector2((ImageDimensions.X * 16) / 2, (ImageDimensions.Y * 16) / 2), mod.ItemType("ImagePainting"), 1, false, -1, true);
			PaintingData item = Main.item[Index].GetGlobalItem<PaintingData>();
			ImagePaintings Mod = ModContent.GetInstance<ImagePaintings>();
			if (Mod.LoadedImagePaintings.ContainsKey(Position))
			{
				if (Mod.LoadedImagePaintings[Position] != default)
				{
					item.SavedImage = ModContent.GetInstance<ImagePaintings>().LoadedImagePaintings[Position];
				}
			}
			ModContent.GetInstance<ImagePaintings>().LoadedImagePaintings[Position] = default;
			item.ImageURL = ImageURL;
			item.ImageDimensions = ImageDimensions;
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Index, 1f);
			}
		}

		public override bool ValidTile(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.active() && tile.type == ModContent.TileType<BlankCanvas>() && tile.frameX == 0 && tile.frameY == 0;
		}

		public override void Update()
		{
			Timer++;

			if (!ByPosition.ContainsKey(Position))
			{
				return;
			}

			if (NeedsSyncing)
			{
				NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
				NeedsSyncing = false;
			}

			if (Timer % 20 == 0 && CheckForInaccuracies())
			{
				NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);

				if (Main.dedServ)
				{
					GeneratePaintingLoot();
					ModPacket packet = mod.GetPacket();
					packet.Write((byte)MessageType.KillPainting);
					packet.WriteVector2(Position.ToVector2());
					packet.Send();
					ByPosition.Remove(Position);
				}
				else
				{
					GeneratePaintingLoot();
					ImagePaintings.DeleteImagePainting(this);
				}
			}
		}

        public override TagCompound Save()
		{
			return new TagCompound
			{
				{"ImageURL", ImageURL},
				{"ImageDimensions", ImageDimensions },
			};
		}

		public override void Load(TagCompound tag)
		{
			string URL = tag.Get<string>("ImageURL");
			ImageURL = URL;

			Vector2 Dims = tag.Get<Vector2>("ImageDimensions");
			ImageDimensions = Dims;
		}

        public override void NetSend(BinaryWriter writer, bool lightSend)
        {
			writer.Write(ImageURL);
			writer.WritePackedVector2(ImageDimensions);
        }

        public override void NetReceive(BinaryReader reader, bool lightReceive)
        {
			ImageURL= reader.ReadString();
			ImageDimensions = reader.ReadPackedVector2();
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, i, j, 1);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, type, 0f, 0, 0, 0);
				return -1;
			}

			return Place(i, j);
		}
    }
}