namespace ImagePaintings
{
	public struct ImageIndex
	{
		public string URL;

		public int SizeX;

		public int SizeY;

		public ImageIndex(string url, int sizeX, int sizeY)
		{
			URL = url;
			SizeX = sizeX;
			SizeY = sizeY;
		}
	}
}