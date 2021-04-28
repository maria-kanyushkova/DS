using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Chain
{
    class Program
    {
        private static Socket _sender;
        private static Socket _listener;
        private static int x;
        static void Main(string[] args)
        {
            var listenPort = Int32.Parse(args[0]);
            var address = args[1];
            var port = Int32.Parse(args[2]);
            bool isFirst = args.Length == 4 && args[3] == "true";

            CreateConnection(listenPort, address, port);

            x = Convert.ToInt32(Console.ReadLine());

            if (isFirst)
            {
                WorkAsInitiator();
            }
            else
            {
                WorkAsNormalProcess();
            }

            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();

        }
        private static void CreateConnection(int listenPort, string address, int port)
        {
            IPAddress listenIpAddress = IPAddress.Any;
            IPEndPoint localEP = new IPEndPoint(listenIpAddress, listenPort);
            _listener = new Socket(
                 listenIpAddress.AddressFamily,
                 SocketType.Stream,
                 ProtocolType.Tcp);

            _listener.Bind(localEP);
            _listener.Listen(10);

            IPAddress ipAddress = address == "localhost" ? IPAddress.Loopback : IPAddress.Parse(address);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            _sender = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            Connect(remoteEP);
        }
        private static void Connect(IPEndPoint remoteEP)
        {
            bool isConnect = false;
            for (int i = 0; i < 3 && !isConnect; ++i)
            {
                try
                {
                    _sender.Connect(remoteEP);
                    isConnect = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                }
            }
            if (!isConnect)
            {
                Console.WriteLine("Cannot connect to next process");
                return;
            }
        }
        private static void WorkAsInitiator()
        {
            var bytes = BitConverter.GetBytes(x);
            _sender.Send(bytes);

            Socket handler = _listener.Accept();

            //receive max from all
            byte[] buf = new byte[4];
            int bytesRec = handler.Receive(buf);
            int y = BitConverter.ToInt32(buf);

            x = y;
            Console.WriteLine(x);

            //send max from all to next
            bytes = BitConverter.GetBytes(Math.Max(x, y));
            int bytesSent = _sender.Send(bytes);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        private static void WorkAsNormalProcess()
        {
            Socket handler = _listener.Accept();

            //receive from previous
            byte[] buf = new byte[4];
            int bytesRec = handler.Receive(buf);
            int y = BitConverter.ToInt32(buf);

            //send max from two to next
            var bytes = BitConverter.GetBytes(Math.Max(x, y));
            int bytesSent = _sender.Send(bytes);

            //receive max from all
            buf = new byte[4];
            bytesRec = handler.Receive(buf);
            int receivedNumber = BitConverter.ToInt32(buf);
            Console.WriteLine(receivedNumber);

            //send max from all to next
            _sender.Send(buf);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}