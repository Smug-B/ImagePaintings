using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria.Enums;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using ImagePaintings.Core.Items;
using System.Collections.Generic;
using System;
using Terraria.Localization;

namespace ImagePaintings.Core.Tiles
{
	public class BlankCanvas : ModTile
	{
		public override void SetDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.newTile.AnchorWall = true;
			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.CoordinateHeights = new[] { 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Painting");
			AddMapEntry(new Color(255, 255, 255), name);
			disableSmartCursor = true;
		}

        public override bool CreateDust(int i, int j, ref int type)
		{
			return false;
		}

		public ImagePaintings Mod => ModContent.GetInstance<ImagePaintings>();

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Point16 Position = new Point16(i, j);
			if (Framing.GetTileSafely(i, j).frameX == 0 && Framing.GetTileSafely(i, j).frameY == 0 && !Main.LocalPlayer.HasBuff(mod.BuffType("Error")))
			{
				if (TileEntity.ByPosition.ContainsKey(Position))
				{
					CanvasTE canvas = TileEntity.ByPosition[Position] as CanvasTE;

					void UpdateData()
					{
						if (Mod.LoadedImagePaintings.ContainsKey(Position))
						{
							Mod.LoadedImagePaintings[Position] = ImagePaintings.GetTextureFromURL(canvas.ImageURL, (int)Math.Max(canvas.ImageDimensions.X, canvas.ImageDimensions.Y));
						}
						else
						{
							Mod.LoadedImagePaintings.Add(Position, ImagePaintings.GetTextureFromURL(canvas.ImageURL, (int)Math.Max(canvas.ImageDimensions.X, canvas.ImageDimensions.Y)));
						}

						if (Main.netMode == NetmodeID.MultiplayerClient)
						{
							ModPacket packet = mod.GetPacket();
							packet.Write((byte)MessageType.UpdateImageData);
							packet.WriteVector2(new Vector2(i, j));
							packet.Send();
						}
					}

					if (Mod.LoadedImagePaintings.ContainsKey(Position))
					{
						if (Mod.LoadedImagePaintings[Position] != default)
						{
							Vector2 Offset = new Vector2(-8, -8);
							Vector2 Zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
							if (Main.drawToScreen)
							{
								Zero = Vector2.Zero;
							}
							Vector2 PositionPerfected = canvas.Position.ToWorldCoordinates() - Main.screenPosition + Offset + Zero;
							Rectangle DestinationRect = new Rectangle((int)PositionPerfected.X, (int)PositionPerfected.Y, (int)(canvas.ImageDimensions.X * 16), (int)(canvas.ImageDimensions.Y * 16));
							Color DrawColor = Lighting.GetColor(i, j);
							spriteBatch.Draw(Mod.LoadedImagePaintings[Position], DestinationRect, DrawColor);
						}
						else
						{
							UpdateData();

						}
					}
					else
					{
						UpdateData();
					}
				}
			}

			return Main.LocalPlayer.HasBuff(mod.BuffType("Error"));
		}
	}
}