using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

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

            AsyncSocketListener.Instance.StartListening();
        }
    }
}
