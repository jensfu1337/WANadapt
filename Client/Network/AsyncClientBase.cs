using Common;
using Common.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client.Network
{
    // Delegates used for eventhandling
    public delegate void ConnectedHandler(AsyncClientBase a, IPEndPoint e);
    public delegate void DisconnectedHandler(AsyncClientBase a, IPEndPoint e);
    public delegate void ClientMessageReceivedHandler(AsyncClientBase a, string msg);
    public delegate void ClientMessageSubmittedHandler(AsyncClientBase a);

    public abstract class AsyncClientBase : IDisposable
    {
        #region Events
        public event ConnectedHandler Connected;
        public event DisconnectedHandler Disconnected;
        public event ClientMessageReceivedHandler MessageReceived;
        public event ClientMessageSubmittedHandler MessageSubmitted;
        // ManualResetEvents used for asynchronous methods
        protected readonly ManualResetEvent mreConnected = new ManualResetEvent(false);
        protected readonly ManualResetEvent mreReceived = new ManualResetEvent(false);
        #endregion Events

        protected ushort _port = Constants.DefaulPort;

        #region Properties
        public ushort Port
        {
            get { return this._port; }
            protected set
            {
                if (Common.Network.Utils.IsPortValid(value))
                    this._port = value;
                else
                    throw new Exception("Exception: Invalid port\nOnly port between " + Constants.PortMin + " and " + Constants.PortMax + " allowed.");
            }
        }
        protected Socket Listener { get; set; }
        protected IPEndPoint Endpoint { get; set; }
        public bool IsConnected { get; protected set; } = false;
        #endregion Properties

        protected AsyncClientBase()
        {            

        }

        #region Abstract Methods
        public abstract void Connect();
        public abstract void Receive();
        public abstract void Send(string msg);
        protected abstract void OnConnected();
        protected abstract void OnDisconnected();
        protected abstract void OnMessageReceived(string message);
        protected abstract void OnMessageSubmitted();
        #endregion Abstract Methods

        #region Implemented Methods
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
        
        public virtual void Close()
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

        public virtual void Dispose()
        {
            this.mreConnected.Dispose();
            this.mreReceived.Dispose();
            this.Close();
        }
        #endregion Implemented Methods
    }
}
