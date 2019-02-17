using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat.Chat_history {
    public class ChatHistorySender : IChatHistorySender {
        public ChatHistorySender()
        {
            connectionsListener = new TcpListener(IPAddress.Any, DefaultValues.TcpChatHistoryPort);
            chatHistory = new List<string>();
        }

        private List<string> chatHistory { get; set; }

        private TcpListener connectionsListener { get; set; }

        public void NewEntry(string entry)
        {
            chatHistory.Add(entry);
        }

        public void StartListening()
        {
            connectionsListener.Start();
            while (true)
            {
                var newClient = connectionsListener.AcceptTcpClient();
                var localStream = new MemoryStream();
                var writer = new StreamWriter(localStream);
                foreach(var entry in chatHistory)
                {
                    writer.WriteLine(entry);
                }

                writer.Flush();
                localStream.Seek(0, SeekOrigin.Begin);
                localStream.CopyTo(newClient.GetStream());
            }
        }

    }
}
