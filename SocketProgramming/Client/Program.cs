using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;


namespace Client
{
    internal class Program
    {
        const string EXIT = "exit";
        const int PORT = 8081;

        static CancellationTokenSource cancel = new CancellationTokenSource();

        static string name;
        static string input = "";

        static bool ClientAlive = true;

        static object lockObject = new();

        static void Main(string[] args)
        {
            Console.Write("Enter Nickname : ");
            name = Console.ReadLine();
            if (name == null) name = "Unknown";

            Socket socket = ConnectSocket();

            Thread readThread = new(ReadThread);
            readThread.IsBackground = true;
            readThread.Start(socket);
            while (ClientAlive)
            {
                string isActive = Write(socket);

                if (isActive.IndexOf(EXIT) > -1)
                {
                    ClientAlive = false;
                    Console.WriteLine("Disconnected!");
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }

            Console.ReadKey();
        }
        private static Socket ConnectSocket()
        {
            IPEndPoint endPoint = new(Dns.GetHostEntry(Dns.GetHostName()).AddressList[1], PORT);
            Socket socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine($"Connected on : {endPoint.Port}");
            socket.Connect(endPoint);
            return socket;
        }
        private static void ReadThread(object obj)
        {
            Socket clientSocket = obj as Socket;
            while (ClientAlive)
            {
                byte[] buffer = new byte[2048];
                int receiveByte = clientSocket.Receive(buffer);
                string receiveMessage = Encoding.UTF8.GetString(buffer);

                cancel.Cancel();
                int cursor = Console.CursorTop;
                Console.SetCursorPosition(0, cursor);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, cursor);
                lock (lockObject) Console.WriteLine(receiveMessage);
                lock (lockObject) Console.Write($"{name} : {input}");
            }
        }
        private static string Write(Socket socket)
        {
            try
            {
                bool onRead = true;
                ConsoleKeyInfo key = new();
                Console.Write($"{name} : ");
                input = "";

                Thread readThread = new(new ThreadStart(() =>
                {
                    while (onRead)
                    {
                        key = Console.ReadKey();
                        input += key.KeyChar;
                        if (key.Key == ConsoleKey.Enter) onRead = false;
                    }
                }));
                readThread.IsBackground = true;
                readThread.Start();
                readThread.Join();

                Console.WriteLine();

                byte[] sendMessage = Encoding.UTF8.GetBytes($"{name} : {input}");
                socket.Send(sendMessage);
                if (input.IndexOf(EXIT) > -1) return EXIT;
                return input;
            }
            catch
            {
                return EXIT;
            }
        }
    }
}