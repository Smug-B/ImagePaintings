using ImagePaintings.Core.Items;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImagePaintings.Core.Commands
{
	public class CreateImage : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "painting";

		public override string Usage => "/painting <image_url> <X in tile> <Y in tile>";

		public override string Description => "Spawn a painting using image from URL. Shortener suggested.";

		public void ErrorText() => Main.NewText("None Valid Parameters!", Microsoft.Xna.Framework.Color.Red);

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length != 3)
			{
				ErrorText();
				return;
			}

			bool DimensionsX = int.TryParse(args[1], out int DimsX);
			bool DimensionsY = int.TryParse(args[2], out int DimsY);
			if (!DimensionsX || !DimensionsY)
			{
				ErrorText();
				return;
			}

			Texture2D texture = ImagePaintings.GetTextureFromURL(args[0], Math.Max(DimsX, DimsY));
			int Index = Item.NewItem(caller.Player.getRect(), mod.ItemType("ImagePainting"));
			PaintingData item = Main.item[Index].GetGlobalItem<PaintingData>();
			item.SavedImage = texture;
			item.ImageURL = args[0];
			item.ImageDimensions = new Vector2(DimsX, DimsY);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Index, 1f);
			}
		}
	}
}