using System;
using Common;
using System.Threading;

namespace Server
{
    public static class MainClass
    {
        public static void Main(string[] args)
        {
            Utils.SetConsoleTitle("Server");

            AsyncSocketListener.Instance.MessageReceived += new MessageReceivedHandler (ClientMessageReceived);
            AsyncSocketListener.Instance.MessageSubmitted += new MessageSubmittedHandler(ServerMessageSubmitted);

            Thread thread = new Thread(new ThreadStart(AsyncSocketListener.Instance.StartListening));
            thread.IsBackground = true;
            thread.Start();

            Console.WriteLine("Server started. Press any key to stop\n\n");
            
            Console.Read();
        }

        private static void ClientMessageReceived(int id, String msg)
        {
            AsyncSocketListener.Instance.Send(id, msg.Replace("client", "server"), false);
            Console.WriteLine("Client {0} > {1}", id, msg);
        }

        private static void ServerMessageSubmitted(int id, bool close)
        {
            if (close)
            {
                AsyncSocketListener.Instance.Close(id);
            }
        }
    }
}
