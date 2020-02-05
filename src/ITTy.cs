using System;
using System.ComponentModel;

namespace AtTerm
{
    public interface ITTy : INotifyPropertyChanged
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