using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace ie_hw_1
{
    class Server
    {
        public bool pEnable;

        public delegate void OnServerCreated();
        public OnServerCreated on_server_created;

        UdpClient client_;
        IPEndPoint ep_for_server_, ep_for_client_;

        public Server(OnServerCreated on_server_created)
        {
            pEnable = true;
            this.on_server_created = on_server_created;
        }

        public void Start(int server_broadcast_port)
        {
            ep_for_server_ = new IPEndPoint(IPAddress.Any, server_broadcast_port);
            client_ = new UdpClient(ep_for_server_);
            //client_.ExclusiveAddressUse = false;

            ep_for_client_ = new IPEndPoint(IPAddress.Any, 0);

            new Thread(HandleClient).Start();
        }

        private void HandleClient()
        {
            on_server_created();

            while (pEnable)
            {
                try
                {
                    var received_bytes = client_.Receive(ref ep_for_client_);

                    string request = Manager.sSingleton.ByteToString(received_bytes);

                    if (string.IsNullOrEmpty(request)) { throw new Exception("empty request"); }

                    var response = Manager.sSingleton.StringToBytes(MakeResponse(request));

                    client_.Send(response, response.Length, ep_for_client_);
                }
                catch (Exception e)
                {
                    Manager.print("[error] server: " + e.Message);
                    break;
                }
            }
        }

        private string MakeResponse(string request)
        {
            request = $"<html><body><p1>[server answer]<p1/><br><br>{ request}<br><br><p1>[server answer]<p1/><body/><html/>";

            var content_len = Manager.sSingleton.StringToBytes(request).Length;

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