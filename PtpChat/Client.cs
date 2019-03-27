using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Chat.Chat_history;
using Chat.Messages_Tracer;
using Chat.Events;
using Chat.Connections;

namespace Chat {
    public class PtpClient {
        public PtpClient(
            ILocalLogger logger,
            IPtpConnectionManager connectionManager)
        {
            this.logger = logger;
            this.connectionManager = connectionManager;
            this.connectionManager.OnEventHappened += ConnectionManager_OnEventHappened;
            this.connectionManager.OnLocalEventHappened += ConnectionManager_OnLocalEventHappened;
            this.connectionManager.OnChatHistoryUpdated += ConnectionManager_OnChatHistoryUpdated;
            connectionManager.Start();
        }

        private void ConnectionManager_OnChatHistoryUpdated(object sender, ChatHistoryUpdatedEventArgs e)
        {
            var history = e.chatHistoryEntries;

            if (history == null)
            {
                logger.LogLocal("Local error happened while receiving the chat history.");
                return;
            }

            Console.WriteLine("<----Chat history start---->");

            foreach (var entry in history)
            {
                logger.Log(entry);
            }
        }

        private string clientName;

        public string ClientName
        {
            get => clientName;
            set
            {
                if (value != null)
                    clientName = value;
            }
        }

        private ILocalLogger logger { get; set; }

        private IPtpConnectionManager connectionManager { get; set; }

        public async Task SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            await connectionManager.SendMessage(message);
        }

        private void ConnectionManager_OnLocalEventHappened(object sender, LogEventArgs e)
        {
            logger.LogLocal(e.message);
        }

        private void ConnectionManager_OnEventHappened(object sender, LogEventArgs e)
        {
            logger.Log(e.message);
        }

    }
}