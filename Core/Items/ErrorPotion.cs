using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImagePaintings.Core.Items
{
    public class ErrorPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Error Potion");
            Tooltip.SetDefault("Allows the visbility of Canvas tiles for any manual error corrections");
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
            item.buffType = ModContent.BuffType<Buffs.Error>();
            item.buffTime = 3600;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<ImagePainting>(), 1);
            recipe.needWater = true;
            recipe.SetResult(item.type);
            recipe.AddRecipe();
        }
    }
}