using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using ImagePaintings.Content.Tiles;
using Terraria.ModLoader.IO;
using System.IO;

namespace ImagePaintings.Content.Items
{
	public class ImagePainting : ModItem
	{
		public override bool CloneNewInstances => true;

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
			item.width = 48;
			item.height = 34;

			item.consumable = true;

			item.useAnimation = 15;
			item.useTime = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTurn = true;
			item.autoReuse = true;

			item.createTile = ModContent.TileType<ImagePaintingTile>();
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
					if (tile.active() || tile.wall <= 0)
					{
						return false;
					}
				}
			}
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(mod, "URL", "URL: " + (string.IsNullOrEmpty(URL) ? "Hmm... This painting appears to be missing a designated URL." : URL)));
			tooltips.Add(new TooltipLine(mod, "Size", "Dimensions: " + Size.X + " blocks wide, " + Size.Y + " blocks tall"));

			if (!Main.keyState.IsKeyDown(Keys.LeftShift))
			{
				tooltips.Add(new TooltipLine(mod, "QOL", "Press Left Shift and preview the image."));
			}
		}

		public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines)
		{
			DrawableTooltipLine lastTooltipLine = lines.LastOrDefault();

			if (lastTooltipLine == null || !Main.keyState.IsKeyDown(Keys.LeftShift))
			{
				return;
			}

			Task<Texture2D> imagePaintingTask = ImagePaintings.FetchImage(URL, Size.X, Size.Y);
			if (imagePaintingTask.IsCompleted && imagePaintingTask.Result != null)
			{
				Vector2 drawPosition = new Vector2(lastTooltipLine.X, lastTooltipLine.Y) + new Vector2(0, lastTooltipLine.font.MeasureString(lastTooltipLine.text).Y * lastTooltipLine.baseScale.Y);
				Rectangle destinationRectangle = new Rectangle((int)drawPosition.X, (int)drawPosition.Y, 320, 320);
				Main.spriteBatch.Draw(imagePaintingTask.Result, destinationRectangle, Color.White);
			}
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

		public override void Load(TagCompound tag)
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

		public override void NetRecieve(BinaryReader reader)
		{
			URL = reader.ReadString();
			Size = new Point(reader.ReadInt32(), reader.ReadInt32());
		}
	}
}