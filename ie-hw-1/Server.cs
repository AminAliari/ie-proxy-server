using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace ie_hw_1
{
    class Server
    {
        public bool pEnable;

        public delegate void OnServerCreated();
        public OnServerCreated on_server_created;

        Socket write_socket_;
        UdpClient listener_for_client_;
        IPEndPoint ep_for_server_, ep_for_client_;

        public Server(OnServerCreated on_server_created)
        {
            pEnable = true;
            this.on_server_created = on_server_created;
        }

        public void Start(int server_broadcast_port, int client_broadcast_port)
        {
            listener_for_client_ = new UdpClient(client_broadcast_port);
            ep_for_client_ = new IPEndPoint(IPAddress.Broadcast, client_broadcast_port);

            write_socket_ = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ep_for_server_ = new IPEndPoint(IPAddress.Broadcast, server_broadcast_port);

            on_server_created();
            new Thread(HandleClient).Start();
        }

        private void HandleClient()
        {
            while (pEnable)
            {
                try
                {
                    var received_bytes = listener_for_client_.Receive(ref ep_for_client_);

                    string request = Encoding.UTF8.GetString(received_bytes);

                    if (string.IsNullOrEmpty(request)) { throw new Exception("empty request"); }

                    var response = Encoding.UTF8.GetBytes(MakeResponse(request));
                    write_socket_.SendTo(response, ep_for_server_);
                }
                catch (Exception e)
                {
                    Manager.print("[error] server: " + e.Message);
                }
            }
        }

        private string MakeResponse(string request)
        {
            request = $"<html><body><p1>server answer:<p1/><br>{request}<body/><html/>";

            var content_len = Encoding.UTF8.GetBytes(request).Length;

            var header = $@"HTTP/1.1 200 OK
                        Date: Sun, 10 Oct 2010 23:26:07 GMT
                        Server: Apache/2.2.8 (Ubuntu) mod_ssl/2.2.8 OpenSSL/0.9.8g
                        Accept - Ranges: bytes
                        Content - Length: {content_len}
                        Connection: close
                        Content - Type: text / html";

            var response = $"{header}\n\n{request}";

            return response;
        }
    }
}