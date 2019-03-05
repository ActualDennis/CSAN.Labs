using System.Collections.Generic;
using System.Net;

namespace Chat.Chat_history {
    public interface IChatHistoryReceiver {
        IEnumerable<string> GetChatHistory(IPAddress source);
    }
}