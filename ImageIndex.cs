using System.IO;
using Terraria.ModLoader.IO;

namespace ImagePaintings
{
	public struct ImageIndex
	{
		public string URL;

		public int ResolutionSizeX;

		public int ResolutionSizeY;

		public ImageIndex(string url, int resSizeX, int resSizeY)
		{
			URL = url;
			ResolutionSizeX = resSizeX;
			ResolutionSizeY = resSizeY;
		}

		public TagCompound Save() => new TagCompound()
		{
			{ "URL", URL },
			{ "ResolutionSizeX", ResolutionSizeX },
			{ "ResolutionSizeY", ResolutionSizeY }
		};

		public static ImageIndex Load(TagCompound tag) => new ImageIndex(
			tag.Get<string>("URL"),
			tag.Get<int>("ResolutionSizeX"),
			tag.Get<int>("ResolutionSizeY"));

		public void NetSend(BinaryWriter writer)
        {
			if (URL == null)
            {
				writer.Write("Null");
				return;
			}

			writer.Write(URL);
			writer.Write(ResolutionSizeX);
			writer.Write(ResolutionSizeY);
		}

		public void NetReceive(BinaryReader reader)
        {
			string possibleURLValue = reader.ReadString();
			if (possibleURLValue != "Null")
			{
				URL = possibleURLValue;
				ResolutionSizeX = reader.ReadInt32();
				ResolutionSizeY = reader.ReadInt32();
			}
		}
	}
}