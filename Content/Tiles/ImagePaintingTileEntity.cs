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
		public PaintingData PaintingData { get; private set; }

		public Vector2 WorldPosition { get; private set; }

		public Point WorldSize { get; private set; }

		public Rectangle Hitbox { get; private set; }

		public void SetData(PaintingData paintingData)
		{
			PaintingData = paintingData;
			WorldPosition = Position.ToWorldCoordinates(0, 0);
			WorldSize = new Point(PaintingData.SizeX * 16, PaintingData.SizeY * 16);
			Hitbox = new Rectangle(Position.X, Position.Y, PaintingData.SizeX, PaintingData.SizeY);
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

		public override void SaveData(TagCompound tag) => tag.Add("Data", PaintingData.Save());

		public override void LoadData(TagCompound tag)
		{
			if (tag.ContainsKey("URL"))
			{
				int sizeX = tag.Get<int>("SizeX");
				int sizeY = tag.Get<int>("SizeY");
				SetData(new PaintingData(new ImageIndex(tag.Get<string>("URL"), sizeX * 16, sizeY * 16), sizeX, sizeY));
			}
			else if (tag.ContainsKey("Index"))
			{
				ObsoleteImageIndex obsoleteIndex = ObsoleteImageIndex.Load(tag.Get<TagCompound>("Index"));
				ImageIndex index = new ImageIndex(obsoleteIndex.URL, obsoleteIndex.ResolutionSizeX, obsoleteIndex.ResolutionSizeY);
				SetData(new PaintingData(index, obsoleteIndex.SizeX, obsoleteIndex.SizeY, obsoleteIndex.FrameDuration));
			}
			else
			{
				SetData(PaintingData.Load(tag.Get<TagCompound>("Data")));
			}
		}

		public override void NetSend(BinaryWriter writer) => PaintingData.NetSend(writer);

		public override void NetReceive(BinaryReader reader)
		{
			PaintingData paintingData = new PaintingData();
			paintingData.NetReceive(reader);
			SetData(paintingData);
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