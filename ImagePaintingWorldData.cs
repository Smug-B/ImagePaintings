using Microsoft.Xna.Framework;
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
            if (ModContent.GetInstance<ImagePaintingConfigs>().AlternativeDraw)
            {
                Terraria.IL_Main.DoDraw_WallsTilesNPCs += AlternativeDraw;
            }
            else
            {
                Terraria.GameContent.Drawing.IL_WallDrawing.DrawWalls += InsertPaintingDrawing;
            }
        }

        public override void PostDrawTiles()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            HandleAltDraw_AboveEverything();
            Main.spriteBatch.End();
        }

        private void AlternativeDraw(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0), i => i.MatchCall<Main>("DoDraw_WallsAndBlacks")))
            {
                Mod.Logger.Error("Failed to match first target for AlternativeDraw");
                return;
            }
            else
            {
                Mod.Logger.Info("Successfully matched first target for AlternativeDraw");
            }

            cursor.EmitDelegate(HandleAltDraw_BehindWall);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0), i => i.MatchCall<Main>("DrawWoF")))
            {
                Mod.Logger.Error("Failed to match second target for AlternativeDraw");
                return;
            }
            else
            {
                Mod.Logger.Info("Successfully matched second target for AlternativeDraw");
            }

            cursor.EmitDelegate(HandleAltDraw_BehindTiles);
        }

        private static void Alt_DrawPaintingBlock(Vector2 drawOffset, KeyValuePair<Rectangle, PaintingData> data)
        {
            int left = (int)(data.Key.X * 16 - drawOffset.X);
            int right = (int)(data.Key.Right * 16 - drawOffset.X);
            int top = (int)(data.Key.Y * 16 - drawOffset.Y);
            int bottom = (int)(data.Key.Bottom * 16 - drawOffset.Y);
            int buffer = 80; // 5 tiles
            if (right < -buffer || left > Main.screenWidth + buffer || bottom < -buffer || top > Main.screenWidth + buffer)
            {
                return;
            }

            Texture2D image = ImagePaintings.FetchImage(data.Value);
            if (image == null)
            {
                return;
            }

            for (int i = data.Key.X; i < data.Key.Right; i++)
            {
                for (int j = data.Key.Y; j < data.Key.Bottom; j++)
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

        private static void HandleAltDraw_BehindWall()
        {
            Vector2 drawOffset = Main.screenPosition;// - (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));

            if (WorldPaintingData == null)
            {
                return;
            }

            foreach (KeyValuePair<Rectangle, PaintingData> data in WorldPaintingData)
            {
                if (data.Value.DrawLayer != PaintingRenderLayer.BehindWall)
                {
                    continue;
                }

                Alt_DrawPaintingBlock(drawOffset, data);
            }
        }

        private static void HandleAltDraw_BehindTiles()
        {
            Vector2 drawOffset = Main.screenPosition;// - (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));
            if (WorldPaintingData == null)
            {
                return;
            }

            foreach (KeyValuePair<Rectangle, PaintingData> data in WorldPaintingData)
            {
                if (data.Value.DrawLayer != PaintingRenderLayer.BehindTiles)
                {
                    continue;
                }

                Alt_DrawPaintingBlock(drawOffset, data);
            }
        }

        private static void HandleAltDraw_AboveEverything()
        {
            Vector2 drawOffset = Main.screenPosition;// - (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));
            if (WorldPaintingData == null)
            {
                return;
            }

            foreach (KeyValuePair<Rectangle, PaintingData> data in WorldPaintingData)
            {
                if (data.Value.DrawLayer != PaintingRenderLayer.AboveEverything)
                {
                    continue;
                }

                Alt_DrawPaintingBlock(drawOffset, data);
            }
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

                        /*if (Framing.GetTileSafely(i, j).HasTile && Lighting.GetColor(i, j) == Color.Black)
                        {
                            return;
                        }*/

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