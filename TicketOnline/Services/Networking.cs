//{
//    public class Networking
//    {
//        public static string Gt
//    }
//}
using System.Net.Sockets;
using System.Net;

//namespace MediaSyncAPI.Utilities
namespace TicketOnline.Services
{
    public class Networking
    {
        /// <summary>
        /// Generates a local URL with the specified port.
        /// </summary>
        /// <param name="port">The port number to include in the URL. Defaults to "5000" if not provided.</param>
        /// <returns>
        /// A string representing the generated local URL, or an empty string if the local IP address is not available.
        /// </returns>
        public static string GenerateUrl(string port = "5000")
        {
            string localIP = GetLocalIpAddress();
            if (string.IsNullOrEmpty(localIP))
            {
                return "";
            }
            return $"https://{localIP}:{port}";
        }


        /// <summary>
        /// Retrieves the local IP address of the machine by establishing a connection to a remote server.
        /// </summary>
        /// <returns>
        /// A string representing the local IP address, or an empty string if the retrieval fails.
        /// </returns>
        public static string GetLocalIpAddress()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint.Address.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

    }
}

