using Terraria;
using Terraria.ModLoader;

namespace ImagePaintings.Core.Buffs
{
    public class Error : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Error");
            Description.SetDefault("Allows the viewing of Canvas Tiles for manual error correction.");
        }
    }
}