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
            ReceiveConnection = new TcpClient();
        }

        private TcpClient ReceiveConnection { get; set; }

        public IEnumerable<string> GetChatHistory(IPAddress source)
        {
            try
            {
                var returnList = new List<string>();

                ReceiveConnection.Connect(new IPEndPoint(source, DefaultValues.TcpChatHistoryPort));

                var connectionStream = ReceiveConnection.GetStream();
                var reader = new StreamReader(connectionStream);
                while (true)
                {
                    var entry = reader.ReadLine();

                    if (!string.IsNullOrEmpty(entry))
                    {
                        returnList.Add(entry);
                    }
                    else
                        return returnList;
                }

            }
            catch { return null; }
        } 

    }
}
