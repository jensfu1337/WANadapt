using Common.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client.Network
{
    public sealed class SimpleClient : AsyncClientBase
    {
        private bool reconnect = false;
        private SimpleClient() { }

        public SimpleClient(IPAddress remoteIP)
        {            
            this.Endpoint = new IPEndPoint(remoteIP, this.Port);
        }

        public SimpleClient(IPAddress remoteIP, ushort port) : this(remoteIP)
        {
            this.Port = port;
        }

        #region Connect
        public override void Connect()
        {
            try
            {
                Console.WriteLine("Connecting to {0}", this.Endpoint.Address);

                this.Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.Listener.BeginConnect(this.Endpoint, this.OnConnectCallback, this.Listener);
                this.mreConnected.WaitOne();
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
                server.EndConnect(result);

                this.OnConnected();
                this.reconnect = false;
            }
            catch (SocketException)
            {
                if (!this.reconnect)
                {
                    Console.Write("\nCould not connect to {0}\nWaiting 1s to reconnect...", this.Endpoint.Address);
                    this.reconnect = true;
                }
                else
                {
                    Console.Write(".");
                }

                Thread.Sleep(1000);
                this.Listener.BeginConnect(this.Endpoint, this.OnConnectCallback, this.Listener);
            }
        }
        #endregion Connect

        #region Receive
        public override void Receive()
        {
            try
            {
                var state = new StateObject(this.Listener);
                state.Listener.BeginReceive(state.Buffer, 0, Constants.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                    Console.WriteLine("Still Connected, but the Send would block");
                else
                {
                    Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    this.OnDisconnected();
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
                    state.Append(receive);
                }

                if (receive == Constants.BufferSize)
                {
                    state.Listener.BeginReceive(state.Buffer, 0, Constants.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
                }
                else
                {
                    this.OnMessageReceived(state.Text);
                    state.Reset();
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
                    this.OnDisconnected();
                }
            }
        }
        #endregion

        #region Send
        public override void Send(string msg)
        {
            byte[] response = Common.Compression.Compressor.CompressBytes(Encoding.UTF8.GetBytes(msg));

            try
            {
                this.Listener.BeginSend(response, 0, response.Length, SocketFlags.None, this.SendCallback, this.Listener);
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                    Console.WriteLine("Still Connected, but the Send would block");
                else
                {
                    // Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    this.OnDisconnected();
                }
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            try
            {
                var resceiver = (Socket)result.AsyncState;
                resceiver.EndSend(result);

                this.OnMessageSubmitted();
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                    Console.WriteLine("Still Connected, but the Send would block");
                else
                {
                    // Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    this.OnDisconnected();
                }
            }
        }
        #endregion

        protected override void OnConnected()
        {
            this.mreConnected.Set();
            this.IsConnected = true;

            if (!this.IsConnected)
                this.RaiseConnected();           
        }

        protected override void OnDisconnected()
        {
            this.Close();
            this.IsConnected = false;

            if (!this.IsConnected)
                this.RaiseDisconnected();           
        }

        protected override void OnMessageReceived(string msg)
        {
            this.mreReceived.Set();
            this.RaiseMessageReceived(msg);
        }

        protected override void OnMessageSubmitted()
        {
            this.RaiseMessageSubmitted();
        }
    }
}
