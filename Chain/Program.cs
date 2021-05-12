using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Chain
{
    internal class Program
    {
        private static Socket _receiver;
        private static Socket _sender;

        static void Main(string[] args)
        {
            var listeningPort = Int32.Parse(args[0]);
            var nextHost = args[1];
            var nextPort = Int32.Parse(args[2]);
            var isInit = false;

            if (args.Length >= 4)
            {
                isInit = bool.Parse(args[3]);
            }

            Start(listeningPort, nextHost, nextPort, isInit);
        }

        public static void Start(int listeningPort, string nextHost, int nextPort, bool isInit)
        {
            try
            {
                var prevIpAddress = IPAddress.Any;
                var nextIpAddress = (nextHost == "localhost") ? IPAddress.Loopback : IPAddress.Parse(nextHost);

                var prevEp = new IPEndPoint(prevIpAddress, listeningPort);
                var nextEp = new IPEndPoint(nextIpAddress, nextPort);

                _sender = new Socket(nextIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _receiver = new Socket(prevIpAddress.AddressFamily, SocketType.Stream,ProtocolType.Tcp);

                try
                {
                    _receiver.Bind(prevEp);
                    _receiver.Listen(10);

                    Connect(nextEp);

                    var number = Console.ReadLine();
                    var x = Convert.ToInt32(number);

                    var listenerHandler = _receiver.Accept();
                    
                    if (isInit)
                        WorkAsInitiator(listenerHandler, x);
                    else
                        WorkAsProcess(listenerHandler, x);
                    
                    listenerHandler.Shutdown(SocketShutdown.Both);
                    listenerHandler.Close();

                    _sender.Shutdown(SocketShutdown.Both);
                    _sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        
        private static void WorkAsInitiator(Socket listenerHandler, int x)
        {
            var msg = Encoding.UTF8.GetBytes(x.ToString());
            var bytesSent = _sender.Send(msg);

            var buf = new byte[1024];
            var bytesRec = listenerHandler.Receive(buf);
            var data = Encoding.UTF8.GetString(buf, 0, bytesRec);
            var y = Int32.Parse(data);

            x = y;

            msg = Encoding.UTF8.GetBytes(x.ToString());
            bytesSent = _sender.Send(msg);

            buf = new byte[1024];
            bytesRec = listenerHandler.Receive(buf);
            data = Encoding.UTF8.GetString(buf, 0, bytesRec);
            x = Int32.Parse(data);

            Console.Write(x);
        }

        private static void WorkAsProcess(Socket listenerHandler, int x)
        {
            var buf = new byte[1024];
            var bytesRec = listenerHandler.Receive(buf);
            var data = Encoding.UTF8.GetString(buf, 0, bytesRec);
            var y = Int32.Parse(data);

            var maxOfXandY = Math.Max(x, y);

            var msg = Encoding.UTF8.GetBytes(maxOfXandY.ToString());
            var bytesSent = _sender.Send(msg);
            
            buf = new byte[1024];
            bytesRec = listenerHandler.Receive(buf);
            data = Encoding.UTF8.GetString(buf, 0, bytesRec);
            x = Int32.Parse(data);

            msg = Encoding.UTF8.GetBytes(x.ToString());
            bytesSent = _sender.Send(msg);

            Console.Write(x);
        }
        
        private static void Connect(IPEndPoint remoteEp)
        {
            while (true)
                try
                {
                    _sender.Connect(remoteEp);
                    break;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
        }
    }
}