using System.Net;
using System.Net.Sockets;

namespace ie_hw_1
{
    class Connection
    {

        public int pServerBroadcastPort, pClientBroadcastPort;
        public IPEndPoint pEpForServer, pEpForClient;
        public UdpClient pListenerForServer;
        public Socket pWriteSocket;

        public Connection(int server_broadcast_port, int client_broadcast_port, IPEndPoint ep_for_server, IPEndPoint ep_for_client, UdpClient listener_for_server, Socket write_socket)
        {
            pServerBroadcastPort = server_broadcast_port;
            pClientBroadcastPort = client_broadcast_port;
            pEpForServer = ep_for_server;
            pEpForClient = ep_for_client;
            pListenerForServer = listener_for_server;
            pWriteSocket = write_socket;
        }
    }
}
