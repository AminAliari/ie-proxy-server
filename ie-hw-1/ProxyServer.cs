using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace ie_hw_1
{
    class ProxyServer
    {
        public bool pEnable;

        private Dictionary<int, Socket> clients_;
        private Dictionary<int, Connection> servers_;

        private Socket server_pool_;

        public ProxyServer()
        {
            pEnable = true;
            clients_ = new Dictionary<int, Socket>();
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

                var server_port = GetServer();
                if (server_port == -1)
                {
                    Manager.print("[error] could not make a connection for client to server.");
                    continue;
                }

                clients_.Add(server_port, client);

                var client_thread = new Thread(ThreadHandleClient);
                client_thread.Start(server_port);
            }
            listener.Stop();
        }

        private void ThreadHandleClient(object o)
        {
            Thread.Sleep(1000);

            while (pEnable)
            {
                try
                {
                    var server_port = (int)o;
                    var client = clients_[server_port];
                    var connection = servers_[server_port];

                    var request = Manager.sSingleton.ReadFromSocket(client);

                    if (string.IsNullOrEmpty(request))
                    {
                        Manager.print("[error] empty request from client.");
                        continue;
                    }

                    var request_bytes = Manager.sSingleton.StringToBytes(request);
                    connection.pClient.Send(request_bytes, request_bytes.Length, "localhost", connection.pServerPort);

                    var received_bytes = connection.pClient.Receive(ref connection.pEndPoint);

                    string response = Manager.sSingleton.ByteToString(received_bytes);
                    Manager.sSingleton.SendToSocket(client, response);
                }
                catch (Exception e)
                {
                    Manager.print("[error] proxy server: " + e.Message);
                    break;
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
            var server_port = Manager.GetPort();
            var client_port = Manager.GetPort();

            Manager.sSingleton.SendToSocket(server_pool_, Manager.CREATE_SERVER + server_port);

            var data_from_server_pool = Manager.sSingleton.ReadFromSocket(server_pool_);

            var decided_port = -1;
            if (data_from_server_pool.StartsWith(Manager.SERVER_CREATED))
            {
                var client = new UdpClient(client_port);

                var connection = new Connection(server_port, client);
                servers_.Add(server_port, connection);

                decided_port = server_port;
            }
            return decided_port;
        }
    }
}
