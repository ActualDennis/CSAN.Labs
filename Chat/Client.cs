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
            connectionManager.Start();
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

        public void SendMessage(string message)
        {
            connectionManager.SendMessage(message);
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