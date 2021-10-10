using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ImagePaintings.Content.Tiles
{
	public class ImagePaintingTileEntity : ModTileEntity
	{
		public string URL { get; private set; }

		public Point Size { get; private set; }

		public Vector2 WorldPosition { get; private set; }

		public Point WorldSize { get; private set; }

		public Rectangle Hitbox { get; private set; }

		public void SetData(string url, Point size)
		{
			URL = url;
			Size = size;
			WorldPosition = Position.ToWorldCoordinates(0, 0);
			WorldSize = new Point(Size.X * 16, Size.Y * 16);
			Hitbox = new Rectangle(Position.X, Position.Y, Size.X, Size.Y);
		}

		public static ImagePaintingTileEntity FetchTileEntity(Point position)
		{
			foreach (TileEntity tileEntity in ByID.Values)
			{
				if (tileEntity is ImagePaintingTileEntity imagePaintingTileEntity)
				{
					if (imagePaintingTileEntity.Hitbox.Contains(position))
					{
						return imagePaintingTileEntity;
					}
				}
			}

			return null;
		}

		public override bool ValidTile(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.active() && tile.type == ModContent.TileType<ImagePaintingTile>();
		}

		public override TagCompound Save()
		{
			return new TagCompound
			{
				{ "URL", URL },
				{ "SizeX", Size.X },
				{ "SizeY", Size.Y }
			};
		}

		public override void Load(TagCompound tag) => SetData(tag.Get<string>("URL"), new Point(tag.Get<int>("SizeX"), tag.Get<int>("SizeY")));

		public override void NetSend(BinaryWriter writer, bool lightSend)
		{
			writer.Write(URL);
			writer.Write(Size.X);
			writer.Write(Size.Y);
		}

		public override void NetReceive(BinaryReader reader, bool lightReceive) => SetData(reader.ReadString(), new Point(reader.ReadInt32(), reader.ReadInt32()));

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