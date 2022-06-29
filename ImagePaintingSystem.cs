using ImagePaintings.Core.Graphics;
using System.Collections.Generic;
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
					imageData?.Update();
				}
			}

			/*if (usingLowMemory)
			{
				IList<KeyValuePair<ImageIndex, ImageData>> validPaintings = new List<KeyValuePair<ImageIndex, ImageData>>();
				foreach (KeyValuePair<ImageIndex, ImageData> data in ImagePaintings.AllLoadedImages)
                {
					if (data.Value != null)
                    {
						if (data.Value.TimeSinceLastUse <= 300)
                        {
							validPaintings.Add(data);
                        }
						else
                        {
							data.Value.Unload();
							validPaintings.Add(new KeyValuePair<ImageIndex, ImageData>(data.Key, null));
						}
                    }
                }

				ImagePaintings.AllLoadedImages = validPaintings.ToDictionary(index => index.Key, data => data.Value);
			}*/
		}

        public override void PreSaveAndQuit()
        {
			if (ImagePaintings.AllLoadedImages != null)
			{
				foreach (KeyValuePair<ImageIndex, ImageData> data in ImagePaintings.AllLoadedImages)
				{
					data.Value.Unload();
				}

				ImagePaintings.AllLoadedImages.Clear();
			}
		}
    }
}
