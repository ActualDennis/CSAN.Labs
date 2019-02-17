using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat.Chat_history {
    public interface IChatHistorySender {
        void StartListening();

        void NewEntry(string entry);
    }
}
