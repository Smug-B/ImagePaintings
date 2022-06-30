using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace ImagePaintings
{
	public struct PaintingData
	{
		public ImageIndex ImageIndex;

		public int SizeX;

		public int SizeY;

		public int FrameDuration;

		public float Brightness;

		public const float PercentBrightness = 255f / 100f;

		public PaintingData(ImageIndex imageIndex, int sizeX, int sizeY, int frameDuration = 5, float brightness = 0)
		{
			ImageIndex = imageIndex;
			SizeX = sizeX;
			SizeY = sizeY;
			FrameDuration = frameDuration;
			Brightness = Utils.Clamp(brightness > 0 ? PercentBrightness * brightness : 0, 0, 255);
		}

		public TagCompound Save() => new TagCompound()
		{
			{ "Index", ImageIndex.Save() },
			{ "SizeX", SizeX },
			{ "SizeY", SizeY },
			{ "FrameDuration", FrameDuration },
			{ "Brightness", Brightness },
		};

		public static PaintingData Load(TagCompound tag) => new PaintingData(
			ImageIndex.Load(tag.Get<TagCompound>("Index")),
			tag.Get<int>("SizeX"),
			tag.Get<int>("SizeY"),
			tag.Get<int>("FrameDuration"),
			tag.Get<float>("Brightness"));

		public void NetSend(BinaryWriter writer)
		{
			ImageIndex.NetSend(writer);
			writer.Write(SizeX);
			writer.Write(SizeY);
			writer.Write(FrameDuration);
			writer.Write(Brightness);
		}

		public void NetReceive(BinaryReader reader)
		{
			ImageIndex.NetReceive(reader);
			SizeX = reader.ReadInt32();
			SizeY = reader.ReadInt32();
			FrameDuration = reader.ReadInt32();
			Brightness = reader.ReadInt32();
		}
	}
}