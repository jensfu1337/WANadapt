using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public delegate void ConnectedHandler(AsyncClientBase a, IPEndPoint e);
    public delegate void DisconnectedHandler(AsyncClientBase a, IPEndPoint e);
    public delegate void ClientMessageReceivedHandler(AsyncClientBase a, string msg);
    public delegate void ClientMessageSubmittedHandler(AsyncClientBase a);

    public abstract class AsyncClientBase : IDisposable
    {
        public event ConnectedHandler Connected;
        public event DisconnectedHandler Disconnected;
        public event ClientMessageReceivedHandler MessageReceived;
        public event ClientMessageSubmittedHandler MessageSubmitted;

        protected readonly ManualResetEvent connected = new ManualResetEvent(false);
        protected readonly ManualResetEvent received = new ManualResetEvent(false);

        public ushort Port { get; protected set; }
        protected Socket Listener { get; set; }
        protected IPEndPoint Endpoint { get; set; }
        public bool IsConnected { get; protected set; }

        public abstract void StartClient();
        public abstract void Receive();
        public abstract void Send(string msg);

        public AsyncClientBase()
        {
            this.Port = 8889;
            this.IsConnected = false;
        }

        protected void RaiseConnected()
        {
            var connected = this.Connected;
            if (connected != null)
            {
                connected(this, this.Endpoint);
            }
        }
        protected void RaiseDisconnected()
        {
            if (!this.IsConnected)
                return;

            var disconntected = this.Disconnected;
            if(disconntected != null)
            {
                disconntected(this, this.Endpoint);
            }
        }

        protected void RaiseMessageReceived(string message)
        {
            var messageReceived = this.MessageReceived;
            if (messageReceived != null)
            {
                messageReceived(this, message);
            }
        }

        protected void RaiseMessageSubmitted()
        {
            var messageSubmitted = this.MessageSubmitted;
            if (messageSubmitted != null)
            {
                messageSubmitted(this);
            }
        }

        protected abstract void OnConnected();
        protected abstract void OnDisconnected();
        protected abstract void OnMessageReceived();
        protected abstract void OnMessageSubmitted();

        public void Close()
        {
            try
            {
                this.Listener.Shutdown(SocketShutdown.Both);
                this.Listener.Close();
            }
            catch (SocketException)
            {
                // tbd
            }
            finally
            {
                this.RaiseDisconnected();
            }
        }

        public void Dispose() { }
    }
}
