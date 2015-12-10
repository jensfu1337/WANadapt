using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

/// <summary>
/// Source: http://codereview.stackexchange.com/questions/24758/tcp-async-socket-server-client-communication
/// </summary>

namespace Client
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Utils.SetTitle("Client");

            AsyncClient client = new AsyncClient();
            client.Connected += new ConnectedHandler(ConnectedToServer);
            client.MessageReceived += new ClientMessageReceivedHandler(ServerMessageReceived);
            client.MessageSubmitted += new ClientMessageSubmittedHandler(ClientMessageSubmitted);

            string message;
            IPAddress ip;
            IPHostEntry host;
            Ping ping = new Ping();
            PingReply reply;
            bool hostFound = false;

            Console.WriteLine("Enter IPv4 address. Leave empty to use local IPv4.\n");

            do
            {
                message = string.Empty;
                ip = null;
                host = null;
                reply = null;

                Console.Write("> ");
                message = Console.ReadLine();

                if (message.Length == 0)
                {
                    host = Dns.GetHostEntry(Dns.GetHostName());
                    ip = host.AddressList[1];
                }
                else
                {
                    try
                    {
                        ip = IPAddress.Parse(message);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid IP address entered...");
                        continue;
                    }
                }
                                
                reply = ping.Send(ip, 3000);
                hostFound = (reply.Status == IPStatus.Success);
            }
            while (!hostFound);

            message = String.Empty;
            client.StartClient(new IPEndPoint(ip, client.Port));

            
            while (message.ToUpper() != "q")
            {

                Console.Write("Client > ");
                message = Console.ReadLine();

                if (message.Length > 0)
                {
                    client.Send(message, false);
                    client.Receive();
                }

            }
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
