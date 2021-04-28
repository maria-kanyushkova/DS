using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server
{
    class Program
    {
        private static List<string> _history = new List<string>();
        public static void StartListening(int port)
        {
            // Привязываем сокет ко всем интерфейсам на текущей машинe
            var ipAddress = IPAddress.Any;

            var localEndPoint = new IPEndPoint(ipAddress, port);

            // CREATE
            var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // BIND
                listener.Bind(localEndPoint);

                // LISTEN
                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Ожидание соединения клиента...");
                    // ACCEPT
                    var handler = listener.Accept();
                    
                    Console.WriteLine("Получение данных...");
                    var buf = new byte[1024];

                    // RECEIVE
                    var bytesRec = handler.Receive(buf);
                    var data = Encoding.UTF8.GetString(buf, 0, bytesRec);

                    _history.Add(data);
                    Console.WriteLine("Полученный текст: {0}", data);

                    // Отправляем текст обратно клиенту
                    var historyJson = JsonSerializer.Serialize(_history);
                    var msg = Encoding.UTF8.GetBytes(historyJson);

                    // SEND
                    handler.Send(msg);

                    // RELEASE
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

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
        static void Main(string[] args)
        {
            Console.WriteLine("Запуск сервера...");
            StartListening(Int32.Parse(args[0]));

            Console.WriteLine("\nНажмите ENTER чтобы выйти...");
            Console.Read();
        }
    }
}
