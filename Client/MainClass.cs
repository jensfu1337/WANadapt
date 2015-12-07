using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Threading;

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

            Thread thread = new Thread(new ThreadStart(client.StartClient));
            thread.Name = "Herr Orhan";
            thread.Start();

            string message = string.Empty;

            while (message.ToUpper() != "q")
            {
                Console.WriteLine("Write to server: ");
                message = Console.ReadLine();

                if (message.Length > 0)
                {
                    client.Send(message, false);
                    client.Receive();
                }
            }
        }

        private static void ConnectedToServer(IAsyncClient a)
        {
            a.Send("Hello, I'm the client.", false);
            a.Receive();
        }

        private static void ServerMessageReceived(IAsyncClient a, String msg)
        {
            Console.WriteLine("Client get Message from server. {0} ", msg);
        }

        private static void ClientMessageSubmitted(IAsyncClient a, bool close)
        {
            if (close)
                a.Dispose();
        }
    }
}
