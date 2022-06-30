﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ImagePaintings
{
    public class ImagePaintingWorldData : ModSystem
    {
        public static List<KeyValuePair<Rectangle, PaintingData>> WorldPaintingData { get; private set; }

        public override void Load()
        {
            WorldPaintingData = new List<KeyValuePair<Rectangle, PaintingData>>();
            IL.Terraria.GameContent.Drawing.WallDrawing.DrawWalls += InsertPaintingDrawing;
        }

        private void InsertPaintingDrawing(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdloc(28)))
            {
                Mod.Logger.Error("Failed to match first target for InsertPaintingDrawing");
                return;
            }
            else
            {
                Mod.Logger.Info("Successfully matched first target for InsertPaintingDrawing");
            }
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Ldc_I4, 1);

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchBrtrue(out _)))
            {
                Mod.Logger.Error("Failed to match second target for InsertPaintingDrawing");
                return;
            }
            else
            {
                Mod.Logger.Info("Successfully matched second target for InsertPaintingDrawing");
            }

            ILLabel continueLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Ldloc, 25);
            cursor.Emit(OpCodes.Ldloc, 26);
            cursor.Emit(OpCodes.Ldloc, 14);
            cursor.EmitDelegate(HandleDrawing);

            cursor.Emit(OpCodes.Ldloc, 28);
            cursor.Emit(OpCodes.Ldc_I4, 0);
            cursor.Emit(OpCodes.Ble, continueLabel);

            MethodInfo wallLoaderPostDraw = typeof(WallLoader).GetMethod("PostDraw", BindingFlags.Public | BindingFlags.Static);
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall(wallLoaderPostDraw)))
            {
                Mod.Logger.Error("Failed to match third target for InsertPaintingDrawing");
                return;
            }
            else
            {
                Mod.Logger.Info("Successfully matched third target for InsertPaintingDrawing");
            }

            cursor.Emit(OpCodes.Ldloc, 25);
            cursor.Emit(OpCodes.Ldloc, 26);
            cursor.Emit(OpCodes.Ldloc, 14);
            cursor.EmitDelegate(HandleDrawing);

            cursor.MarkLabel(continueLabel);
        }

        private static void HandleDrawing(int i, int j, Vector2 zero)
        {
            Vector2 drawOffset = Main.screenPosition - zero;
            if (ModContent.GetInstance<ImagePaintingConfigs>().AccurateLighting)
            {
                if (TryFindPainting(new Point(i, j), out KeyValuePair<Rectangle, PaintingData> data, true, true))
                {
                    Texture2D image = ImagePaintings.FetchImage(data.Value);
                    if (image != null)
                    {
                        int x = (int)(i * 16 - drawOffset.X);
                        int y = (int)(j * 16 - drawOffset.Y);
                        float sourceWidth = image.Width / (float)data.Value.SizeX;
                        float widthScale = sourceWidth / 16f;
                        float sourceHeight = image.Height / (float)data.Value.SizeY;
                        float heightScale = sourceHeight / 16f;
                        Rectangle sourceRect = new Rectangle((int)((i - data.Key.X) * 16 * widthScale), (int)((j - data.Key.Y) * 16 * heightScale), (int)sourceWidth, (int)sourceHeight);
                        Color drawColor = data.Value.Brightness > 0 ? new Color(new Vector3(data.Value.Brightness)) : Lighting.GetColor(i, j);
                        Main.spriteBatch.Draw(image, new Rectangle(x, y, 16, 16), sourceRect, drawColor);
                    }
                }
            }
            else
            {
                if (TryFindPainting(new Point(i, j), out KeyValuePair<Rectangle, PaintingData> data, false, true))
                {
                    Texture2D image = ImagePaintings.FetchImage(data.Value);
                    if (image != null)
                    {
                        int x = (int)(i * 16 - drawOffset.X);
                        int y = (int)(j * 16 - drawOffset.Y);
                        Color drawColor = data.Value.Brightness > 0 ? new Color(new Vector3(data.Value.Brightness)) : Lighting.GetColor(i + data.Value.SizeX / 2, j + data.Value.SizeY / 2);
                        Main.spriteBatch.Draw(image, new Rectangle(x, y, data.Value.SizeX * 16, data.Value.SizeY * 16), drawColor);
                    }
                }
            }
        }

        public override void OnWorldLoad() => WorldPaintingData.Clear();

        public override void OnWorldUnload() => WorldPaintingData.Clear();

        public override void SaveWorldData(TagCompound tag) => tag.Add("WorldPaintingData", WorldPaintingData.Select(data => new TagCompound()
        {
            { "Location", data.Key.Location.ToVector2() },
            { "Data", data.Value.Save() },
        }).ToList());

        public override void LoadWorldData(TagCompound tag) => WorldPaintingData = tag.Get<List<TagCompound>>("WorldPaintingData").Select(tag =>
        {
            Point location = tag.Get<Vector2>("Location").ToPoint();
            PaintingData paintingData = PaintingData.Load(tag.Get<TagCompound>("Data"));
            return new KeyValuePair<Rectangle, PaintingData>(new Rectangle(location.X, location.Y, paintingData.SizeX, paintingData.SizeY), paintingData);
        }).ToList();

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(WorldPaintingData.Count);
            foreach (KeyValuePair<Rectangle, PaintingData> paintingData in WorldPaintingData)
            {
                writer.Write(paintingData.Key.X);
                writer.Write(paintingData.Key.Y);
                paintingData.Value.NetSend(writer);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            WorldPaintingData.Clear();
            int worldPaintingDataCount = reader.ReadInt32();
            for (int i = 0; i < worldPaintingDataCount; i++)
            {
                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                PaintingData paintingData = new PaintingData();
                paintingData.NetReceive(reader);
                WorldPaintingData.Add(new KeyValuePair<Rectangle, PaintingData>(new Rectangle(x, y, paintingData.SizeX, paintingData.SizeY), paintingData));
            }
        }

        public static bool TryFindPainting(Point point, out KeyValuePair<Rectangle, PaintingData> data, bool hitBoxCheck = true, bool backToFrontSearch = false)
        {
            if (backToFrontSearch)
            {
                for (int i = WorldPaintingData.Count - 1; i >= 0; i--)
                {
                    KeyValuePair<Rectangle, PaintingData> dataPair = WorldPaintingData[i];
                    if (hitBoxCheck && dataPair.Key.Contains(point))
                    {
                        data = dataPair;
                        return true;
                    }
                    else if (dataPair.Key.Location == point)
                    {
                        data = dataPair;
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < WorldPaintingData.Count; i++)
                {
                    KeyValuePair<Rectangle, PaintingData> dataPair = WorldPaintingData[i];
                    if (hitBoxCheck && dataPair.Key.Contains(point))
                    {
                        data = dataPair;
                        return true;
                    }
                    else if (dataPair.Key.Location == point)
                    {
                        data = dataPair;
                        return true;
                    }
                }
            }

            data = new KeyValuePair<Rectangle, PaintingData>();
            return false;
        }
    }
}