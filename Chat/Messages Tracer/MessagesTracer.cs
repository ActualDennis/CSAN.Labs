using Chat.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Messages_Tracer {
    public class MessagesTracer : ITcpTracer {

        public MessagesTracer()
        {

        }

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        public event EventHandler<UserDisconnectedEventArgs> OnUserDisconnected;

        public async Task TraceConnectionMessages(TcpClient connection, string username, bool IsLocalConnection)
        {
            if (IsLocalConnection)
                return;

            var address = connection.Client.GetRemoteEndPointIpAddress(false);

            try
            {
                var connectionStream = connection.GetStream();
                var reader = new StreamReader(connectionStream);
                while (connection.Connected)
                {
                    // this blocks until a message is received.
                    var message = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(message))
                        return;

                    var normalizedMessage = GetNormalizedMessage(username, message);

                    OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs()
                    {
                        normalizedMessage = normalizedMessage
                    });
                }
            }
            finally
            {
                OnUserDisconnected.Invoke(this, new UserDisconnectedEventArgs()
                {
                    username = username,
                    address = address
                });
            }
        }

        private string GetNormalizedMessage(string username, string message)
        {
            return $"{DateTime.Now.ToString("HH:MM")} " +
                            $"{username} : " +
                            $"{message}";
        }
    }
}
