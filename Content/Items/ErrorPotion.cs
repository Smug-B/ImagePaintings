using Terraria;
using Terraria.GameContent.Creative;
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
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 30;

            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;

            Item.maxStack = 30;
            Item.consumable = true;
            Item.buffType = ModContent.BuffType<Buffs.ErrorSight>();
            Item.buffTime = 18000;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<ImagePainting>());
            recipe.HasCondition(Recipe.Condition.NearWater);
            recipe.Register();
        }
    }
}