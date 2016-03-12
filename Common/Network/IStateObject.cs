﻿using System.Net.Sockets;

namespace Common.Network
{
    public interface IStateObject
    {
        int BufferSize { get; }
        int Id { get; }
        byte[] Buffer { get; }
        Socket Listener { get; }
        string Text { get; }
        void Append(string text);
        void Reset();
    }
}
