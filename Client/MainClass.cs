using System;
using Common;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Client
{
    class MainClass
    {
        private static SimpleClient client;
        private static readonly ManualResetEvent messageReceived = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            Utils.SetConsoleTitle("Client");

            List<IPAddress> localIPs = NetUtils.GetLocalClassDRangeIPs();
            var sepp = "\n\t- ";
            string locIPsOutput = sepp + string.Join<IPAddress>(sepp, localIPs);

            Console.Write("List with available local IPs:");
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

                hostFound = NetUtils.IsAlive(message);     
            }
            while (!hostFound);

            // Initialize and start client
            InitClient(message);
            StartClient();
            message = string.Empty;
            Console.Write("Client > ");

            while (message.ToUpper() != "q" && client.IsConnected)
            {
                if (message.Length > 0)
                {
                    client.Send(message);
                    client.Receive();
                }

                message = string.Empty;
                message = Console.ReadLine();
            }

            Console.WriteLine("Howdy! ;)");
            Console.Read();
        }

        private static void InitClient(string msg)
        {
            var remoteIP = IPAddress.Parse(msg);
            // Create client object and set eventhandler
            client = new SimpleClient(remoteIP);
            client.Connected += new ConnectedHandler(ConnectedToServer);
            client.Disconnected += new DisconnectedHandler(DisconnectedFromServer);
            client.MessageReceived += new ClientMessageReceivedHandler(ServerMessageReceived);
            client.MessageSubmitted += new ClientMessageSubmittedHandler(ClientMessageSubmitted);
        }

        private static void StartClient()
        {
            client.Connect();
        }

        private static void ConnectedToServer(AsyncClientBase a, IPEndPoint e)
        {
            Console.WriteLine("\nConnected to {0}:{1}...\n\n", e.Address, e.Port);
        }

        private static void DisconnectedFromServer(AsyncClientBase a, IPEndPoint e)
        {
            Console.WriteLine("Disconnected from Server...");
        }

        private static void ServerMessageReceived(AsyncClientBase a, String msg)
        {
            Console.WriteLine("\nServer > {0} ", msg);
            Console.Write("Client > ");
        }

        private static void ClientMessageSubmitted(AsyncClientBase a)
        {

        }
    }
}
