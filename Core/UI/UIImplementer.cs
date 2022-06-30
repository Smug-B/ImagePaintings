using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ImagePaintings.Core.UI
{
	public class UIImplementer : ModSystem
	{
		public override void Load()
		{
			if (Main.dedServ)
            {
				return;
            }

			foreach (Type type in Mod.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(AutoUIState))))
			{
				if (type.GetConstructor(Type.EmptyTypes) == null)
                {
					Mod.Logger.Warn(Mod.Name + " UIImplementer: AutoUIState" + type.Name + " had constructor parameters and was ignored.");
					continue;
				}

				AutoUIState autoUIState = Activator.CreateInstance(type) as AutoUIState;
				string uiStateName = type.Name;
				autoUIState.PreLoad(ref uiStateName);
				autoUIState.Mod = Mod;
				autoUIState.Name = uiStateName;
				autoUIState.DefaultSetUpInterface();
				autoUIState.UIHandler = autoUIState.Load();

				if (autoUIState.AutoAddHandler)
				{
					UIHandler.ProcessedUIs.Add(autoUIState.UIHandler);
				}

				ContentInstance.Register(autoUIState);
			}
		}

		public override void Unload() => UIHandler.ProcessedUIs?.Clear();

		public override void UpdateUI(GameTime gameTime) => UIHandler.HandleUpdate(gameTime);

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) => UIHandler.HandleModifyInterfaceLayers(layers);
	}
}