using Chat.Chat_history;
using Chat.Events;
using Chat.Messages_Tracer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Connections {
    public class PtpConnectionManager : IPtpConnectionManager {
        public PtpConnectionManager(
            string clientName,
            ITcpTracer messagesTracer,
            IChatHistoryContainer chatHistorySender,
            IChatHistoryReceiver chatHistoryReceiver
            )
        {
            Initialize();
            this.clientName = clientName;
            this.messagesTracer = messagesTracer;
            this.chatHistoryContainer = chatHistorySender;
            this.chatHistoryReceiver = chatHistoryReceiver;
            this.messagesTracer.OnMessageReceived += MessagesTracer_OnMessageReceived;
            this.messagesTracer.OnUserDisconnected += MessagesTracer_OnUserDisconnected;
        }

        private void Initialize()
        {
            userInfoBroadcaster = new UdpClient();
            connectedUserEntries = new List<PtpUserEntry>();
            userInfoReceiver = new UdpClient();
            userInfoReceiver.JoinMulticastGroup(multicastGroupAddress);
            userInfoReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            userInfoReceiver.Client.Bind(new IPEndPoint(IPAddress.Any, DefaultValues.UdpBroadCastPort));

            userInfoBroadcaster.JoinMulticastGroup(multicastGroupAddress);

            try
            {
                messagesListener = new TcpListener(IPAddress.Any, DefaultValues.TcpMessagingPort);
                messagesListener.Start();
            }
            catch
            {
                throw new InvalidOperationException(
                    $"Either port {DefaultValues.TcpMessagingPort} " +
                    $"is not open , or one instance of this client is already running.");
            }
        }

        private string clientName { get; set; }

        public event EventHandler<LogEventArgs> OnLocalEventHappened;

        public event EventHandler<LogEventArgs> OnEventHappened;

        public event EventHandler<ChatHistoryUpdatedEventArgs> OnChatHistoryUpdated;
         
        private List<PtpUserEntry> connectedUserEntries { get; set; }

        private TcpListener messagesListener { get; set; }

        private UdpClient userInfoBroadcaster { get; set; }

        private UdpClient userInfoReceiver { get; set; }

        public ITcpTracer messagesTracer { get; set; }

        private static IPAddress multicastGroupAddress => IPAddress.Parse("239.1.1.1");

        private static IPEndPoint DefaultOnConnectedEndpoint => new IPEndPoint(IPAddress.Broadcast, DefaultValues.UdpBroadCastPort);

        public IChatHistoryContainer chatHistoryContainer { get; set; }

        public IChatHistoryReceiver chatHistoryReceiver { get; set; }

        private const int MaxWaitTries = 10;

        private const int MaxUpdateChatHistoryTries = 10; 

        public async Task Start()
        {
            await OnConnected();
            _ = Task.Run(() => UdpPacketsListener());
            _ = Task.Run(() => chatHistoryContainer.StartListening());
            _ = Task.Run(() => TryUpdateChatHistory());
        }

        #region Connection stuff

        /// <summary>
        /// When connected, sends broadcast message 
        /// of this clients' username and starts listening for messages
        /// </summary>
        private async Task OnConnected()
        {
            _ = Task.Run(() => AcceptChatConnections());

            await userInfoBroadcaster.SendAsync(
                Encoding.ASCII.GetBytes(clientName),
                clientName.Length,
                DefaultOnConnectedEndpoint);

            OnLocalEventHappened?.Invoke(this, new LogEventArgs() { message = "Sent \"OnConnected\" broadcast message ." });
        }



        /// <summary>
        /// Listens for incoming udp packets containing usernames,
        /// and opens tcp connection with them
        /// </summary>
        /// <returns></returns>
        private async Task UdpPacketsListener()
        {
            while (true)
            {
                var clientEp = new IPEndPoint(IPAddress.Any, 0);
                var receivedBytes = userInfoReceiver.Receive(ref clientEp);
                var receivedMessage = Encoding.ASCII.GetString(receivedBytes);

                //user already connected to us and just wants us to know his name.

                if (receivedMessage.StartsWith("USR "))
                {
                    var username = receivedMessage.Replace("USR ", string.Empty);

                    int index = 0;
                    do
                    {
                        index = connectedUserEntries.FindIndex(x =>
                        x.ipAddress.ToString() == clientEp.Address.ToString());
                    } while (index.Equals(-1));

                    connectedUserEntries[index].username = username;

                    continue;
                }

                var userEntryIndex = connectedUserEntries.FindIndex(x =>
                x.ipAddress.ToString().Equals(clientEp.Address.ToString())
                && x.username != "Unknown");

                //Add new client if we don't know anything about him

                if (userEntryIndex.Equals(-1))
                {
                    connectedUserEntries.Add(new PtpUserEntry()
                    {
                        chatConnection = null,
                        username = receivedMessage,
                        ipAddress = clientEp.Address
                    });

                    userEntryIndex = connectedUserEntries.FindIndex(x =>
                    x.ipAddress.ToString().Equals(clientEp.Address.ToString())
                    && x.username == receivedMessage);
                }

                connectedUserEntries[userEntryIndex].username = receivedMessage;

                await InitiateTcpConnection(userEntryIndex);
            }

        }

        /// <summary>
        /// Opens tcp connection on <see cref="connectedUserEntries"/>[userEntryIndex]
        /// if it's opened, returns
        /// </summary>
        /// <param name="userEntryIndex"></param>
        /// <returns></returns>
        private async Task InitiateTcpConnection(int userEntryIndex)
        {
            //Check if user's tcpConnection is open, if not, open it

            if (connectedUserEntries[userEntryIndex].chatConnection != null
            && connectedUserEntries[userEntryIndex].chatConnection.Connected)
                return;

            var newConnection = new TcpClient();
            newConnection.Connect(new IPEndPoint(connectedUserEntries[userEntryIndex].ipAddress, DefaultValues.TcpMessagingPort));

            connectedUserEntries[userEntryIndex].chatConnection = newConnection;

            _ = Task.Run(() => messagesTracer.TraceConnectionMessages(
                newConnection,
                connectedUserEntries[userEntryIndex].username,
                newConnection.Client.GetRemoteEndPointIpAddress()
                == newConnection.Client.GetLocalEndPointIpAddress()));

            OnLocalEventHappened.Invoke(this, new LogEventArgs()
            {
                message = $"Connected to {connectedUserEntries[userEntryIndex].username} ."
            });

            //send who we are to the user, 
            //add "header" to distinguish between usual OnConnected messages.

            var userInfoBytes = Encoding.ASCII.GetBytes("USR " + clientName);

            await userInfoBroadcaster.SendAsync(
                userInfoBytes,
                userInfoBytes.Length,
                DefaultOnConnectedEndpoint);
        }


        /// <summary>
        /// When connected, user accepts  
        /// tcp connections from other clients
        /// </summary>
        /// <returns></returns>
        private async Task AcceptChatConnections()
        {
            while (true)
            {
                var clientConnection = messagesListener.AcceptTcpClient();

                var address = clientConnection.Client.GetRemoteEndPointIpAddress(false);


                connectedUserEntries.Add(new PtpUserEntry()
                {
                    chatConnection = clientConnection,
                    ipAddress = address,
                    IsConnected = true,
                    username = "Unknown"
                });

                string currentUsername = connectedUserEntries.Find(
                    x =>
                    x.ipAddress.ToString() == address.ToString())
                    .username;

                //wait until the user information is updated
                int tries = 0;
                while (currentUsername == "Unknown")
                {
                    await Task.Delay(150);

                    currentUsername = connectedUserEntries.Find(
                    x =>
                    x.ipAddress.ToString() == address.ToString())
                    .username;

                    ++tries;

                    if (tries > MaxWaitTries)
                        break;
                }

                var message = $"User {currentUsername} connected.";
                OnEventHappened?.Invoke(this, new LogEventArgs() { message = message });
                chatHistoryContainer.NewEntry(message);

                _ = Task.Run(() => messagesTracer.TraceConnectionMessages(clientConnection, currentUsername, false));
            }
        }

        //TODO::
        private void Reconnect()
        {

        }

        #endregion

        #region Message sending

        public void SendMessage(string message)
        {
            connectedUserEntries.ForEach(async client =>
            {
                var clientStream = client.chatConnection.GetStream();
                var messageBytes = Encoding.ASCII.GetBytes(message + "\r\n");
                await clientStream.WriteAsync(messageBytes, 0, messageBytes.Length);
            });

        }

        #endregion

        #region Chat history updating

        private async Task TryUpdateChatHistory()
        {
            var userIndex = connectedUserEntries.FindIndex(x => !x.IsLocalUser());
            int tries = 1;
            while (userIndex.Equals(-1))
            {
                await Task.Delay(350);
                userIndex = connectedUserEntries.FindIndex(x => !x.IsLocalUser());

                if (tries > MaxUpdateChatHistoryTries)
                    return;
            }

            var chatHistory = chatHistoryReceiver.GetChatHistory(connectedUserEntries[userIndex].ipAddress)?.ToList();

            OnChatHistoryUpdated?.Invoke(this, new ChatHistoryUpdatedEventArgs()
            {
                chatHistoryEntries = chatHistory
            });

            chatHistoryContainer.ChatHistory = chatHistory;
        }

        #endregion

        #region Messages tracer events

        private void MessagesTracer_OnUserDisconnected(object sender, UserDisconnectedEventArgs e)
        {
            var message = $"User {e.username} disconnected.";
            OnEventHappened?.Invoke(this, new LogEventArgs() { message = message });
            chatHistoryContainer.NewEntry(message);

            connectedUserEntries.RemoveAll(x =>
                x.ipAddress.ToString().Equals(e.address.ToString())
            );
        }

        private void MessagesTracer_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            OnEventHappened?.Invoke(null, new LogEventArgs() { message = e.normalizedMessage });
            chatHistoryContainer.NewEntry(e.normalizedMessage);
        }

        #endregion
    }
}
