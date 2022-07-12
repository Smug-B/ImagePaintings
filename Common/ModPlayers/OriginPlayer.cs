using ImagePaintings.Common.ModSystems;
using ImagePaintings.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ImagePaintings.Common.ModPlayers
{
    public class OriginPlayer : ModPlayer
    {
        public Vector2 PaintingPlaceOrigin;

        public static bool IsKeyPressed(Keys key) => Main.keyState.IsKeyDown(key) && Main.oldKeyState.IsKeyUp(key);

        public void ModifyPaintingPlaceOrigin(Item heldItem, int x, int y)
        {
            if (heldItem.ModItem is not PaintingBase paintingBase)
            {
                return;
            }

            PaintingPlaceOrigin.X = Utils.Clamp(PaintingPlaceOrigin.X + x, 0, paintingBase.PaintingData.SizeX - 1);
            PaintingPlaceOrigin.Y = Utils.Clamp(PaintingPlaceOrigin.Y + y, 0, paintingBase.PaintingData.SizeY - 1);
        }

        public Point AdjustPointForPlaceOrigin(Point point) => point - PaintingPlaceOrigin.ToPoint();

        public override void PostItemCheck()
        {
            if (Main.dedServ)
            {
                return;
            }

            Item heldItem = Main.mouseItem.IsAir ? Player.HeldItem : Main.mouseItem;
            if (heldItem.IsAir || (heldItem.type != ModContent.ItemType<ImagePainting>() && heldItem.type != ModContent.ItemType<NewImagePainting>()))
            {
                return;
            }

            ModifyPaintingPlaceOrigin(heldItem, 0, 0);

            if (!OriginSystem.ConfigPlacePaintingOriginKey.Current)
            {
                return;
            }

            ImagePaintingConfigs configs = ModContent.GetInstance<ImagePaintingConfigs>();
            if (configs.RightClickOriginConfiguration && Main.mouseRight && Main.mouseRightRelease)
            {
                if (Main.mouseY > Main.screenHeight * 0.66f)
                {
                    ModifyPaintingPlaceOrigin(heldItem, 0, 1);
                }
                else if (Main.mouseY < Main.screenHeight * 0.33f)
                {
                    ModifyPaintingPlaceOrigin(heldItem, 0, -1);
                }

                if (Main.mouseX > Main.screenWidth * 0.66f)
                {
                    ModifyPaintingPlaceOrigin(heldItem, 1, 0);
                }
                else if (Main.mouseX < Main.screenWidth * 0.33f)
                {
                    ModifyPaintingPlaceOrigin(heldItem, -1, 0);
                }

                return;
            }

            if (IsKeyPressed(Keys.Up))
            {
                ModifyPaintingPlaceOrigin(heldItem, 0, -1);
            }

            if (IsKeyPressed(Keys.Down))
            {
                ModifyPaintingPlaceOrigin(heldItem, 0, 1);
            }

            if (IsKeyPressed(Keys.Left))
            {
                ModifyPaintingPlaceOrigin(heldItem, -1, 0);
            }

            if (IsKeyPressed(Keys.Right))
            {
                ModifyPaintingPlaceOrigin(heldItem, 1, 0);
            }
        }

        public override void SaveData(TagCompound tag) => tag.Add("PaintingPlaceOrigin", PaintingPlaceOrigin);

        public override void LoadData(TagCompound tag) => PaintingPlaceOrigin = tag.Get<Vector2>("PaintingPlaceOrigin");
    }
}
