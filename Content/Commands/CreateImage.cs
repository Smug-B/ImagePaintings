using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ImagePaintings.Content.Items;

namespace ImagePaintings.Content.Commands
{
	public class CreateImage : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "painting";

		public override string Usage => "/painting <image url> <X size ( tiles )> <Y size ( tiles )> <OPTIONAL: frame duration> <OPTIONAL: resolution X size> <OPTIONAL: resolution Y size>";

		public override string Description => "Spawns an unique painting that displays an image when placed";

		// Dreadful, clean this up later
		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Main.NewText("This command is deprecated! Use /paintingUI to bring up a much more navigable UI for generating paintings!");

			if (args.Length < 3 || args.Length > 6)
			{
				Main.NewText("You inputted " + args.Length + " arguments while the command expects 3 to 6.");
				return;
			}

			if (!int.TryParse(args[1], out int sizeX) || !int.TryParse(args[2], out int sizeY))
			{
				Main.NewText("Seems like input 2, or 3 isn't a valid interger...");
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


			int frameDuration = 5;
			int resSizeX = -1;
			int resSizeY = -1;
			if (args.Length > 3)
			{
				if (int.TryParse(args[3], out int possibleFrameDuration))
				{
					if (possibleFrameDuration <= 0)
					{
						Main.NewText("Frame duration is limited to a positive interger value. You inputted: " + possibleFrameDuration);
						return;
					}

					frameDuration = possibleFrameDuration;
				}
				else
				{
					Main.NewText("While input 4 ( Frame Duration )is optional, it does expect a valid interger...");
					return;
				}

				if (args.Length > 4)
                {
					if (int.TryParse(args[4], out int possibleResSizeX))
					{
						if (possibleResSizeX <= 0)
						{
							Main.NewText("Resolution ( X ) is limited to a positive interger value. You inputted: " + possibleResSizeX);
							return;
						}

						resSizeX = possibleResSizeX;
					}
					else
					{
						Main.NewText("While input 5 ( Resolution X ) is optional, it does expect a valid interger...");
						return;
					}

					if (args.Length > 5)
					{
						if (int.TryParse(args[5], out int possibleResSizeY))
						{
							if (possibleResSizeY <= 0)
							{
								Main.NewText("Resolution ( Y ) is limited to a positive interger value. You inputted: " + possibleResSizeY);
								return;
							}

							resSizeY = possibleResSizeY;
						}
						else
						{
							Main.NewText("While input 6 ( Resolution Y ) is optional, it does expect a valid interger...");
							return;
						}
					}
				}
			}

			if (resSizeX <= 0)
            {
				resSizeX = sizeX * 16;
            }

			if (resSizeY <= 0)
			{
				resSizeY = sizeY * 16;
			}

			int imageIndex = Item.NewItem(Item.GetSource_None(), caller.Player.getRect(), ModContent.ItemType<ImagePainting>());
			ImagePainting generatedPainting = Main.item[imageIndex].ModItem as ImagePainting;
			generatedPainting.PaintingData = new PaintingData(new ImageIndex(args[0], resSizeX, resSizeY), sizeX, sizeY, frameDuration);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, imageIndex, 1f);
			}
		}
	}
}