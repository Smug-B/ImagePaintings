using ImagePaintings.Common.ModPlayers;
using ImagePaintings.Content.Items;
using ImagePaintings.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ImagePaintings
{
    public class ImagePaintingSystem : ModSystem
    {
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
			Player player = Main.LocalPlayer;
			Item heldItem = Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
			if (heldItem.type != ModContent.ItemType<NewImagePainting>() && heldItem.type != ModContent.ItemType<ImagePainting>())
            {
				return;

            }

			PaintingBase paintingBase = heldItem.ModItem as PaintingBase;
			Color drawColor = heldItem.ModItem is ImagePainting imagePainting && !imagePainting.CanUseItem(player) ? Color.Red * 0.35f : Color.White * 0.5f;
			Point placePoint = Main.MouseWorld.ToTileCoordinates();
			Vector2 drawOffset = Main.screenPosition;// - (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));
			int x = (int)(placePoint.X * 16f - drawOffset.X);
			int y = (int)(placePoint.Y * 16f - drawOffset.Y);
			Texture2D image = ImagePaintings.FetchImage(paintingBase.PaintingData);
			if (image != null)
			{
				spriteBatch.Draw(image,
				new Rectangle(x, y, (int)(paintingBase.PaintingData.SizeX * 16 * Main.GameZoomTarget), (int)(paintingBase.PaintingData.SizeY * 16 * Main.GameZoomTarget)),
				null, drawColor, 0f, player.GetModPlayer<OriginPlayer>().PaintingPlaceOrigin * 16, SpriteEffects.None, 0f);
			}
		}

        public override void PostUpdateEverything()
		{
			bool usingLowMemory = ModContent.GetInstance<ImagePaintingConfigs>().LowMemoryMode;
			foreach (ImageIndex imageIndex in ImagePaintings.AllLoadedImages.Keys)
			{
				if (ImagePaintings.AllLoadedImages.TryGetValue(imageIndex, out ImageData imageData))
				{
					imageData?.Update();
				}
			}

			/*if (usingLowMemory)
			{
				IList<KeyValuePair<ImageIndex, ImageData>> validPaintings = new List<KeyValuePair<ImageIndex, ImageData>>();
				foreach (KeyValuePair<ImageIndex, ImageData> data in ImagePaintings.AllLoadedImages)
                {
					if (data.Value != null)
                    {
						if (data.Value.TimeSinceLastUse <= 300)
                        {
							validPaintings.Add(data);
                        }
						else
                        {
							data.Value.Unload();
							validPaintings.Add(new KeyValuePair<ImageIndex, ImageData>(data.Key, null));
						}
                    }
                }

				ImagePaintings.AllLoadedImages = validPaintings.ToDictionary(index => index.Key, data => data.Value);
			}*/
		}

        public override void PreSaveAndQuit()
        {
			if (ImagePaintings.AllLoadedImages != null)
			{
				foreach (KeyValuePair<ImageIndex, ImageData> data in ImagePaintings.AllLoadedImages)
				{
					data.Value.Unload();
				}

				ImagePaintings.AllLoadedImages.Clear();
			}
		}
	}
}
