using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace ie_hw_1
{
    class ProxyServer
    {
        public bool pEnable;

        private Dictionary<Socket, int> clients_;
        private Dictionary<int, Connection> servers_;

        private Socket server_pool_;

        public ProxyServer()
        {
            pEnable = true;
            clients_ = new Dictionary<Socket, int>();
            servers_ = new Dictionary<int, Connection>();
        }

        public void Start()
        {
            new Thread(ThreadStart).Start();
        }

        private void ThreadStart()
        {
            ConnectToServerPool();

            var listener = new TcpListener(IPAddress.Loopback, Manager.kProxyPort);
            listener.Start(100);

            while (pEnable)
            {
                if (clients_.Count > Manager.kMaxClient)
                {
                    Manager.print("[error] maximum client reached.");
                    continue;
                }

                var client = listener.AcceptSocket();

                var server_listen_port = GetServer();
                if (server_listen_port == -1)
                {
                    Manager.print("[error] could not make a connection for client to server.");
                    continue;
                }

                clients_.Add(client, server_listen_port);

                var client_thread = new Thread(ThreadHandleClient);
                client_thread.Start(client);
            }
            listener.Stop();
        }

        private void ThreadHandleClient(object o)
        {
            while (pEnable)
            {
                try
                {
                    var client = (Socket)o;
                    var server_listen_port = clients_[client];
                    var connection = servers_[server_listen_port];

                    var request = Manager.sSingleton.ReadFromSocket(client);

                    if (string.IsNullOrEmpty(request))
                    {
                        Manager.print("[error] empty request from client.");
                        continue;
                    }

                    var request_bytes = Encoding.UTF8.GetBytes(request);
                    connection.pWriteSocket.SendTo(request_bytes, connection.pEpForClient);

                    var received_bytes = connection.pListenerForServer.Receive(ref connection.pEpForServer);
                    string response = Encoding.UTF8.GetString(received_bytes);

                    Manager.sSingleton.SendToSocket(client, response);
                }
                catch (Exception e)
                {
                    Manager.print("[error] proxy server: " + e.Message);
                }
            }
        }

        private void ConnectToServerPool()
        {
            server_pool_ = new Socket(IPAddress.Loopback.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server_pool_.Connect(IPAddress.Loopback, Manager.kServerPoolPort);
        }

        private int GetServer()
        {
            var server_broadcast_port = Manager.GetPort();
            var client_broadcast_port = Manager.GetPort();

            Manager.sSingleton.SendToSocket(server_pool_, Manager.CREATE_SERVER + server_broadcast_port + "|" + client_broadcast_port);

            var data_from_server_pool = Manager.sSingleton.ReadFromSocket(server_pool_);

            var decided_port = -1;
            if (data_from_server_pool.StartsWith(Manager.SERVER_CREATED))
            {
                var listener_for_server = new UdpClient(server_broadcast_port);
                var ep_for_client_ = new IPEndPoint(IPAddress.Broadcast, client_broadcast_port);

                var write_socket_ = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var ep_for_server_ = new IPEndPoint(IPAddress.Broadcast, server_broadcast_port);

                var connection = new Connection(server_broadcast_port, client_broadcast_port, ep_for_server_, ep_for_client_, listener_for_server, write_socket_);
                servers_.Add(server_broadcast_port, connection);

                decided_port = server_broadcast_port;
            }
            return decided_port;
        }
    }
}
