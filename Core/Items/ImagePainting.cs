using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using ImagePaintings.Core.Tiles;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace ImagePaintings.Core.Items
{
	public class ImagePainting : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Image Painting");
			Tooltip.SetDefault("Creates a image in the form of a painting");
		}

		public override void SetDefaults()
		{
			item.width = 48;
			item.height = 32;
			item.maxStack = 1;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = true;
		}

        public override bool CanUseItem(Player player)
        {
			Point16 mousePos = Main.MouseWorld.ToTileCoordinates16();
			PaintingData data = item.GetGlobalItem<PaintingData>();

			if (data.SavedImage == default || data.ImageURL == string.Empty || data.ImageDimensions == default)
            {
				return false;
            }

			for (int X = mousePos.X; X < mousePos.X + data.ImageDimensions.X; X++)
			{
				for (int Y = mousePos.Y; Y < mousePos.Y + data.ImageDimensions.Y; Y++)
				{
					Tile ExtraCanvas = Framing.GetTileSafely(X, Y);
					if (ExtraCanvas.active() || ExtraCanvas.wall <= 0)
                    {
						return false;
                    }
				}
			}

			return true;
        }

		public static void CreatePainting(Point Position, int whoAmI, int InventorySlot)
		{
			Player player = Main.player[whoAmI];
			Item item = player.inventory[InventorySlot];
			int i = Position.X;
			int j = Position.Y;
			List<Vector2> TEPositions = new List<Vector2>();
			PaintingData data = item.GetGlobalItem<PaintingData>();
			for (int X = i; X < i + data.ImageDimensions.X; X++)
			{
				for (int Y = j; Y < j + data.ImageDimensions.Y; Y++)
				{
					TEPositions.Add(new Vector2(X, Y));
					WorldGen.PlaceTile(X, Y, ModContent.TileType<BlankCanvas>(), true);
					Tile ExtraCanvas = Framing.GetTileSafely(X, Y);
					ExtraCanvas.frameX = (short)((X == i && Y == j) ? 0 : 18);
					ExtraCanvas.frameY = 0;
					NetMessage.SendTileRange(-1, X, Y, 1, 1, TileChangeType.None);
				}
			}

			ModTileEntity thisTE = ModContent.GetInstance<CanvasTE>();
			thisTE.Hook_AfterPlacement(i, j, ModContent.GetInstance<CanvasTE>().type, -1, -1);
			TileEntity te = TileEntity.ByPosition[new Point16(i, j)];
			var PaintingInstance = te as CanvasTE;
			ImagePaintings mod = ModContent.GetInstance<ImagePaintings>();
			if (mod.LoadedImagePaintings.ContainsKey(new Point16(i, j)))
			{
				mod.LoadedImagePaintings[new Point16(i, j)] = data.SavedImage;
			}
			else
            {
				mod.LoadedImagePaintings.Add(new Point16(i, j), data.SavedImage);
            }
			PaintingInstance.ImageURL = data.ImageURL;
			PaintingInstance.ImageDimensions = data.ImageDimensions;
			PaintingInstance.NeedsSyncing = true;
		}

        public override bool UseItem(Player player)
        {
			bool WithinRange()
            {
				if (player.whoAmI == Main.myPlayer)
                {
					Point PlayerPos = player.Center.ToTileCoordinates();
					int RangeX = Player.tileRangeX - 1;
					int RangeY = Player.tileRangeY - 1;
					Point MousePos = Main.MouseWorld.ToTileCoordinates();
					Rectangle AreaOfInteraction = new Rectangle(PlayerPos.X - RangeX, PlayerPos.Y - RangeY, RangeX * 2, RangeY * 2);
					return AreaOfInteraction.Contains(MousePos);
				}
				return false;
            }

			if (CanUseItem(player) && WithinRange())
			{
				int slot = 0;
				for (int Indexer = 0; Indexer < player.inventory.Length; Indexer++)
				{
					Item invItem = player.inventory[Indexer];
					if (item == invItem)
					{
						slot = Indexer;
						break;
					}
				}

				Point MousePosition = Main.MouseWorld.ToTileCoordinates();
				Main.PlaySound(SoundID.Dig, Main.MouseWorld, 1);
				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					ModPacket packet = mod.GetPacket();
					packet.Write((byte)MessageType.CreatePainting);
					packet.WriteVector2(MousePosition.ToVector2());
					packet.Write(player.whoAmI);
					packet.Write(slot);
					packet.Send();
				}
				else
				{
					CreatePainting(MousePosition, player.whoAmI, slot);
				}

				return true;
			}

			return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			PaintingData dataBase = item.GetGlobalItem<PaintingData>();

			if (!string.IsNullOrEmpty(dataBase.ImageURL))
			{
				tooltips.Add(new TooltipLine(mod, "URL", item.GetGlobalItem<PaintingData>().ImageURL ?? "null"));
			}

			Vector2 dims = dataBase.ImageDimensions;
			if (dims != Vector2.Zero && dims != null)
			{
				tooltips.Add(new TooltipLine(mod, "Dimensions", dims.X + "x" + dims.Y));
			}
		}
    }
}