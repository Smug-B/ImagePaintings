using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImagePaintings
{
	[BackgroundColor(100, 70, 55)]
	public class ImagePaintingConfigs : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(false)]
		public bool LowMemoryMode;

		[Header("Game")]
		[BackgroundColor(200, 170, 130)]
		[DefaultValue(true)]
		public bool AccurateLighting;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(true)]
		public bool PlaceholderLoadingTexture;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(true)]
		public bool GIFs;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(false)]
		public bool RightClickOriginConfiguration;

		[Header("Networking")]
		[BackgroundColor(200, 170, 130)]
		[DefaultValue(1000)]
		[Range(1000, 100000)]
		[Increment(500)]
		public int PingResponseTimeout;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(false)] 
		public bool AlternativeOnlineTest;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(false)]
		public bool DisableOnlineTest;

		[Header("Reload")]
		[ReloadRequired]
		[BackgroundColor(200, 170, 130)]
		[DefaultValue(false)]
		public bool AlternativeDraw;
	}
}