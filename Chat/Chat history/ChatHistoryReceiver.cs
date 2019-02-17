using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat.Chat_history {
    public class ChatHistoryReceiver : IChatHistoryReceiver {
        public ChatHistoryReceiver()
        {
            chatHistoryConnection = new TcpClient();
        }

        private TcpClient chatHistoryConnection { get; set; }

        public IEnumerable<string> GetChatHistory(IPAddress source)
        {
            try
            {
                var returnList = new List<string>();

                chatHistoryConnection.Connect(new IPEndPoint(source, DefaultValues.TcpChatHistoryPort));

                var connectionStream = chatHistoryConnection.GetStream();
                var reader = new StreamReader(connectionStream);
                while (connectionStream.DataAvailable)
                {
                    var entry = reader.ReadLine();
                    if (!string.IsNullOrEmpty(entry))
                    {
                        returnList.Add(entry);
                    }
                }

                return returnList;
            }
            catch { return null; }
        } 

    }
}
