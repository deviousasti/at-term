using System;

namespace AtTerm
{
    public interface ITTy
    {
        event Action<string> Received;
        event Action<string> Connected;

        event Action<string> Disconnected;
        void Send(string text);
        bool Connect();
        bool Disconnect();
        bool IsConnected { get; }
    }
}