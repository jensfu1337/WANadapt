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
    public delegate void ClientMessageSubmittedHandler(IAsyncClient a);

    public sealed class AsyncClient : IAsyncClient
    {
        private const ushort _port = 8889;
        private Socket listener;
        private IPEndPoint endpoint;
        private bool isConnected = false;

        public ushort Port
        {
            get
            {
                return _port;
            }
        }

        public bool IsConnected
        {
            get
            {
                // isConnected = !(this.listener.Poll(1000, SelectMode.SelectRead) && this.listener.Available == 0);
                return isConnected;
            }
        }

        private readonly ManualResetEvent connected = new ManualResetEvent(false);
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
                this.connected.WaitOne();

                isConnected = true;
                var connectedHandler = this.Connected;
                if (connectedHandler != null)
                {
                    connectedHandler(this, endpoint);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        private void OnConnectCallback(IAsyncResult result)
        {
            try
            {
                var server = (Socket)result.AsyncState;
                Console.WriteLine("Connecting to {0}", endpoint.Address);

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

        #region Receive
        public void Receive()
        {
            try
            {
                var state = new StateObject(this.listener);
                state.Listener.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                    Console.WriteLine("Still Connected, but the Send would block");
                else
                {
                    Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    this.RaiseDisconnected();
                }
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                var state = (IStateObject)result.AsyncState;
                int receive = state.Listener.EndReceive(result);

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
                    var messageReceivedHandler = this.MessageReceived;
                    if (messageReceivedHandler != null)
                    {
                        messageReceivedHandler(this, state.Text);
                    }

                    state.Reset();
                    this.received.Set();
                }
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                    Console.WriteLine("Still Connected, but the Send would block");
                else
                {
                    Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    this.RaiseDisconnected();
                }
            }
        }
        #endregion

        #region Send
        public void Send(string msg)
        {
            byte[] response = Encoding.UTF8.GetBytes(msg);

            try
            {
                this.listener.BeginSend(response, 0, response.Length, SocketFlags.None, this.SendCallback, this.listener);
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                    Console.WriteLine("Still Connected, but the Send would block");
                else
                {
                    // Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    this.RaiseDisconnected();
                }
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            try
            {
                var resceiver = (Socket)result.AsyncState;
                resceiver.EndSend(result);

                var messageSubmittedHandler = this.MessageSubmitted;
                if (messageSubmittedHandler != null)
                { 
                    messageSubmittedHandler(this);
                }
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                    Console.WriteLine("Still Connected, but the Send would block");
                else
                {
                    // Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    this.RaiseDisconnected();
                }
            }
        }
        #endregion

        private void Close()
        {
            try
            {
                this.listener.Shutdown(SocketShutdown.Both);
                this.listener.Close();
            }
            catch (SocketException)
            {
                // TODO:
            }
            finally
            {
                this.RaiseDisconnected();
            }
        }

        private void RaiseDisconnected()
        {
            if (!isConnected)
                return;

            var disconnectedHandler = this.Disconnected;
            if (disconnectedHandler != null)
            {
                disconnectedHandler(this, endpoint);
            }

            isConnected = false;
        }

        public void Dispose()
        {
            this.connected.Dispose();
            this.received.Dispose();
            this.Close();
        }
    }
}
