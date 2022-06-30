using System;
using System.IO;
using Terraria.ModLoader.IO;

namespace ImagePaintings
{
	[Obsolete("Should not be used. Exists purely to prevent cross version compatibility issues.")]
	public struct ObsoleteImageIndex
	{
		public string URL;

		public int SizeX;

		public int SizeY;

		public int FrameDuration;

		public int ResolutionSizeX;

		public int ResolutionSizeY;

		public ObsoleteImageIndex(string url, int sizeX, int sizeY, int frameDuration = 5, int resSizeX = -1, int resSizeY = -1)
		{
			URL = url;
			SizeX = sizeX;
			SizeY = sizeY;
			FrameDuration = frameDuration;
			ResolutionSizeX = resSizeX <= 0 ? SizeX * 16 : resSizeX;
			ResolutionSizeY = resSizeY <= 0 ? SizeY * 16 : resSizeY;
		}

		public TagCompound Save() => new TagCompound()
		{
			{ "URL", URL },
			{ "SizeX", SizeX },
			{ "SizeY", SizeY },
			{ "FrameDuration", FrameDuration },
			{ "ResolutionSizeX", ResolutionSizeX },
			{ "ResolutionSizeY", ResolutionSizeY }
		};

		public static ObsoleteImageIndex Load(TagCompound tag) => new ObsoleteImageIndex(
			tag.Get<string>("URL"),
			tag.Get<int>("SizeX"),
			tag.Get<int>("SizeY"),
			tag.Get<int>("FrameDuration"),
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
			writer.Write(SizeX);
			writer.Write(SizeY);
			writer.Write(FrameDuration);
			writer.Write(ResolutionSizeX);
			writer.Write(ResolutionSizeY);
		}

		public void NetReceive(BinaryReader reader)
		{
			string possibleURLValue = reader.ReadString();
			if (possibleURLValue != "Null")
			{
				URL = possibleURLValue;
				SizeX = reader.ReadInt32();
				SizeY = reader.ReadInt32();
				FrameDuration = reader.ReadInt32();
				ResolutionSizeX = reader.ReadInt32();
				ResolutionSizeY = reader.ReadInt32();
			}
		}
	}
}