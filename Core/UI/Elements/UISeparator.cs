using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI;

namespace ImagePaintings.Core.UI.Elements
{
	public class UISeparator : UIElement
	{
        public Color Color;

        public UISeparator(Color? color = null) => Color = color ?? Color.White;

        public override void Draw(SpriteBatch spriteBatch) => spriteBatch.Draw(TextureAssets.MagicPixel.Value, GetDimensions().ToRectangle(), Color);
    }
}