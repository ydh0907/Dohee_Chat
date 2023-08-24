using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    internal class Program
    {
        static bool ServerAlive;
        static List<Socket> clients = new List<Socket>();
        static object lockObject = new();
        static void Main(string[] args)
        {
            ServerAlive = true;
            Thread thread = new(new ThreadStart(MakeCommunicationThread));
            thread.IsBackground = true;
            thread.Start();

            while (ServerAlive)
            { 
                string input = Console.ReadLine();
                if(input == "close")
                {
                    ServerAlive = false;
                }
            }

            Console.ReadKey();
        }
        private static void MakeCommunicationThread() 
        {
            Socket listenSocket = CreateListenScoket();

            while (ServerAlive)
            {
                Socket clientSocket = listenSocket.Accept();
                Thread thread = new Thread(CommunicationThread);
                thread.IsBackground = true;
                thread.Start(clientSocket);
            }

            listenSocket.Close();
            Console.WriteLine("Server Closed");
        }
        private static void CommunicationThread(object obj)
        {
            Socket clientSocket = obj as Socket;
            lock(lockObject) clients.Add(clientSocket);
            while (ServerAlive)
            {
                bool isSuccess = Communication(clientSocket);

                if (!isSuccess)
                {
                    lock(lockObject) clients.Remove(clientSocket);
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();

                    Console.WriteLine("Disconnected with client");

                    break;
                }
            }
        }

        private static bool Communication(Socket clientSocket)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int receicedSize = clientSocket.Receive(buffer);

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, receicedSize);
                IPEndPoint clientEndPoint = (clientSocket.RemoteEndPoint) as IPEndPoint;
                Console.WriteLine($"Message from {clientEndPoint.Address} : {receivedMessage}");

                string echoMessage = $"{receivedMessage}";
                byte[] echoBytes = Encoding.UTF8.GetBytes(echoMessage);

                for(int i = 0; i < clients.Count; i++)
                {
                    if (clients[i] != clientSocket)
                        lock(lockObject) clients[i].Send(echoBytes);
                }

                if (receivedMessage.IndexOf("exit") > -1) return false;
                return true;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return false;
            }
        }

        private static Socket CreateListenScoket()
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 8081);

            Socket socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endPoint);
            socket.Listen(1);

            Console.WriteLine($"Server opened on port : {endPoint.Port}");

            return socket;
        }
    }
}