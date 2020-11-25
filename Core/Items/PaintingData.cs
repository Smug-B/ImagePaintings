using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ImagePaintings.Core.Items
{
	public class PaintingData : GlobalItem
	{
		public override bool CloneNewInstances => true;

		public override bool InstancePerEntity => true;

		public Texture2D SavedImage;

		public string ImageURL = string.Empty;

		public Vector2 ImageDimensions = Vector2.Zero;

		public void LoadImage()
        {
			if (SavedImage == default && !string.IsNullOrEmpty(ImageURL) && ImageDimensions.X > 0 && ImageDimensions.Y > 0)
			{
				SavedImage = ImagePaintings.GetTextureFromURL(ImageURL, (int)Math.Max(ImageDimensions.X, ImageDimensions.Y));
			}
		}

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
			LoadImage();
			return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        public override bool NeedsSaving(Item item)
        {
			if (item.type == mod.ItemType("ImagePainting"))
            {
				return true;
            }
            return base.NeedsSaving(item);
        }

        public override TagCompound Save(Item item)
		{
			return new TagCompound
			{
				{"ImageURL", ImageURL},
				{"ImageDimensions", ImageDimensions },
			};
		}

        public override void Load(Item item, TagCompound tag)
        {
			string URL = tag.Get<string>("ImageURL");
			ImageURL = URL;

			Vector2 Dims = tag.Get<Vector2>("ImageDimensions");
			ImageDimensions = Dims;
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			writer.Write(ImageURL);
			writer.WriteVector2(ImageDimensions);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			ImageURL = reader.ReadString();
			ImageDimensions = reader.ReadVector2();
		}
	}
}