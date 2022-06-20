using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using ImagePaintings.Content.Tiles;
using Terraria.ModLoader.IO;
using System.IO;

namespace ImagePaintings.Content.Items
{
	public class ImagePainting : ModItem
	{
		protected override bool CloneNewInstances => true;

		public string URL { get; private set; }

		public Point Size { get; private set; }

		public void SetData(string url, Point size)
		{
			URL = url;
			Size = size;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Image Painting");
			Tooltip.SetDefault("Creates a image in the form of a painting.");
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
			Point mouseTilePosition = Main.MouseWorld.ToTileCoordinates();
			for (int x = mouseTilePosition.X; x < mouseTilePosition.X + Size.X; x++)
			{
				for (int y = mouseTilePosition.Y; y < mouseTilePosition.Y + Size.Y; y++)
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
			tooltips.Add(new TooltipLine(Mod, "URL", "URL: " + (string.IsNullOrEmpty(URL) ? "Hmm... This painting appears to be missing a designated URL." : URL)));
			tooltips.Add(new TooltipLine(Mod, "Size", "Dimensions: " + Size.X + " blocks wide, " + Size.Y + " blocks tall"));

			if (!Main.keyState.IsKeyDown(Keys.LeftShift))
			{
				tooltips.Add(new TooltipLine(Mod, "QOL", "Press Left Shift and preview the image."));
			}
		}

		public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines)
		{
			DrawableTooltipLine lastTooltipLine = lines.LastOrDefault();

			if (lastTooltipLine == null || !Main.keyState.IsKeyDown(Keys.LeftShift))
			{
				return;
			}

			Texture2D image = ImagePaintings.FetchImage(URL, Size.X, Size.Y);
			if (image != null)
			{
				Vector2 drawPosition = new Vector2(lastTooltipLine.X, lastTooltipLine.Y) + new Vector2(0, lastTooltipLine.Font.MeasureString(lastTooltipLine.Text).Y * lastTooltipLine.BaseScale.Y);
				bool widthGreaterThanHeight = image.Width >= image.Height;
				float widthHeightRatio = (float)image.Width / image.Height;
				float heightWidthRatio = (float)image.Height / image.Width;
				int maxDisplaySize = 320;
				int width = widthGreaterThanHeight ? maxDisplaySize : (int)(maxDisplaySize * widthHeightRatio);
				int height = widthGreaterThanHeight ? (int)(maxDisplaySize * heightWidthRatio) : maxDisplaySize;
				Rectangle destinationRectangle = new Rectangle((int)drawPosition.X, (int)drawPosition.Y, width, height);
				Main.spriteBatch.Draw(image, destinationRectangle, Color.White);
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("URL", URL);
			tag.Add("SizeX", Size.X);
			tag.Add("SizeY", Size.Y);
		}

		public override void LoadData(TagCompound tag)
		{
			URL = tag.Get<string>("URL");
			Size = new Point(tag.Get<int>("SizeX"), tag.Get<int>("SizeY"));
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(URL);
			writer.Write(Size.X);
			writer.Write(Size.Y);
		}

		public override void NetReceive(BinaryReader reader)
		{
			URL = reader.ReadString();
			Size = new Point(reader.ReadInt32(), reader.ReadInt32());
		}
	}
}