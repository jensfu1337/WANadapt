using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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

        protected readonly ManualResetEvent mreConnected = new ManualResetEvent(false);
        protected readonly ManualResetEvent mreReceived = new ManualResetEvent(false);

        public ushort Port
        {
            get { return this.Port; }         
            protected set
            {
                if (NetUtils.IsPortValid(value))
                    this.Port = value;
            }
        }
        protected Socket Listener { get; set; }
        protected IPEndPoint Endpoint { get; set; }
        public bool IsConnected { get; protected set; }

        public abstract void Connect();
        public abstract void Receive();
        public abstract void Send(string msg);

        protected AsyncClientBase()
        {
            this.IsConnected = false;
        }

        #region Raise events
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
        #endregion Raise events

        protected abstract void OnConnected();
        protected abstract void OnDisconnected();
        protected abstract void OnMessageReceived(string message);
        protected abstract void OnMessageSubmitted();

        public void Close()
        {
            try
            {
                if (this.IsConnected)
                {
                    this.Listener.Shutdown(SocketShutdown.Both);
                    this.Listener.Close();
                }
            }
            catch (SocketException)
            {
                // tbd
            }
            finally
            {
                this.IsConnected = false;
            }
        }

        public void Dispose()
        {
            this.mreConnected.Dispose();
            this.mreReceived.Dispose();
            this.Close();
        }
    }
}
