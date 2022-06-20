using System.Linq;
using Terraria.ModLoader;

namespace ImagePaintings
{
    public class ImagePaintingSystem : ModSystem
    {
		public override void PostUpdateEverything()
		{
			bool usingLowMemory = ModContent.GetInstance<ImagePaintingConfigs>().LowMemoryMode;
			foreach (ImageIndex imageIndex in ImagePaintings.AllLoadedImages.Keys)
			{
				if (ImagePaintings.AllLoadedImages.TryGetValue(imageIndex, out ImageData imageData))
				{
					if (usingLowMemory || imageData.Texture == null)
					{
						imageData.TimeSinceLastUse++;
					}
				}
			}

			if (usingLowMemory)
			{
				ImagePaintings.AllLoadedImages = ImagePaintings.AllLoadedImages.Where(indexDataValue => indexDataValue.Value.TimeSinceLastUse <= 300).ToDictionary(index => index.Key, data => data.Value);
			}
		}
    }
}
