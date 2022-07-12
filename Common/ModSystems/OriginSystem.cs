using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace ImagePaintings.Common.ModSystems
{
    public class OriginSystem : ModSystem
    {
		public static ModKeybind ConfigPlacePaintingOriginKey { get; private set; }

		public override void Load() => ConfigPlacePaintingOriginKey = KeybindLoader.RegisterKeybind(Mod, "Configure Place Origin", Keys.O);

		public override void Unload() => ConfigPlacePaintingOriginKey = null;
	}
}
