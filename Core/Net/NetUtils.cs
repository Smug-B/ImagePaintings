using System;
using System.Net;
using System.Net.NetworkInformation;
using Terraria;
using Terraria.ModLoader;

namespace ImagePaintings.Core.Net
{
    public static class NetUtils
    {
		public static IPAddress Google { get; private set; } = new IPAddress(new byte[] { 8, 8, 8, 8 });

        public static void Unload() => Google = null;

        /// <summary>
        /// A simple test to see whether an user is connected to the internet through attempting to reach Google
        /// </summary>
        /// <param name="timeOut">Verbatim. Defaults to 1000 ( 1 second )</param>
        /// <returns>Whether or not the user is online.</returns>
		public static bool Online(bool includeChatText = false)
        {
            ImagePaintingConfigs imagePaintingConfigs = ModContent.GetInstance<ImagePaintingConfigs>();

            if (imagePaintingConfigs.DisableOnlineTest)
            {
                return true;
            }

            int timeOut = imagePaintingConfigs.PingResponseTimeout;
            if (imagePaintingConfigs.AlternativeOnlineTest)
            {
                try
                {
                    HttpWebRequest request = WebRequest.Create(new Uri("https://www.google.com/")) as HttpWebRequest;
                    request.KeepAlive = false;
                    request.Timeout = timeOut;
                    using WebResponse response = request.GetResponse();
                    return true;
                }
                catch
                {
                    Main.NewText("Failed to load images as the client appears to be offline...");
                    Main.NewText("If this is not actually the case, please try turning on 'Disable Online Test' in your configurations and reload the image.");
                    return false;
                }
            }
            else
            {
                using Ping ping = new Ping();
                PingReply reply = ping.Send(Google, timeOut);
                bool result = reply != null && reply.Status == IPStatus.Success;
                if (includeChatText && !result)
                {
                    Main.NewText("Failed to load images as the client appears to be offline...");
                    Main.NewText("If this is not actually the case, please try turning on 'Alternative Online Test' in your configurations and reload the image.");
                }
                return result;
            }
        }
	}
}
