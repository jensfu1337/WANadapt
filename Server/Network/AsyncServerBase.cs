using System;
using Common.Network;
using System.Threading;
using System.Collections.Generic;

namespace Server
{ 
    public delegate void MessageReceivedHandler(int id, string msg);
    public delegate void MessageSubmittedHandler(int id);

    public abstract class AsyncServerBase : IDisposable
    {
        #region Events
        public event MessageReceivedHandler MessageReceived;
        public event MessageSubmittedHandler MessageSubmitted;

        protected readonly ManualResetEvent mreConnected = new ManualResetEvent(false);
        #endregion Events

        #region Fields
        protected ushort _port = Common.Network.Constants.DefaulPort;
        protected byte _maxClients = Common.Network.Constants.MaxClients;
        protected static AsyncServerBase _instance;
        protected readonly IDictionary<int, IStateObject> _clients = new Dictionary<int, IStateObject>();
        #endregion Fields

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

        public byte MaxClients
        {
            get { return this._maxClients; }
            protected set
            {
                if (value > 0)
                    this._maxClients = value;
                else
                    throw new Exception("Exception: Invalid number of maximal clients\nMax clients allowed: " + ushort.MaxValue);
            }
        }
        #endregion Properties

        protected AsyncServerBase() { }

        protected void RaiseMessageReceived(int id, string message)
        {
            var messageReceived = this.MessageReceived;
            if (messageReceived != null)
            {
                messageReceived(id, message);
            }
        }
        protected void RaiseMessageSubmitted(int id)
        {
            var messageSubmitted = this.MessageSubmitted;
            if (messageSubmitted != null)
            {
                messageSubmitted(id);
            }
        }


        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        protected abstract void OnMessageReceive(int id, string msg);
        protected abstract void OnMessageSubmitted(int id);

        public abstract void StartListening();
        public abstract bool IsConnected(int id);
        protected abstract void OnClientConnect(IAsyncResult result);
        protected abstract void ReceiveCallback(IAsyncResult result);
        public abstract void Send(int id, string msg);
        public abstract void Close(int id);
    }
}
