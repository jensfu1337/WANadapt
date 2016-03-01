using System;
using Common;
using System.Net;
using System.Collections.Generic;
using System.Threading;

namespace Client
{
    class MainClass
    {
        private static AsyncClient client;
        private static ManualResetEvent messageReceived;
        private static bool isConnected;

        public static void Main(string[] args)
        {
            Utils.SetConsoleTitle("Client");
            
            List<IPAddress> localIPs = IPHelper.GetLocalClassDRangeIPs();
            string locIPsOutput = String.Join<IPAddress>("\n", localIPs);

            Console.WriteLine("List with available local IPs:");
            Console.WriteLine(locIPsOutput);
            Console.WriteLine("\n\nEnter IPv4 address. Leave empty to use local IPv4.\n");

            // Used to find server and process messages
            string message;
            bool hostFound = false;

            do
            {
                message = string.Empty;
                Console.Write("> ");
                message = Console.ReadLine();

                // Get local IPv4 if message is empty
                if (message.Length == 0)
                    message = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString(); 

                hostFound = IPHelper.IsAlive(message);     
            }
            while (!hostFound);

            // Initialize and start client
            InitClient();
            StartClient(message);
            
            while (message.ToUpper() != "q")
            {
                message = string.Empty;
                Console.Write("Client > ");
                message = Console.ReadLine();

                if (!isConnected)
                    break;

                if (message.Length > 0)
                {
                    client.Send(message, false);
                    client.Receive();
                    messageReceived.WaitOne();
                }                
            }
            Console.Read();
        }

        private static void InitClient()
        {
            // Create client object and set eventhandler
            client = new AsyncClient();
            client.Connected += new ConnectedHandler(ConnectedToServer);
            client.Disconnected += new DisconnectedHandler(DisconnectedFromServer);
            client.MessageReceived += new ClientMessageReceivedHandler(ServerMessageReceived);
            client.MessageSubmitted += new ClientMessageSubmittedHandler(ClientMessageSubmitted);

            messageReceived = new ManualResetEvent(false);
            isConnected = false;
        }

        private static void StartClient(string ipAddress)
        {
            var connectIP = IPAddress.Parse(ipAddress);
            var endPoint = new IPEndPoint(connectIP, client.Port);

            client.StartClient(endPoint);
        }

        private static void ConnectedToServer(IAsyncClient a, IPEndPoint e)
        {
            isConnected = true;
            Console.WriteLine("\nConnected to {0}:{1}...\n\n", e.Address, e.Port);
        }

        private static void DisconnectedFromServer(IAsyncClient a, IPEndPoint e)
        {
            isConnected = false;
            Console.WriteLine("Disconnected from Server...");
        }

        private static void ServerMessageReceived(IAsyncClient a, String msg)
        {
            Console.WriteLine("\nServer > {0} ", msg);
            messageReceived.Set();
        }

        private static void ClientMessageSubmitted(IAsyncClient a, bool close)
        {
            if (close)
                a.Dispose();
        }
    }
}
