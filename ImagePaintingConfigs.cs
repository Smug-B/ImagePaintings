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
		[Label("Low Memory Mode")]
		[Tooltip("NOTICE: This setting is currently ignored." 
		+ "\nDiscards image textures shortly when said texture is not in use.")]
		public bool LowMemoryMode;

		[Header("Game Quality of Life")]
		[BackgroundColor(200, 170, 130)]
		[DefaultValue(true)]
		[Label("Per Tile Drawing")]
		[Tooltip("Draws each tile individually."
		+ "\nSlightly more cost ineffective than the alternative."
		+ "\nDisability incompatible with Alternative Draw."
		+ "\nDisability incompatible with large paintings.")]
		public bool AccurateLighting;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(true)]
		[Label("Placeholder Loading Texture")]
		[Tooltip("Utilises an embeded texture as a placeholder for loading images.")]
		public bool PlaceholderLoadingTexture;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(true)]
		[Label("GIFs")]
		[Tooltip("Determines whether or not GIFs are properly loaded." 
			+ "\nIf this is disabled only the first frame of the GIF would be loaded."
			+ "\nSuggested for very low end PCs")]
		public bool GIFs;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(false)]
		[Label("Right Click Origin Configuration")]
		[Tooltip("Allows you to modify your painting placement origin through right clicking!"
			+ "\nSimply press and hold onto your binded 'Configure Place Origin' key while having a painting in hand, "
			+ "then right click different areas of your screen to change your place origin."
			+ "\nUseful when you don't own a 75+% keyboard.")]
		public bool RightClickOriginConfiguration;

		[Header("Networking")]
		[BackgroundColor(200, 170, 130)]
		[DefaultValue(1000)]
		[Range(1000, 100000)]
		[Increment(500)]
		[Label("Ping Response Timeout")]
		[Tooltip("Determines the timeout on the standard connectivity test in milliseconds."
			+"\nNote: 1,000 milliseconds = 1 second")]
		public int PingResponseTimeout;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(false)]
		[Label("Alternative Online Test")]
		[Tooltip("Utilises an alternate connectivity test."
			+ "\nUseful in cases where the default option fails to adequately check for connectivity."
			+ "\nIt's suggested that, before using this option, make sure that your firewall or network are not blocking ping attempts.")]
		public bool AlternativeOnlineTest;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(false)]
		[Label("Disable Online Test")]
		[Tooltip("Disables the online test done before attempting to pull images."
			+ "\nUseful in circumstances where all other options fail despite you being connected to the internet."
			+ "\nNOTICE: If you are offline, and the image request connection fails, this may or may not crash your game.")]
		public bool DisableOnlineTest;

		[Header("Reload Required")]
		[ReloadRequired]
		[BackgroundColor(200, 170, 130)]
		[DefaultValue(false)]
		[Label("Alternative Draw ( > 12 FPS on GIFs )")]
		[Tooltip("Utilises an alternative drawing method that allows for: "
            + "\n- More FPS on GIF paintings, "
			+ "\n- Painting layering options,"
			+ "\nAt the cost of performance.")]
		public bool AlternativeDraw;
	}
}