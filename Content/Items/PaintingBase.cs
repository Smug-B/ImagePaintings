using Terraria.ModLoader;
using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader.IO;
using System.IO;

namespace ImagePaintings.Content.Items
{
	public abstract class PaintingBase : ModItem
	{
		public PaintingData PaintingData;

		protected override bool CloneNewInstances => true;

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "URL", "URL: " + (string.IsNullOrEmpty(PaintingData.ImageIndex.URL) ? "Hmm... This painting appears to be missing a designated URL." : PaintingData.ImageIndex.URL)));
			tooltips.Add(new TooltipLine(Mod, "Size", "Dimensions: " + PaintingData.SizeX + " blocks wide, " + PaintingData.SizeY + " blocks tall"
			+ "\nResolution Dimensions: " + PaintingData.ImageIndex.ResolutionSizeX + ", " + PaintingData.ImageIndex.ResolutionSizeY));
			tooltips.Add(new TooltipLine(Mod, "Frame Duration", "Frame Duration: " + PaintingData.FrameDuration + " ticks, this is only relevant for GIFs"));
			tooltips.Add(new TooltipLine(Mod, "Brightness", "Brightness: " + PaintingData.Brightness + " ticks, this is only relevant if the value greater than 0"));

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

			Texture2D image = ImagePaintings.FetchImage(PaintingData);
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
				Color drawColor = PaintingData.Brightness > 0 ? new Color(new Vector3(PaintingData.Brightness)) : Color.White;
				Main.spriteBatch.Draw(image, destinationRectangle, drawColor);
			}
		}

		public override void SaveData(TagCompound tag) => tag.Add("Data", PaintingData.Save());

		public override void LoadData(TagCompound tag) => PaintingData = PaintingData.Load(tag.Get<TagCompound>("Data"));

		public override void NetSend(BinaryWriter writer) => PaintingData.NetSend(writer);

		public override void NetReceive(BinaryReader reader) => PaintingData.NetReceive(reader);
	}
}