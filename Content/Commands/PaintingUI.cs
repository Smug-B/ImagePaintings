using Terraria.ModLoader;
using ImagePaintings.Content.UI;
using Terraria;

namespace ImagePaintings.Content.Commands
{
	public class PaintingUI : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "paintingUI";

		public override string Usage => "/paintingUI";

		public override string Description => "Toggles the Image Painting Generator UI";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			GeneratePaintingState generatePainting = ModContent.GetInstance<GeneratePaintingState>();
			if (generatePainting.UserInterface.CurrentState is GeneratePaintingState generatePaintingState)
			{
				generatePainting.UserInterface.SetState(null);
			}
			else
			{
				Main.playerInventory = false;
				generatePainting.UserInterface.SetState(new GeneratePaintingState());
			}
		}
	}
}