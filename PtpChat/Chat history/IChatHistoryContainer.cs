using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat.Chat_history {
    public interface IChatHistoryContainer {
        List<string> ChatHistory { get; set; }

        void StartListening();

        void NewEntry(string entry);
    }
}
