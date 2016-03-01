using System;
using Common;
using System.Net;
using System.Collections.Generic;

namespace Client
{
    class MainClass
    {
        private static AsyncClient client;
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

                if (message.Length > 0)
                {
                    client.Send(message, false);
                    client.Receive();
                }

            }
        }

        private static void InitClient()
        {
            // Create client object and set eventhandler
            client = new AsyncClient();
            client.Connected += new ConnectedHandler(ConnectedToServer);
            client.MessageReceived += new ClientMessageReceivedHandler(ServerMessageReceived);
            client.MessageSubmitted += new ClientMessageSubmittedHandler(ClientMessageSubmitted);
        }

        private static void StartClient(string ipAddress)
        {
            var connectIP = IPAddress.Parse(ipAddress);
            var endPoint = new IPEndPoint(connectIP, client.Port);

            client.StartClient(endPoint);
        }

        private static void ConnectedToServer(IAsyncClient a, IPEndPoint e)
        {
            Console.WriteLine("\nConnected to {0}:{1}...\n\n", e.Address, e.Port);
        }

        private static void ServerMessageReceived(IAsyncClient a, String msg)
        {
            Console.WriteLine("\nServer > {0} ", msg);
        }

        private static void ClientMessageSubmitted(IAsyncClient a, bool close)
        {
            if (close)
                a.Dispose();
        }
    }
}
