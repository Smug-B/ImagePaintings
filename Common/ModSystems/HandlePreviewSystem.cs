using ImagePaintings.Common.ModPlayers;
using ImagePaintings.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ImagePaintings.Common.ModSystems
{
    public class HandlePreviewSystem : ModSystem
    {
        public override void Load()
        {
            //On.Terraria.GameContent.Drawing.TileDrawing.Draw += InsertPreview;
        }

		private Vector2 GetMouseWorldAccountingForZoom()
        {
            Vector2 result = Main.MouseScreen / Main.GameZoomTarget + Main.screenPosition;
            if (Main.LocalPlayer.gravDir == -1f)
            {
                result.Y = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY / Main.GameZoomTarget;
            }

            return result;
        }

        private void InsertPreview(Terraria.GameContent.Drawing.On_TileDrawing.orig_Draw orig, Terraria.GameContent.Drawing.TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride)
        {
            orig.Invoke(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);

			Player player = Main.LocalPlayer;
			Item heldItem = Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
			if (heldItem.type != ModContent.ItemType<NewImagePainting>())
			{
				return;
			}

			PaintingBase imagePainting = heldItem.ModItem as PaintingBase;

			Point placePoint = player.GetModPlayer<OriginPlayer>().AdjustPointForPlaceOrigin(GetMouseWorldAccountingForZoom().ToTileCoordinates());
			Vector2 drawOffset = Main.screenPosition - (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));
			int x = (int)(placePoint.X * 16f - drawOffset.X);
			int y = (int)(placePoint.Y * 16f - drawOffset.Y);
			Texture2D image = ImagePaintings.FetchImage(imagePainting.PaintingData);
			if (image != null)
			{
				Main.spriteBatch.Draw(image, new Rectangle(x, y, (int)(imagePainting.PaintingData.SizeX * 16), (int)(imagePainting.PaintingData.SizeY * 16)), Color.White * 0.5f);
			}
		}
    }
}
