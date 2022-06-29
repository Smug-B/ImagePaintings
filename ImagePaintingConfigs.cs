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

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(true)]
		[Label("Accurate Lighting")]
		[Tooltip("Utilises multiple draw calls to get more accurate lighting onto image paintings."
		+ "\nSlightly less performant than the alternative of a singular light value for all tiles."
		+ "\nMaybe more memory intensive than the alternative.")]
		public bool AccurateLighting;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(true)]
		[Label("Placeholder Loading Texture")]
		[Tooltip("Utilises an embeded texture as a placeholder for loading images.")]
		public bool PlaceholderLoadingTexture;

		[BackgroundColor(200, 170, 130)]
		[DefaultValue(true)]
		[Label("GIFs")]
		[Tooltip("Determines whether or not GIFs are properly loaded. " 
			+ "\nIf this is disabled only the first frame of the GIF would be loaded."
			+ "\nSuggested for very low end PCs")]
		public bool GIFs;
	}
}