using Chat.Events;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Messages_Tracer {
    public interface ITcpTracer {
        Task TraceConnectionMessages(TcpClient connection, string username, bool IsLocalConnection);

        event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        event EventHandler<UserDisconnectedEventArgs> OnUserDisconnected;
    }
}
