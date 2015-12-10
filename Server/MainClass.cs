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
/// 
namespace Server
{
    public static class MainClass
    {
        public static void Main(string[] args)
        {
            Utils.SetTitle("Server");

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
