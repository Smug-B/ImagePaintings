using System.Net;
using System.Net.NetworkInformation;
using Terraria;

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
		public static bool Online(int timeOut = 1000, bool includeChatText = false)
		{
            using Ping ping = new Ping();
            PingReply reply = ping.Send(Google, timeOut);
            bool result = reply != null && reply.Status == IPStatus.Success;
            if (includeChatText && !result)
            {
                Main.NewText("Failed to load images as the client appears to be offline...");
            }
            return result;
        }
	}
}
