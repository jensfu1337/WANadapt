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

            SimpleServer.Instance.MessageReceived += new MessageReceivedHandler (ClientMessageReceived);
            SimpleServer.Instance.MessageSubmitted += new MessageSubmittedHandler(ServerMessageSubmitted);

            Thread thread = new Thread(new ThreadStart(SimpleServer.Instance.StartListening));
            thread.IsBackground = true;
            thread.Start();

            Console.WriteLine("Server started. Press any key to stop\n\n");
            
            Console.Read();
        }

        private static void ClientMessageReceived(int id, String msg)
        {
            SimpleServer.Instance.Send(id, msg.Replace("client", "server"));
            Console.WriteLine("Client {0} > {1}", id, msg);
        }

        private static void ServerMessageSubmitted(int id)
        {
            //if (close)
            //{
            //    SimpleServer.Instance.Close(id);
            //}
        }
    }
}
