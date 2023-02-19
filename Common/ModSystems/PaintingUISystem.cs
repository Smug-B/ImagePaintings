using Terraria;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;
using ImagePaintings.Content.UI;

namespace ImagePaintings.Common.ModSystems
{
	public class PaintingUISystem : ModSystem
	{
		public static ModKeybind TogglePaintingUIKey { get; private set; }

		public override void Load() => TogglePaintingUIKey = KeybindLoader.RegisterKeybind(Mod, "Toggle Painting UI", Keys.X);

		public override void Unload() => TogglePaintingUIKey = null;

        public override void PostUpdateInput()
        {
            if (Main.dedServ)
            {
                return;
            }

            if (TogglePaintingUIKey?.JustPressed == true)
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
}
