using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ImagePaintings.Content.Items;
using Point = Microsoft.Xna.Framework.Point;

namespace ImagePaintings.Content.Commands
{
	public class CreateImage : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "painting";

		public override string Usage => "/painting <image_url> <X size ( tiles )> <Y size ( tiles )>";

		public override string Description => "Spawns an unique painting that displays an image when placed";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length != 3)
			{
				Main.NewText("You inputted " + args.Length + " arguments while the command expects 3.");
				return;
			}

			if (!int.TryParse(args[1], out int sizeX) || !int.TryParse(args[2], out int sizeY))
			{
				Main.NewText("Seems like input 2 or 3 isn't a valid interger...");
				return;
			}

			if (sizeX <= 0 || sizeX > 50)
			{
				Main.NewText("The X dimension is limited to a value between 0 and 51. You inputted: " + sizeX);
				return;
			}

			if (sizeY <= 0 || sizeY > 50)
			{
				Main.NewText("The Y dimension is limited to a value between 0 and 51. You inputted: " + sizeY);
				return;
			}

			int imageIndex = Item.NewItem(caller.Player.getRect(), ModContent.ItemType<ImagePainting>());
			ImagePainting generatedPainting = Main.item[imageIndex].modItem as ImagePainting;
			generatedPainting.SetData(args[0], new Point(sizeX, sizeY));
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, imageIndex, 1f);
			}
		}
	}
}