using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Threading;
using System.Net;
using System.Net.Sockets;

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
            IPEndPoint endpoint;
            TcpClient tclient;
            IPHostEntry host;

            Console.WriteLine("Enter IPv4 address. Leave empty to use local IPv4.\n\n");

            do
            {
                message = string.Empty;
                ip = null;
                endpoint = null;
                tclient = null;
                host = null;

                Console.WriteLine("> ");
                message = Console.ReadLine();

                if (message.Length == 0)
                {
                    host = Dns.GetHostEntry(Dns.GetHostName());
                }
                else
                {
                    host = Dns.GetHostEntry(message);
                }

                ip = host.AddressList[1];
                endpoint = new IPEndPoint(ip, client.Port);
                tclient = new TcpClient();

                try
                {
                    tclient.Connect(endpoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } while (!tclient.Connected);

            tclient.Close();

            client.StartClient(endpoint);

            
            while (message.ToUpper() != "q")
            {
                Console.WriteLine("Write to server: ");
                message = Console.ReadLine();

                if (message.Length > 0)
                {
                    client.Send(message, false);
                    client.Receive();
                }
                message = String.Empty;
            }
        }

        private static void ConnectedToServer(IAsyncClient a, IPEndPoint e)
        {
            Console.WriteLine("\nConnected to {0}:{1}...\n\n", e.Address, e.Port);
        }

        private static void ServerMessageReceived(IAsyncClient a, String msg)
        {
            Console.Write("Client get Message from server. {0} ", msg);
        }

        private static void ClientMessageSubmitted(IAsyncClient a, bool close)
        {
            if (close)
                a.Dispose();
        }
    }
}
