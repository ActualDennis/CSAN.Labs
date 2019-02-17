using System;
using System.Threading.Tasks;

namespace Chat.Connections {
    public interface IPtpConnectionManager {
        Task Start();

        void SendMessage(string message);

        event EventHandler<LogEventArgs> OnLocalEventHappened;

        event EventHandler<LogEventArgs> OnEventHappened;
    }
}