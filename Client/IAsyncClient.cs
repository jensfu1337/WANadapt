﻿using System;
using System.Net;

namespace Client
{
    public interface IAsyncClient : IDisposable
    {
        event ConnectedHandler Connected;
        event DisconnectedHandler Disconnected;
        event ClientMessageReceivedHandler MessageReceived;
        event ClientMessageSubmittedHandler MessageSubmitted;

        void StartClient(IPEndPoint endpoint);
        void Receive();
        void Send(string msg);
    }
}
