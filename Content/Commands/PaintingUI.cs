using Terraria.ModLoader;
using ImagePaintings.Content.UI;

namespace ImagePaintings.Content.Commands
{
	public class PaintingUI : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "paintingUI";

		public override string Usage => "/paintingUI";

		public override string Description => "Brings up the Image Painting Generator UI";

		public override void Action(CommandCaller caller, string input, string[] args) => ModContent.GetInstance<GeneratePaintingState>().UserInterface.SetState(new GeneratePaintingState());
	}
	
}