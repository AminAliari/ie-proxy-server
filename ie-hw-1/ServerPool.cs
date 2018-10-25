using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace ie_hw_1
{
    class ServerPool
    {
        public bool pEnable;

        private Socket proxy_server_;

        private int servers_count_;

        public ServerPool()
        {
            pEnable = true;
            servers_count_ = 0;
        }

        public void Start()
        {
            new Thread(ThreadStart).Start();
        }

        private void ThreadStart()
        {
            var listener = new TcpListener(IPAddress.Loopback, Manager.kServerPoolPort);
            listener.Start(100);

            proxy_server_ = listener.AcceptSocket();
            HandleProxy();

            listener.Stop();
        }

        private void HandleProxy()
        {
            while (pEnable)
            {
                try
                {
                    var data = Manager.sSingleton.ReadFromSocket(proxy_server_);
                    if (string.IsNullOrEmpty(data))
                    {
                        Manager.print("[error] empty proxy server request.");
                        continue;
                    }

                    if (data.StartsWith(Manager.CREATE_SERVER))
                    {
                        servers_count_++;
                        var ports = data.Split('|');
                        var server_broadcast_port = int.Parse(ports[1]);
                        var client_broadcast_port = int.Parse(ports[2]);
                        CreateServer(server_broadcast_port, client_broadcast_port);
                    }
                }
                catch (Exception e)
                {
                    Manager.print("[error] server pool: " + e.Message);
                }
            }
        }

        private void CreateServer(int server_broadcast_port, int client_broadcast_port)
        {
            new Server(() => Manager.sSingleton.SendToSocket(proxy_server_, Manager.SERVER_CREATED)).Start(server_broadcast_port, client_broadcast_port);
        }
    }
}
