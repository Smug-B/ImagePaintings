using Terraria;
using Terraria.UI;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ImagePaintings.Core.UI
{
	public class UIHandler
	{
		public static IList<UIHandler> ProcessedUIs { get; private set; } = new List<UIHandler>();

		public UserInterface Interface;

		public string DrawLayer;

		public string LayerName;

		public GameInterfaceDrawMethod DelegateDraw;

		public InterfaceScaleType InterfaceScaleType;

		public UIHandler(UserInterface userInterface, string drawLayer, string layerName, GameInterfaceDrawMethod delegateDraw = null, InterfaceScaleType interfaceScaleType = InterfaceScaleType.UI)
		{
			Interface = userInterface;
			DrawLayer = drawLayer;
			LayerName = layerName;
			DelegateDraw = delegateDraw;
			InterfaceScaleType = interfaceScaleType;
		}

		public virtual void Update(GameTime gameTime) => Interface?.Update(gameTime);

		public virtual void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals(DrawLayer));

			if (inventoryIndex == -1)
			{
				return;
			}

			layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(LayerName, DelegateDraw ?? DefaultDraw, InterfaceScaleType));
		}

		public bool DefaultDraw()
		{
			Interface?.Draw(Main.spriteBatch, new GameTime());
			return true;
		}

		public static void HandleUpdate(GameTime gameTime)
		{
			if (ProcessedUIs == null)
            {
				return;
            }

			foreach (UIHandler uiHandler in ProcessedUIs)
			{
				uiHandler.Update(gameTime);
			}
		}

		public static void HandleModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			if (ProcessedUIs == null)
			{
				return;
			}

			foreach (UIHandler uiHandler in ProcessedUIs)
			{
				uiHandler.ModifyInterfaceLayers(layers);
			}
		}
	}
}