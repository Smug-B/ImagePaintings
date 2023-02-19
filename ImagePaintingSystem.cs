using ImagePaintings.Common.ModPlayers;
using ImagePaintings.Content.Items;
using ImagePaintings.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ImagePaintings
{
    public class ImagePaintingSystem : ModSystem
    {
        public override void PostDrawTiles()
		{
			Player player = Main.LocalPlayer;
			Item heldItem = Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
			if (heldItem.type != ModContent.ItemType<NewImagePainting>() && heldItem.type != ModContent.ItemType<ImagePainting>())
			{
				return;
			}

			PaintingBase paintingBase = heldItem.ModItem as PaintingBase;
			Color drawColor = heldItem.ModItem is ImagePainting imagePainting && !imagePainting.CanUseItem(player) ? Color.Red * 0.35f : Color.White * 0.5f;
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
			Vector2 drawPosition = Main.Camera.ScaledPosition - Main.screenPosition + Main.MouseScreen * (1 / Main.GameZoomTarget);
			drawPosition += Main.screenPosition;
			drawPosition -= new Vector2(drawPosition.X % 16, drawPosition.Y % 16);
			drawPosition -= Main.screenPosition;
			Texture2D image = ImagePaintings.FetchImage(paintingBase.PaintingData);
			if (image != null)
			{
				Main.spriteBatch.Draw(image,
				new Rectangle((int)drawPosition.X, (int)drawPosition.Y, paintingBase.PaintingData.SizeX * 16, paintingBase.PaintingData.SizeY * 16),
				null, drawColor, 0f, player.GetModPlayer<OriginPlayer>().PaintingPlaceOrigin.ToWorldCoordinates(0, 0), SpriteEffects.None, 0f);
			}
			Main.spriteBatch.End();
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
