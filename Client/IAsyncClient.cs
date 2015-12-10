using System;
using System.Net;

namespace Client
{
    public interface IAsyncClient : IDisposable
    {
        event ConnectedHandler Connected;
        event ClientMessageReceivedHandler MessageReceived;
        event ClientMessageSubmittedHandler MessageSubmitted;

        void StartClient(IPEndPoint endpoint);
        bool IsConnected();
        void Receive();
        void Send(string msg, bool close);
    }
}
