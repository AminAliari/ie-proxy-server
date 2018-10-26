using System.Net;
using System.Net.Sockets;

namespace ie_hw_1
{
    class Connection
    {

        public int pServerPort;
        public UdpClient pClient;
        public IPEndPoint pEndPoint;

        public Connection(int server_port, UdpClient client)
        {
            pServerPort = server_port;
            pClient = client;
            pEndPoint = new IPEndPoint(IPAddress.Any, server_port);
        }
    }
}
