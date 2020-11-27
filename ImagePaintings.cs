using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ImagePaintings.Core.Tiles;
using ImagePaintings.Core.Items;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;
using System.Collections.Generic;
using Point = Microsoft.Xna.Framework.Point;

namespace ImagePaintings
{
	internal enum MessageType : byte
	{
		CreatePainting,
		UpdateImageData,
		KillPainting,
	}

	public class ImagePaintings : Mod
	{
		public Dictionary<Point16, Texture2D> LoadedImagePaintings = new Dictionary<Point16, Texture2D>();

		public void SetLIPData(Point16 position, Texture2D texture)
        {
			if (LoadedImagePaintings.ContainsKey(position))
			{
				LoadedImagePaintings[position] = texture;
			}
			else
			{
				LoadedImagePaintings.Add(position, texture);
			}
		}

		public static Texture2D GetTextureFromURL(string URL, int Scale)
		{
			Texture2D texture = ModContent.GetTexture("ImagePaintings/BruhCat");

			if (Scale <= 0)
			{
				string ErrorText = "Image dimensions cannot be Zero or Negative!";
				if (Main.dedServ)
				{
					NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(ErrorText), Microsoft.Xna.Framework.Color.Red); ;
				}
				else
				{
					Main.NewText(ErrorText, Microsoft.Xna.Framework.Color.Red);
				}
				return texture;
			}

			if (Scale > 50)
			{
				string ErrorText = "Those dimensions are a bit too large";
				if (Main.dedServ)
				{
					NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(ErrorText), Microsoft.Xna.Framework.Color.Red); ;
				}
				else
				{
					Main.NewText(ErrorText, Microsoft.Xna.Framework.Color.Red);
				}
				return texture;
			}

			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
			WebClient GetImage = new WebClient();
			Stream stream = GetImage.OpenRead(URL);
			if (!(stream is MemoryStream))
			{
				MemoryStream memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
				memoryStream.Position = 0L;
				stream = memoryStream;
			}
			texture = Texture2D.FromStream(Main.instance.GraphicsDevice, stream, Scale * 16, Scale * 16, false);
			return texture;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType type = (MessageType)reader.ReadByte();
			switch (type)
			{
				case MessageType.CreatePainting:
					{
						Vector2 Origin = reader.ReadVector2();
						int PlayerWhoAmI = reader.ReadInt32();
						if (Main.player[PlayerWhoAmI].active)
						{
							ImagePainting.CreatePainting(Origin.ToPoint(), PlayerWhoAmI, reader.ReadInt32());
						}
					}
					break;

				case MessageType.UpdateImageData:
					{
						Vector2 value = reader.ReadVector2();
						if (Main.netMode == NetmodeID.Server)
						{
							return;
						}
						CanvasTE canvas = TileEntity.ByPosition[new Point16((int)value.X, (int)value.Y)] as CanvasTE;
						ImagePaintings mod = ModContent.GetInstance<ImagePaintings>();
						Point16 pos = value.ToPoint16();
						Texture2D image = GetTextureFromURL(canvas.ImageURL, (int)Math.Max(canvas.ImageDimensions.X, canvas.ImageDimensions.Y));
						if (mod.LoadedImagePaintings.ContainsKey(pos))
						{
							mod.LoadedImagePaintings[pos] = image;
						}
						else
						{
							mod.LoadedImagePaintings.Add(pos, image);
						}
					}
					break;

				case MessageType.KillPainting:
					{
						Vector2 Position = reader.ReadVector2();
						if (TileEntity.ByPosition.ContainsKey(Position.ToPoint16()))
						{
							CanvasTE canvas = TileEntity.ByPosition[new Point16((int)Position.X, (int)Position.Y)] as CanvasTE;
							DeleteImagePainting(canvas);
						}
					}
					break;

				default:
					Logger.WarnFormat("Image Paintings: Unknown Message type: {0}", type);
					break;
			}
		}

		public static void DeleteImagePainting(CanvasTE te)
		{
			for (int X = te.Position.X; X < te.Position.X + te.ImageDimensions.X; X++)
			{
				for (int Y = te.Position.Y; Y < te.Position.Y + te.ImageDimensions.Y; Y++)
				{
					if (X <= 0 || X >= Main.maxTilesX || Y <= 0 || Y >= Main.maxTilesY)
					{
						continue;
					}

					Tile tile = Framing.GetTileSafely(X, Y);
					if ((tile.type == ModContent.TileType<BlankCanvas>() || tile.type == ModContent.TileType<NewCanvas>()) && tile.active())
					{
						tile.active(false);
						tile.halfBrick(false);
						tile.frameX = -1;
						tile.frameY = -1;
						tile.color(0);
						tile.frameNumber(0);
						tile.type = 0;
						tile.inActive(false);
					}

					NetMessage.SendTileRange(-1, X, Y, 1, 1, TileChangeType.None);
				}
			}

			ModContent.GetInstance<CanvasTE>().Kill(te.Position.X, te.Position.Y);
		}

		public override void Unload()
		{
			LoadedImagePaintings = null;
		}

		public override void PostUpdateEverything()
		{
			if (Main.netMode == NetmodeID.Server)
			{
				return;
			}
		}
	}
}