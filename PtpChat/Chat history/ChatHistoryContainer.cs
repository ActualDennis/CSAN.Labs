using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat.Chat_history {
    public class ChatHistoryContainer : IChatHistoryContainer {
        public ChatHistoryContainer()
        {
            connectionsListener = new TcpListener(IPAddress.Any, DefaultValues.TcpChatHistoryPort);
            chatHistory = new List<string>();
        }

        private List<string> chatHistory;

        public List<string> ChatHistory
        {
            get => chatHistory;
            set
            {
                if (value != null && value.Count != 0)
                    chatHistory = value;
            }
        }

        private TcpListener connectionsListener { get; set; }

        public void NewEntry(string entry)
        {
            ChatHistory.Add(entry);
        }

        /// <summary>
        /// Listens for another client that 
        /// will try to connect to receive chat history entries
        /// </summary>
        public void StartListening()
        {
            connectionsListener.Start();
            while (true)
            {
                var newClient = connectionsListener.AcceptTcpClient();
                Console.WriteLine($"(Local) User {newClient.Client.GetRemoteEndPointIpAddress()} connected. Sending total chathistory of {chatHistory.Count} entries");
                var localStream = new MemoryStream();
                var writer = new StreamWriter(localStream);
                foreach(var entry in ChatHistory)
                {
                    writer.WriteLine(entry);
                }

                writer.Flush();
                localStream.Seek(0, SeekOrigin.Begin);
                localStream.CopyTo(newClient.GetStream());
                newClient.Close();
            }
        }

    }
}
