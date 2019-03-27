using Chat.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chat.Connections {
    public interface IPtpConnectionManager {
        Task Start();

        Task SendMessage(string message);

        event EventHandler<LogEventArgs> OnLocalEventHappened;

        event EventHandler<LogEventArgs> OnEventHappened;

        event EventHandler<ChatHistoryUpdatedEventArgs> OnChatHistoryUpdated;
    }
}