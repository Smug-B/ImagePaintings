namespace ImagePaintings
{
	public struct ImageIndex
	{
		public ImageIndex(string url, int sizeX, int sizeY)
		{
			URL = url;
			SizeX = sizeX;
			SizeY = sizeY;
		}

		public string URL;

		public int SizeX;

		public int SizeY;
	}
}