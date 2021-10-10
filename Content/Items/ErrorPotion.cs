using Terraria.ID;
using Terraria.ModLoader;

namespace ImagePaintings.Content.Items
{
    public class ErrorPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Error Potion");
            Tooltip.SetDefault("Disables all image painting and shows the normally invisible tiles said paintings sit upon.");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 30;

            item.useStyle = ItemUseStyleID.EatingUsing;
            item.useAnimation = 15;
            item.useTime = 15;
            item.useTurn = true;
            item.UseSound = SoundID.Item3;

            item.maxStack = 30;
            item.consumable = true;
            item.buffType = ModContent.BuffType<Buffs.ErrorSight>();
            item.buffTime = 18000;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<ImagePainting>());
            recipe.needWater = true;
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}