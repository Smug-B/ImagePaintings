using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
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
using System.Drawing.Imaging;
using WinImage = System.Drawing.Image;
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

        public bool IsOnline()
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

                HttpResponseMessage response = Client.GetAsync("https://www.google.com/").Result;
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                ImagePaintings.Mod.Logger.Error(exception);
            }
            return false;
        }

        public void LoadTexture(ImageIndex imageIndex)
        {
            string uri = imageIndex.URL;
            try
            {
                if (!IsOnline())
                {
                    Main.NewText("You appear to be offline! If this is not the case, try relogging into your world or disabling the online test through configs.");
                    return;
                }

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
                    Main.NewText("The given url is not an image address: " + uri);
                    Main.NewText("Try right clicking on the image and then copy the image address");
                    throw new Exception("Media type of website associated with: " + uri);
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
                        Texture2D loadedImage = Texture2D.FromStream(Main.instance.GraphicsDevice, contentStream, imageIndex.ResolutionSizeX, imageIndex.ResolutionSizeY, false);
                        ImagePaintings.AllLoadedImages[imageIndex] = new ImageData(loadedImage);
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
                TryLoadGIF_Windows(imageIndex, httpContent);
            }
            else
            {
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
                            loadedFrames.Add(new GIFFrame(image, (int)(BitConverter.ToInt32(frameDurationData, 4 * frameIndexer) * 0.6f)));
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
                            Texture2D image = Texture2D.FromStream(Main.instance.GraphicsDevice, gifFrameStream, imageIndex.ResolutionSizeX, imageIndex.ResolutionSizeX, false);
                            return new GIFFrame(image, (int)(i.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay * 0.6f));
                        }).ToList();

                        ImagePaintings.AllLoadedImages[imageIndex] = new GIFData(loadedFrames);
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