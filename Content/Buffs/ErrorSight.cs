using Terraria.ModLoader;

namespace ImagePaintings.Content.Buffs
{
    public class ErrorSight : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Error Sight");
            Description.SetDefault("Disables all image painting and shows the normally invisible tiles said paintings sit upon.");
        }
    }
}