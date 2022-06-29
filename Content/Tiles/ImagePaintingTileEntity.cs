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
		public ImageIndex ImageIndex { get; private set; }

		public Vector2 WorldPosition { get; private set; }

		public Point WorldSize { get; private set; }

		public Rectangle Hitbox { get; private set; }

		public void SetData(ImageIndex imageIndex)
		{
			ImageIndex = imageIndex;
			WorldPosition = Position.ToWorldCoordinates(0, 0);
			WorldSize = new Point(ImageIndex.SizeX * 16, ImageIndex.SizeY * 16);
			Hitbox = new Rectangle(Position.X, Position.Y, ImageIndex.SizeX, ImageIndex.SizeY);
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

		public override bool IsTileValidForEntity(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.HasTile && tile.TileType == ModContent.TileType<ImagePaintingTile>();
		}

		public override void SaveData(TagCompound tag) => tag.Add("Index", ImageIndex.Save());

		public override void LoadData(TagCompound tag)
		{
			if (tag.ContainsKey("URL"))
			{
				SetData(new ImageIndex(tag.Get<string>("URL"), tag.Get<int>("SizeX"), tag.Get<int>("SizeY")));
			}
			else
            {
				SetData(ImageIndex.Load(tag.Get<TagCompound>("Index")));
            }
		}

		public override void NetSend(BinaryWriter writer) => ImageIndex.NetSend(writer);

		public override void NetReceive(BinaryReader reader)
		{
			ImageIndex imageIndex = new ImageIndex();
			imageIndex.NetReceive(reader);
			SetData(imageIndex);
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
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