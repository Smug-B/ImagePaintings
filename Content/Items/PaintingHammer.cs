using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;
using Terraria.Audio;

namespace ImagePaintings.Content.Items
{
	public class PaintingHammer : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Painting Hammer");
			Tooltip.SetDefault("Can remove paintings that aren't bound by tiles or walls."
			+ "\nToo delicate to function as an actual hammer...");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 36;
			Item.height = 36;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.autoReuse = true;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server)
			{
				Point mouseTilePosition = Main.MouseWorld.ToTileCoordinates();

				if (ImagePaintingWorldData.TryFindPainting(mouseTilePosition, out KeyValuePair<Rectangle, PaintingData> data, true, true))
				{
					SoundEngine.PlaySound(SoundID.Dig, Main.MouseWorld);
					int imageIndex = Item.NewItem(Item.GetSource_NaturalSpawn(), data.Key.Center.ToWorldCoordinates(), ModContent.ItemType<NewImagePainting>());
					NewImagePainting generatedPainting = Main.item[imageIndex].ModItem as NewImagePainting;
					generatedPainting.PaintingData = data.Value;
					ImagePaintingWorldData.WorldPaintingData.Remove(data);
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, imageIndex, 1f);
						ModPacket packet = Mod.GetPacket();
						packet.Write((byte)ImagePaintings.MessageType.KillPainting);
						packet.Write((byte)player.whoAmI);
						packet.WriteVector2(new Vector2(data.Key.X, data.Key.Y));
						data.Value.NetSend(packet);
						packet.Send();
					}
				}
				return true;
			}
			return null;
		}

		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 5)
			.Register();
    }
}