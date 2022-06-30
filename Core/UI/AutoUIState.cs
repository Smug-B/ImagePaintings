using Terraria.ModLoader;
using Terraria.UI;

namespace ImagePaintings.Core.UI
{
	public abstract class AutoUIState : UIState
	{
		public Mod Mod { get; internal set; }

		public string Name { get; internal set; }

		public string LayerName => Mod.Name + ": " + Name;

		public bool AutoSetState { get; protected set; }

		public bool AutoAddHandler { get; protected set; }

		public UserInterface UserInterface { get; private set; }

		public UIHandler UIHandler { get; internal set; }

		public virtual void PreLoad(ref string name)
		{
			AutoSetState = true;
			AutoAddHandler = true;
		}

		public abstract UIHandler Load();

		internal void DefaultSetUpInterface()
		{
			UserInterface = new UserInterface();

			if (AutoSetState)
			{
				UserInterface.SetState(this);
			}
		}
	}
}