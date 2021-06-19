using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Client
{
    internal class Program
    {
        public static void StartClient(string address, int port, string message)
        {
            try
            {
                var ipAddress = address == "localhost" ? IPAddress.Loopback : IPAddress.Parse(address);
                var remoteEP = new IPEndPoint(ipAddress, port);

                // CREATE
                var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // CONNECT
                    sender.Connect(remoteEP);

                    // Подготовка данных к отправке
                    var msgBytes = Encoding.UTF8.GetBytes(message);
                    
                    // SEND
                    sender.Send(BitConverter.GetBytes(msgBytes.Length).Concat(msgBytes).ToArray());

                    // RECEIVE
                    var lenBuf = new byte[sizeof(int)];
                    sender.Receive(lenBuf);
                    var buf = new byte[BitConverter.ToInt32(lenBuf)];
                    var data = Encoding.UTF8.GetString(buf, 0, sender.Receive(buf));
                    
                    var history = JsonSerializer.Deserialize<List<string>>(data);
                    foreach (var msg in history) Console.WriteLine(msg);

                    // RELEASE
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e);
                }
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void SendMsg(Socket socket, string msg)
        {
            var data = Encoding.UTF8.GetBytes(msg);
            socket.Send(BitConverter.GetBytes(data.Length).Concat(data).ToArray());
        }

        private static void Main(string[] args)
        {
            StartClient(args[0], int.Parse(args[1]), args[2]);
        }
    }
}