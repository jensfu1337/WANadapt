using System.Net.Sockets;

namespace Common.Network
{
    public interface IStateObject
    {
        int Id { get; }
        byte[] Buffer { get; }
        Socket Listener { get; }
        string Text { get; }
        byte[] Data { get; }
        void Append(int size);
        void Reset();
    }
}
