using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public delegate void ConnectedHandler(IAsyncClient a, IPEndPoint e);
    public delegate void DisconnectedHandler(IAsyncClient a, IPEndPoint e);
    public delegate void ClientMessageReceivedHandler(IAsyncClient a, string msg);
    public delegate void ClientMessageSubmittedHandler(IAsyncClient a, bool close);

    public sealed class AsyncClient : IAsyncClient
    {
        private const ushort _port = 8889;
        private Socket listener;
        private bool close;
        private IPEndPoint endpoint;

        public ushort Port
        {
            get
            {
                return _port;
            }
        }

        private readonly ManualResetEvent connected = new ManualResetEvent(false);
        private readonly ManualResetEvent sent = new ManualResetEvent(false);
        private readonly ManualResetEvent received = new ManualResetEvent(false);

        public event ConnectedHandler Connected;
        public event DisconnectedHandler Disconnected;
        public event ClientMessageReceivedHandler MessageReceived;
        public event ClientMessageSubmittedHandler MessageSubmitted;

        public void StartClient(IPEndPoint _endpoint)
        {
            endpoint = _endpoint;

            try
            {
                this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.listener.BeginConnect(endpoint, this.OnConnectCallback, this.listener);
                Console.WriteLine("Waiting for connection...");
                this.connected.WaitOne();

                var connectedHandler = this.Connected;

                if (connectedHandler != null)
                {
                    Console.WriteLine("Connection found!");
                    connectedHandler(this, endpoint);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        private bool IsConnected()
        {
            return !(this.listener.Poll(1000, SelectMode.SelectRead) && this.listener.Available == 0);
        }

        public bool IsConnectionValid()
        {
            if (!IsConnected())
            {
                if (this.Disconnected != null)
                {
                    Disconnected(this, endpoint);
                    this.Dispose();
                }
                return false;
            }
            return true;
        }

        private void OnConnectCallback(IAsyncResult result)
        {
            var server = (Socket)result.AsyncState;

            try
            {
                server.EndConnect(result);
                this.connected.Set();
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not connect to {0}\nWaiting 1000ms...\n", endpoint.Address);

                Thread.Sleep(1000);
                this.listener.BeginConnect(endpoint, this.OnConnectCallback, this.listener);
            }
        }

        #region Receive data
        public void Receive()
        {
            var state = new StateObject(this.listener);

            if (!this.IsConnectionValid())
                return;
            
            state.Listener.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            var state = (IStateObject)result.AsyncState;
            var receive = state.Listener.EndReceive(result);

            if (receive > 0)
            {
                state.Append(Encoding.UTF8.GetString(state.Buffer, 0, receive));
            }

            if (receive == state.BufferSize)
            {
                state.Listener.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
            }
            else
            {
                var messageReceived = this.MessageReceived;

                if (messageReceived != null)
                {
                    messageReceived(this, state.Text);
                }

                state.Reset();
                this.received.Set();
            }
        }
        #endregion

        #region Send data
        public void Send(string msg, bool close)
        {
            if (!this.IsConnectionValid())
                return;

            var response = Encoding.UTF8.GetBytes(msg);

            this.close = close;
            this.listener.BeginSend(response, 0, response.Length, SocketFlags.None, this.SendCallback, this.listener);
        }

        private void SendCallback(IAsyncResult result)
        {
            try
            {
                var resceiver = (Socket)result.AsyncState;

                resceiver.EndSend(result);
            }
            catch (SocketException)
            {
                // TODO:
            }
            catch (ObjectDisposedException)
            {
                // TODO;
            }

            var messageSubmitted = this.MessageSubmitted;

            if (messageSubmitted != null)
            {
                messageSubmitted(this, this.close);
            }

            this.sent.Set();
        }
        #endregion

        private void RaiseDisconnect()
        {

        }

        private void Close()
        {
            try
            {
                if (!this.IsConnected())
                    return;
                
                this.listener.Shutdown(SocketShutdown.Both);
                this.listener.Close();
            }
            catch (SocketException)
            {
                // TODO:
            }
        }

        public void Dispose()
        {
            this.connected.Dispose();
            this.sent.Dispose();
            this.received.Dispose();
            this.Close();
        }
    }
}
