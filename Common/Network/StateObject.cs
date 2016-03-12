using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Common.Network
{
    public sealed class StateObject : IStateObject
    {
        public StateObject(Socket listener, int id = -1)
        {
            this.Listener = listener;
            this.Id = id;
            this.Reset();
        }

        public int Id { get; private set; }
        public byte[] Buffer { get; private set; } = new byte[Network.Constants.BufferSize];
        public Socket Listener { get; private set; }
        public byte[] Data { get; private set; }

        public string Text
        {
            get
            {
                return Encoding.UTF8.GetString(this.Data);
            }
        }

        public void Append(int size)
        {
            byte[] data = new List<byte>(this.Buffer).GetRange(0, size).ToArray();
            data = Compression.Compressor.DecompressBytes(data);

            if (this.Data != null)
            {
                byte[] newData = new byte[this.Data.Length + data.Length];
                this.Data.CopyTo(newData, 0);
                data.CopyTo(newData, this.Data.Length);
            }
            else
            {
                this.Data = new byte[data.Length];
                Array.Copy(data, this.Data, data.Length);
            }
        }

        public void Reset()
        {
            this.Data = null;
            this.Buffer = new byte[Network.Constants.BufferSize];
        }
    }
}
