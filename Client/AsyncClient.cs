using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public sealed class AsyncClient : AsyncClientBase
    {
        private AsyncClient() { }
        public AsyncClient(IPAddress remoteIP): this()
        {
            this.Endpoint = new IPEndPoint(remoteIP, this.Port);
        }
        public override void StartClient()
        {
            try
            {
                this.Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.Listener.BeginConnect(this.Endpoint, this.OnConnectCallback, this.Listener);
                this.connected.WaitOne();

                IsConnected = true;
                this.RaiseConnected();
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
                Console.WriteLine("Connecting to {0}", this.Endpoint.Address);

                server.EndConnect(result);
                this.connected.Set();
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not connect to {0}\nWaiting 1000ms...\n", this.Endpoint.Address);

                Thread.Sleep(1000);
                this.Listener.BeginConnect(this.Endpoint, this.OnConnectCallback, this.Listener);
            }
        }

        #region Receive
        public override void Receive()
        {
            try
            {
                var state = new StateObject(this.Listener);
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
                    this.RaiseMessageReceived(state.Text);

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
        public override void Send(string msg)
        {
            byte[] response = Encoding.UTF8.GetBytes(msg);

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

                this.RaiseMessageSubmitted();
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

        public new void Dispose()
        {
            this.connected.Dispose();
            this.received.Dispose();
            this.Close();
        }

        protected override void OnConnected()
        {
            throw new NotImplementedException();
        }

        protected override void OnDisconnected()
        {
            throw new NotImplementedException();
        }

        protected override void OnMessageReceived()
        {
            throw new NotImplementedException();
        }

        protected override void OnMessageSubmitted()
        {
            throw new NotImplementedException();
        }
    }
}
