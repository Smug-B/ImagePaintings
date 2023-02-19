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

		public PaintingRenderLayer DrawLayer;

		public PaintingData(ImageIndex imageIndex, int sizeX, int sizeY, int frameDuration = 5, float brightness = 0, PaintingRenderLayer drawLayer = PaintingRenderLayer.AboveEverything)
		{
			ImageIndex = imageIndex;
			SizeX = sizeX;
			SizeY = sizeY;
			FrameDuration = frameDuration;
			Brightness = Utils.Clamp(brightness > 0 ? PercentBrightness * brightness : 0, 0, 255);
			DrawLayer = drawLayer;
		}

		public TagCompound Save() => new TagCompound()
		{
			{ "Index", ImageIndex.Save() },
			{ "SizeX", SizeX },
			{ "SizeY", SizeY },
			{ "FrameDuration", FrameDuration },
			{ "Brightness", Brightness },
			{ "DrawLayer", (byte)DrawLayer },
		};

		public static PaintingData Load(TagCompound tag) => new PaintingData(
			ImageIndex.Load(tag.Get<TagCompound>("Index")),
			tag.Get<int>("SizeX"),
			tag.Get<int>("SizeY"),
			tag.Get<int>("FrameDuration"),
			tag.Get<float>("Brightness"),
            (PaintingRenderLayer)tag.Get<byte>("DrawLayer"));

		public void NetSend(BinaryWriter writer)
		{
			ImageIndex.NetSend(writer);
			writer.Write(SizeX);
			writer.Write(SizeY);
			writer.Write(FrameDuration);
			writer.Write(Brightness);
			writer.Write((byte)DrawLayer);
		}

		public void NetReceive(BinaryReader reader)
		{
			ImageIndex.NetReceive(reader);
			SizeX = reader.ReadInt32();
			SizeY = reader.ReadInt32();
			FrameDuration = reader.ReadInt32();
			Brightness = reader.ReadInt32();
			DrawLayer = (PaintingRenderLayer)reader.ReadByte();
		}

		public void BareNetSend(BinaryWriter writer)
        {
			writer.Write(SizeX);
			writer.Write(SizeY);
		}

		public void BareNetRecieve(BinaryReader reader)
        {
			SizeX = reader.ReadInt32();
			SizeY = reader.ReadInt32();
		}
	}
}