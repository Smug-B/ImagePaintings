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
	public class NewCanvas : ModTile
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

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height)
		{
			base.SetDrawPositions(i, j, ref width, ref offsetY, ref height);
		}

		public CanvasTE FindTE(int i, int j, int FrameX, out int YAway)
		{
			bool Found = false;
			int Times = 0;
			Point16 CurrentTEPosCheck = new Point16(i - (FrameX / 16), j);
			while (!Found && Times < 33)
			{
				if (TileEntity.ByPosition.ContainsKey(CurrentTEPosCheck))
				{
					Found = true;
				}
				else
				{
					CurrentTEPosCheck = new Point16(CurrentTEPosCheck.X, CurrentTEPosCheck.Y - 1);
					Times++;
				}
			}

			YAway = Times;

			if (Times >= 33)
			{
				return null;
			}
			else
			{
				return TileEntity.ByPosition[CurrentTEPosCheck] as CanvasTE;
			}
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (!Main.LocalPlayer.HasBuff(mod.BuffType("Error")))
			{
				Point16 Position = new Point16(i, j);
				Tile tile = Framing.GetTileSafely(i, j);
				CanvasTE canvas = FindTE(i, j, tile.frameX, out int FrameY);

				if (canvas != null)
				{
					Point MyPlayerPos = Main.LocalPlayer.Center.ToTileCoordinates();
					int ClosestX = (int)(Math.Abs(canvas.Position.X - MyPlayerPos.X) < Math.Abs((canvas.Position.X + canvas.ImageDimensions.X - 1) - MyPlayerPos.X) ? canvas.Position.X : canvas.Position.X + canvas.ImageDimensions.X - 1);
					int ClosestY = (int)(Math.Abs(canvas.Position.Y - MyPlayerPos.Y) < Math.Abs((canvas.Position.Y + canvas.ImageDimensions.Y - 1) - MyPlayerPos.Y) ? canvas.Position.Y : canvas.Position.Y + canvas.ImageDimensions.Y - 1);
					if (Position == new Point16(ClosestX, ClosestY))
					{
						void UpdateData()
						{
							Mod.SetLIPData(canvas.Position, ImagePaintings.GetTextureFromURL(canvas.ImageURL, (int)Math.Max(canvas.ImageDimensions.X, canvas.ImageDimensions.Y)));

							if (Main.netMode == NetmodeID.MultiplayerClient)
							{
								ModPacket packet = mod.GetPacket();
								packet.Write((byte)MessageType.UpdateImageData);
								packet.WriteVector2(new Vector2(i, j));
								packet.Send();
							}
						}

						if (Mod.LoadedImagePaintings.ContainsKey(canvas.Position))
						{
							if (Mod.LoadedImagePaintings[canvas.Position] != null)
							{
								Vector2 Offset = new Vector2(-8, -8);
								Vector2 Zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
								if (Main.drawToScreen)
								{
									Zero = Vector2.Zero;
								}
								Vector2 PositionPerfected = canvas.Position.ToWorldCoordinates() - Main.screenPosition + Offset + Zero;
								Rectangle DestinationRect = new Rectangle((int)PositionPerfected.X, (int)PositionPerfected.Y, (int)(canvas.ImageDimensions.X * 16), (int)(canvas.ImageDimensions.Y * 16));
								Color DrawColor = Lighting.GetColor((int)(canvas.Position.X + canvas.ImageDimensions.X / 2), (int)(canvas.Position.Y + canvas.ImageDimensions.Y / 2));
								spriteBatch.Draw(Mod.LoadedImagePaintings[canvas.Position], DestinationRect, DrawColor);
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
			}

				return Main.LocalPlayer.HasBuff(mod.BuffType("Error"));
		}
	}
}