using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Client
{
    public sealed class SimpleClient : AsyncClientBase
    {
        private SimpleClient() { }

        public SimpleClient(IPAddress remoteIP, [Optional, DefaultParameterValue((ushort)8889)]ushort port)
        {
            this.Port = port;
            this.Endpoint = new IPEndPoint(remoteIP, this.Port);
        }
        #region Connect
        public override void Connect()
        {
            try
            {
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
                Console.WriteLine("Connecting to {0}", this.Endpoint.Address);

                server.EndConnect(result);
                this.OnConnected();
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not connect to {0}\nWaiting 1000ms...\n", this.Endpoint.Address);

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
                    state.Append(Encoding.UTF8.GetString(state.Buffer, 0, receive));
                }

                if (receive == state.BufferSize)
                {
                    state.Listener.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
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
