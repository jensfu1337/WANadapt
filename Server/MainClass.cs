﻿using System;
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

            AsyncSocketListener.Instance.StartListening();
        }

        private static void ClientMessageReceived(int id, String msg)
        {
            AsyncSocketListener.Instance.Send(id, msg.Replace("client", "server"), false);
            Console.WriteLine("Server get Message from client. {0} ", msg);
        }

        private static void ServerMessageSubmitted(int id, bool close)
        {
            if (close)
                AsyncSocketListener.Instance.Close(id);
        }
    }
}
