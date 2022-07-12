using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader.IO;
using ImagePaintings.Content.Tiles;
using ImagePaintings.Common.ModPlayers;

namespace ImagePaintings.Content.Items
{
    public class ImagePainting : PaintingBase
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Image Painting [ Legacy ]");
			Tooltip.SetDefault("Creates an image in the form of a painting on valid walled surfaces.");
		}

		public override void SetDefaults()
		{
			Item.width = 48;
			Item.height = 34;

			Item.consumable = true;

			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.autoReuse = true;

			Item.createTile = ModContent.TileType<ImagePaintingTile>();
		}

		public override bool CanUseItem(Player player)
		{
			Point placePoint = player.GetModPlayer<OriginPlayer>().AdjustPointForPlaceOrigin(Main.MouseWorld.ToTileCoordinates());
			for (int x = placePoint.X; x < placePoint.X + PaintingData.SizeX; x++)
			{
				for (int y = placePoint.Y; y < placePoint.Y + PaintingData.SizeY; y++)
				{
					if (!WorldGen.InWorld(x, y))
					{
						return false;
					}

					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile || tile.WallType <= 0)
					{
						return false;
					}
				}
			}
			return true;
		}

        public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "URL", "URL: " + (string.IsNullOrEmpty(PaintingData.ImageIndex.URL) ? "Hmm... This painting appears to be missing a designated URL." : PaintingData.ImageIndex.URL)));
			tooltips.Add(new TooltipLine(Mod, "Size", "Dimensions: " + PaintingData.SizeX + " blocks wide, " + PaintingData.SizeY + " blocks tall"
			+ "\nResolution Dimensions: " + PaintingData.ImageIndex.ResolutionSizeX + ", " + PaintingData.ImageIndex.ResolutionSizeY));
			tooltips.Add(new TooltipLine(Mod, "Frame Duration", "Frame Duration: " + PaintingData.FrameDuration + " ticks, this is only relevant for GIFs"));
			tooltips.Add(new TooltipLine(Mod, "Brightness", "Brightness: " + PaintingData.Brightness + " ticks, this is only relevant if the value is not -1"));

			if (!Main.keyState.IsKeyDown(Keys.LeftShift))
			{
				tooltips.Add(new TooltipLine(Mod, "QOL", "Press Left Shift and preview the image."));
			}
		}

		public override void LoadData(TagCompound tag)
		{
			if (tag.ContainsKey("URL"))
			{
				int sizeX = tag.Get<int>("SizeX");
				int sizeY = tag.Get<int>("SizeY");
				PaintingData = new PaintingData(new ImageIndex(tag.Get<string>("URL"), sizeX * 16, sizeY * 16), sizeX, sizeY);
			}
			else if (tag.ContainsKey("Index"))
			{
				ObsoleteImageIndex obsoleteIndex = ObsoleteImageIndex.Load(tag.Get<TagCompound>("Index"));
				ImageIndex index = new ImageIndex(obsoleteIndex.URL, obsoleteIndex.ResolutionSizeX, obsoleteIndex.ResolutionSizeY);
				PaintingData = new PaintingData(index, obsoleteIndex.SizeX, obsoleteIndex.SizeY, obsoleteIndex.FrameDuration);
			}
			else
			{
				PaintingData = PaintingData.Load(tag.Get<TagCompound>("Data"));
			}
		}
	}
}