using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Events {
    public class ChatHistoryUpdatedEventArgs {
        public List<string> chatHistoryEntries;
    }
}
