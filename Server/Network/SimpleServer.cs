using Common;
using Common.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public sealed class SimpleServer : AsyncServerBase
    {   
        // Get instance (Singleton)     
        public static AsyncServerBase Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new SimpleServer();

                return _instance;
            }
        }
        
        public override void StartListening()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, Port);

            try
            {
                using (var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    listener.Bind(endpoint);
                    listener.Listen(this.MaxClients);

                    while (true)
                    {
                        this.mreConnected.Reset();
                        listener.BeginAccept(this.OnClientConnect, listener);
                        this.mreConnected.WaitOne();
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /* Gets a socket from the clients dictionary by his Id. */
        private IStateObject GetClient(int id)
        {
            IStateObject state;

            return this._clients.TryGetValue(id, out state) ? state : null;
        }

        /* Checks if the socket is connected. */
        public override bool IsConnected(int id)
        {
            IStateObject state = this.GetClient(id);

            return !(state.Listener.Poll(1000, SelectMode.SelectRead) && state.Listener.Available == 0);
        }

        #region Receive data
        protected override void OnClientConnect(IAsyncResult result)
        {
            this.mreConnected.Set();

            try
            {
                IStateObject state;

                lock (this._clients)
                {
                    var id = !this._clients.Any() ? 1 : this._clients.Keys.Max() + 1;

                    state = new StateObject(((Socket)result.AsyncState).EndAccept(result), id);
                    this._clients.Add(id, state);
                    Console.WriteLine("Client connected.\n-->IP: {0}\n-->ID: {1}\n", state.Listener.LocalEndPoint, id);
                }

                state.Listener.BeginReceive(state.Buffer, 0, Constants.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
            }
            catch (SocketException)
            {
                // TODO:
            }
        }

        protected override void ReceiveCallback(IAsyncResult result)
        {
            var state = (IStateObject)result.AsyncState;

            try
            {
                if (!this.IsConnected(state.Id))
                {
                    Close(state.Id);
                    return;
                }

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
                    this.OnMessageReceive(state.Id, state.Text);

                    state.Reset();

                    state.Listener.BeginReceive(state.Buffer, 0, Constants.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }
        #endregion
        
        #region Send data
        public override void Send(int id, string msg)
        {
            var state = this.GetClient(id);

            if (state == null)
            {
                throw new Exception("Client does not exist.");
            }

            if (!this.IsConnected(state.Id))
            {
                throw new Exception("Destination socket is not connected.");
            }

            try
            {
                var send = Common.Compression.Compressor.CompressBytes(Encoding.UTF8.GetBytes(msg));
                
                state.Listener.BeginSend(send, 0, send.Length, SocketFlags.None, this.SendCallback, state);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine(ae.Message);
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            var state = (IStateObject)result.AsyncState;

            try
            {
                state.Listener.EndSend(result);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
            catch (ObjectDisposedException ode)
            {
                Console.WriteLine(ode.Message);
            }
            finally
            {
                this.OnMessageSubmitted(state.Id);
            }
        }
        #endregion

        public override void Close(int id)
        {
            var state = (IStateObject)this.GetClient(id);

            if (state == null)
            {
                Console.WriteLine("Client {0} does not exist.", state.Id);
            }

            try
            {
                state.Listener.Shutdown(SocketShutdown.Both);
                state.Listener.Close();
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
            finally
            {
                lock (this._clients)
                {
                    this._clients.Remove(state.Id);
                    Console.WriteLine("Client disconnected with Id {0}", state.Id);
                }
            }
        }

        public override void Dispose()
        {
            foreach (var id in this._clients.Keys)
            {
                this.Close(id);
            }

            this.mreConnected.Dispose();
        }

        protected override void OnMessageReceive(int id, string msg)
        {
            this.RaiseMessageReceived(id, msg);
        }

        protected override void OnMessageSubmitted(int id)
        {
            this.RaiseMessageSubmitted(id);
        }
    }
}
