using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using ImagePaintings.Core.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using System.Linq;
using System.Reflection;
using SixLabors.ImageSharp.Metadata;
using ImagePaintings.Core.UI;
using System.Drawing.Imaging;
using WinImage = System.Drawing.Image;
using static Terraria.GameContent.Bestiary.IL_BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;
using System.Runtime.Versioning;

namespace ImagePaintings.Core.Net
{
    public class ImagePaintingClient : IDisposable
    {
        public static IPAddress[] Google { get; private set; }

        public HttpClient Client { get; }

        private delegate Image<Rgba32> ImageCtorDelegate(Configuration configuration, ImageMetadata metaData, IEnumerable<ImageFrame<Rgba32>> frames);

        private readonly static ConstructorInfo ImageCtor;

        static ImagePaintingClient()
        {
            Google = new IPAddress[]
            {
                new IPAddress(new byte[] { 8, 8, 8, 8 }),
                new IPAddress(new byte[] { 8, 8, 4, 4 }),
            };

            Type gifType = typeof(Image<Rgba32>);
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            Type[] ctorParameters = new Type[] { typeof(Configuration), typeof(ImageMetadata), typeof(IEnumerable<ImageFrame<Rgba32>>) };
            ImageCtor = gifType.GetConstructor(bindingFlags, ctorParameters);
        }

        public ImagePaintingClient() => Client = new HttpClient();

        public async Task<bool> IsOnline()
        {
            try
            {
                ImagePaintingConfigs imagePaintingConfigs = ModContent.GetInstance<ImagePaintingConfigs>();
                if (imagePaintingConfigs.DisableOnlineTest)
                {
                    return true;
                }

                int timeOut = imagePaintingConfigs.PingResponseTimeout;

                using Ping ping = new Ping();
                foreach (IPAddress googleIPAddress in Google)
                {
                    PingReply reply = ping.Send(googleIPAddress, timeOut);
                    if (reply?.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }

                HttpResponseMessage response = await Client.GetAsync("https://www.google.com/");
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                Main.NewText(exception);
            }
            return false;
        }

        public void LoadTexture(ImageIndex imageIndex)
        {
            string uri = imageIndex.URL;

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.SystemDefault;
                HttpResponseMessage responseMessage = Client.GetAsync(uri).Result;
                string rawMediaType = responseMessage.Content.Headers.ContentType.MediaType ?? throw new Exception("Could not recognize the media type associated with: " + uri);
                string[] readableMediaType = rawMediaType.Split('/', StringSplitOptions.RemoveEmptyEntries);
                string mediaType = readableMediaType[0];
                string mediaSubType = readableMediaType[1];
                if (mediaType == "image")
                {
                    switch (mediaSubType)
                    {
                        case "png":
                        case "jpeg":
                        case "jpg":
                            TryLoadImage(imageIndex, responseMessage.Content);
                            break;

                        case "gif":
                            TryLoadGIF(imageIndex, responseMessage.Content);
                            break;

                        default:
                            throw new Exception("Could not recognize the media type associated with: " + uri);
                    }
                }
                else if (mediaType == "text" && mediaSubType == "html")
                {

                }
            }
            catch (Exception exception)
            {
                ImagePaintings.Mod.Logger.Error(exception);
                Main.NewText("An error seems to have occured when fetching the image from the given URL.");
                Main.NewText("Please check your logs for more details.");
            }
        }

        private static Image NewImage(Configuration configuration, ImageMetadata metaData, IEnumerable<ImageFrame<Rgba32>> frames) => (Image<Rgba32>)ImageCtor.Invoke(new object[] { configuration, metaData, frames });

        private static void TryLoadImage(ImageIndex imageIndex, HttpContent httpContent)
        {
            try
            {
                using Stream contentStream = httpContent.ReadAsStream();
                contentStream.Position = 0;
                Main.QueueMainThreadAction(() =>
                {
                    try
                    {
                        Main.NewText("Attempted to load Texture2D");
                        Main.NewText(Main.instance.GraphicsDevice.ToString());
                        Main.NewText("Stream length: " + contentStream.Length);

                        Texture2D loadedImage = Texture2D.FromStream(Main.instance.GraphicsDevice, contentStream, imageIndex.ResolutionSizeX, imageIndex.ResolutionSizeY, false);
                        //ImagePaintings.AllLoadedImages[imageIndex] = new ImageData(loadedImage);
                    }
                    catch (Exception exception)
                    {
                        ImagePaintings.Mod.Logger.Error(exception);
                        Main.NewText("An error seems to have occured when loading: " + imageIndex.URL);
                        Main.NewText("Please check your logs for more details.");
                    }
                });

                while (ImagePaintings.AllLoadedImages[imageIndex].GetTexture == null)
                {

                }

                Main.NewText("We made it out the hood", 255, 100, 180);
            }
            catch (Exception exception)
            {
                ImagePaintings.Mod.Logger.Error(exception);
                Main.NewText("An error seems to have occured when serializing: " + imageIndex.URL);
                Main.NewText("Please check your logs for more details.");
            }
        }

        private static void TryLoadGIF(ImageIndex imageIndex, HttpContent httpContent)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Main.NewText("Loaded GIF WINDOWS");
                TryLoadGIF_Windows(imageIndex, httpContent);
            }
            else
            {
                Main.NewText("Loaded GIF CROSS PLATFORM");
                TryLoadGIF_CrossPlatform(imageIndex, httpContent);
            }
        }

        [SupportedOSPlatform("windows")]
        private static void TryLoadGIF_Windows(ImageIndex imageIndex, HttpContent httpContent)
        {
            try
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    throw new Exception("Attempted to load " + imageIndex.URL + " using tools that only support Windows.");
                }

                using Stream contentStream = httpContent.ReadAsStream();
                using WinImage gifImage = WinImage.FromStream(contentStream);
                List<GIFFrame> loadedFrames = new List<GIFFrame>();
                int frameCount = gifImage.GetFrameCount(FrameDimension.Time);
                byte[] frameDurationData = gifImage.GetPropertyItem(0x5100).Value;

                Main.QueueMainThreadAction(() =>
                {
                    try
                    {
                        for (int frameIndexer = 0; frameIndexer < frameCount; frameIndexer++)
                        {
                            gifImage.SelectActiveFrame(FrameDimension.Time, frameIndexer);
                            using MemoryStream gifFrameStream = new MemoryStream();
                            gifImage.Save(gifFrameStream, ImageFormat.Png);
                            Texture2D image = Texture2D.FromStream(Main.instance.GraphicsDevice, gifFrameStream, imageIndex.ResolutionSizeX, imageIndex.ResolutionSizeX, false);
                            loadedFrames.Add(new GIFFrame(image, BitConverter.ToInt32(frameDurationData, 4 * frameIndexer)));
                        };

                        ImagePaintings.AllLoadedImages[imageIndex] = new GIFData(loadedFrames);
                    }
                    catch (Exception exception)
                    {
                        ImagePaintings.Mod.Logger.Error(exception);
                        Main.NewText("An error seems to have occured when fetching the image from the given URL.");
                        Main.NewText("Please check your logs for more details.");
                    }
                });

                while (ImagePaintings.AllLoadedImages[imageIndex].GetTexture == null)
                {

                }

                Main.NewText("We made it out the hood", 255, 100, 180);
            }
            catch (Exception exception)
            {
                ImagePaintings.Mod.Logger.Error(exception);
                Main.NewText("An error seems to have occured when fetching the image from the given URL.");
                Main.NewText("Please check your logs for more details.");
            }
        }

        private static ImageData TryLoadGIF_CrossPlatform(ImageIndex imageIndex, HttpContent httpContent)
        {
            try
            {
                // TO DO (later): Find a way to differentiate between GIFs that record frame and those that record change...
                using Stream contentStream = httpContent.ReadAsStream();
                List<GIFFrame> loadedFrames = null;
                using Image<Rgba32> gifImage = (Image<Rgba32>)Image.Load(contentStream);
                int totalFrames = gifImage.Frames.Count;
                Configuration imageConfiguration = gifImage.GetConfiguration();
                List<Image<Rgba32>> cachedImages = new List<Image<Rgba32>>();

                using Image<Rgba32> imageCanvas = new Image<Rgba32>(imageConfiguration, gifImage.Width, gifImage.Height);
                for (int i = 0; i < totalFrames; i++)
                {
                    using Image gifFrame = NewImage(imageConfiguration, gifImage.Metadata.DeepClone(), new ImageFrame<Rgba32>[] { gifImage.Frames[i] });
                    imageCanvas.Mutate(x => x.DrawImage(gifFrame, 1));
                    cachedImages.Add(imageCanvas.Clone());
                }

                Main.QueueMainThreadAction(() =>
                {
                    try
                    {
                        loadedFrames = cachedImages.Select(i =>
                        {
                            using MemoryStream gifFrameStream = new MemoryStream();
                            i.Save(gifFrameStream, new GifEncoder());
                            i.Dispose();
                            Main.NewText(gifImage.Height);
                            //Texture2D image = Texture2D.FromStream(Main.instance.GraphicsDevice, gifFrameStream, imageIndex.ResolutionSizeX, imageIndex.ResolutionSizeX, false);
                            //loadedFrames.Add(new GIFFrame(image, BitConverter.ToInt32(frameDurationData, 4 * frameIndexer)));
                            //Texture2D image = Texture2D.FromStream(Main.instance.GraphicsDevice, gifFrameStream, imageIndex.ResolutionSizeX, imageIndex.ResolutionSizeY, false);
                            return new GIFFrame(null, 100);// new GIFFrame(image, i.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay);
                        }).ToList();

                        //ImagePaintings.AllLoadedImages[imageIndex] = new GIFData(loadedFrames);
                    }
                    catch (Exception exception)
                    {
                        ImagePaintings.Mod.Logger.Error(exception);
                        Main.NewText("An error seems to have occured when loading: " + imageIndex.URL);
                        Main.NewText("Please check your logs for more details.");
                    }
                });

                while (ImagePaintings.AllLoadedImages[imageIndex].GetTexture == null)
                {

                }

                Main.NewText("We made it out the hood", 255, 100, 180);
            }
            catch (Exception exception)
            {
                ImagePaintings.Mod.Logger.Error(exception);
                Main.NewText("An error seems to have occured when serializing: " + imageIndex.URL);
                Main.NewText("Please check your logs for more details.");
            }

            return null;
        }

        public void Dispose() 
        {
            Client.Dispose();
            Google = null;
            GC.SuppressFinalize(this);
        }
    }
}
