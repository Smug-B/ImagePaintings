using Terraria.ID;
using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ModLoader;
using ImagePaintings.Common.ModPlayers;

namespace ImagePaintings.Content.Items
{
    public class NewImagePainting : PaintingBase
	{
		public override string Texture => "ImagePaintings/Content/Items/ImagePainting";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Image Painting");
			Tooltip.SetDefault("Creates an image in the form of a painting, anywhere."
				+ "\nNOTE: These paintings can only be broken through a 'Painting Hammer'!"
				+ "\nYou can craft one through 5 pieces of wood!");
		}

		public override void SetDefaults()
		{
			Item.width = 48;
			Item.height = 34;

			Item.consumable = true;

			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.autoReuse = true;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server)
			{
				SoundEngine.PlaySound(SoundID.Dig, Main.MouseWorld);
				Point placePoint = player.GetModPlayer<OriginPlayer>().AdjustPointForPlaceOrigin(Main.MouseWorld.ToTileCoordinates());
				ImagePaintingWorldData.WorldPaintingData.Add(new KeyValuePair<Rectangle, PaintingData>(new Rectangle(placePoint.X, placePoint.Y, PaintingData.SizeX, PaintingData.SizeY), PaintingData));

				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					ModPacket packet = Mod.GetPacket();
					packet.Write((byte)ImagePaintings.MessageType.CreatePainting);
					packet.Write((byte)Main.myPlayer);
					packet.WriteVector2(new Vector2(placePoint.X, placePoint.Y));
					PaintingData.NetSend(packet);
					packet.Send();
				}
				return true;
			}
			return null;
		}
	}
}