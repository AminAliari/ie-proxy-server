using System;
using System.Net.Sockets;
using System.Text;

namespace ie_hw_1
{
    class Manager
    {
        #region consts
        public const int kProxyPort = 29800;
        public const int kServerPoolPort = kProxyPort + 1;
        public const int kMaxClient = 100;
        public const int kBufferSize = 2048;

        public const string CREATE_SERVER = "CREATE_SERVER|";
        public const string SERVER_CREATED = "SERVER_CREATED|";

        private const bool kIsDebug = true;
        #endregion

        #region singleton
        public static Manager singleton_;
        public static Manager sSingleton
        {
            get
            {
                if (singleton_ == null)
                {
                    singleton_ = new Manager();
                }
                return singleton_;
            }
        }
        #endregion

        private static int init_port_ = kProxyPort + 2;

        static void Main(string[] args)
        {
            print("1. server\t2.client");

            while (true)
            {
                var input = ReadLine();
                if (input == "1")
                {
                    new ServerPool().Start();
                    print("[event] server pool was initialized...\n");
                    break;
                }
                else if (input == "2")
                {
                    new ProxyServer().Start();
                    print("[event] proxy server was initialized...\n");
                    break;
                }
            }
        }

        public string ReadFromSocket(Socket socket)
        {
            return ReadFromNetworkStream(new NetworkStream(socket));
        }

        public void SendToSocket(Socket socket, string data)
        {
            var data_bytes = Encoding.UTF8.GetBytes(data);
            socket.Send(data_bytes, data_bytes.Length, SocketFlags.None);
        }

        public void SendToTcpClient(TcpClient tcp_client, string data)
        {
            var data_bytes = Encoding.UTF8.GetBytes(data);
            tcp_client.GetStream().Write(data_bytes, 0, data_bytes.Length);
        }

        public string ReadFromTcpClient(TcpClient tcp_client)
        {
            return ReadFromNetworkStream(tcp_client.GetStream());
        }

        private string ReadFromNetworkStream(NetworkStream ns)
        {
            var data = "";
            var buffer = new byte[kBufferSize];
            var received_bytes_length = 0;

            do
            {
                received_bytes_length = ns.Read(buffer, 0, buffer.Length);
                data += Encoding.UTF8.GetString(buffer);
            } while (received_bytes_length == buffer.Length);

            ns.Close();

            return data;
        }


        public static int GetPort()
        {
            init_port_++;
            return init_port_;
        }

        public static void print(object o)
        {
            if (kIsDebug)
                Console.Out.WriteLine(o.ToString());
        }

        public static string ReadLine()
        {
            return Console.In.ReadLine();
        }
    }
}