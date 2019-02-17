using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Chat {
    public class PtpClient {
        public PtpClient(ILocalLogger logger)
        {
            this.logger = logger;
            userInfoBroadcaster = new UdpClient();
            connectedUserEntries = new List<PtpUserEntry>();
            chatHistory = new List<string>();
            InitializeLocal();
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

        private UdpClient userInfoBroadcaster { get; set; }

        private UdpClient userInfoReceiver { get; set; }

        private static IPAddress multicastGroupAddress => IPAddress.Parse("239.1.1.1");

        private static IPEndPoint DefaultOnConnectedEndpoint => new IPEndPoint(IPAddress.Broadcast, DefaultValues.UdpPort);

        private ILocalLogger logger { get; set; }

        private List<PtpUserEntry> connectedUserEntries { get; set; }

        private TcpListener messagesListener { get; set; }

        private List<string> chatHistory { get; set; }

        //Find free port on this machine to listen for incoming requests
        private void InitializeLocal()
        {
            userInfoReceiver = new UdpClient();
            userInfoReceiver.JoinMulticastGroup(multicastGroupAddress);
            userInfoReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            userInfoReceiver.Client.Bind(new IPEndPoint(IPAddress.Any, DefaultValues.UdpPort));

            userInfoBroadcaster.JoinMulticastGroup(multicastGroupAddress);

            try
            {
                messagesListener = new TcpListener(IPAddress.Any, DefaultValues.TcpPort);
                messagesListener.Start();
            }
            catch
            {
                logger.LogLocal($"Either port {DefaultValues.TcpPort} is not open , or one instance of this client is already running.");
            }
        }

        public async Task Initialize()
        {
            await OnConnected();
            _ = Task.Run(() => PermanentListener());
        }

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

            logger.LogLocal("Sent \"OnConnected\" broadcast message .");
        }


        /// <summary>
        /// Listens for incoming udp packets containing usernames and updates 
        /// <see cref="connectedUserEntries"/> accordingly
        /// </summary>
        /// <returns></returns>
        private async Task PermanentListener()
        {
            while (true)
            {
                var clientEp = new IPEndPoint(IPAddress.Any, 0);
                var receivedBytes = userInfoReceiver.Receive(ref clientEp);
                var receivedMessage = Encoding.ASCII.GetString(receivedBytes);


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

                //if this client was initialized before, we must be able to accept new clients

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
            newConnection.Connect(new IPEndPoint(connectedUserEntries[userEntryIndex].ipAddress, DefaultValues.TcpPort));
            connectedUserEntries[userEntryIndex].chatConnection = newConnection;

            _ = Task.Run(() => TraceConnectionMessages(
                newConnection,
                newConnection.Client.GetRemoteEndPointIpAddress()
                == newConnection.Client.GetLocalEndPointIpAddress()));

            logger.Log($"Connected to {connectedUserEntries[userEntryIndex].username} .");

            //send who we are to the user

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

                //do not duplicate this user's connection

                //if (address.ToString() ==
                //    ((IPEndPoint)clientConnection.Client.LocalEndPoint).Address.ToString())
                //    continue;

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

                while (currentUsername == "Unknown")
                {
                    await Task.Delay(150);

                    currentUsername = connectedUserEntries.Find(
                    x =>
                    x.ipAddress.ToString() == address.ToString())
                    .username;
                }

                logger.Log($"User {currentUsername} connected.");

                _ = Task.Run(() => TraceConnectionMessages(clientConnection, false));
            }
        }



        /// <summary>
        /// Waits for messages from the "connection" parameter
        /// And displays them.
        /// If it's local connection, i.e this user wants to see his own messages,
        /// he should trace messages only once.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private async Task TraceConnectionMessages(TcpClient connection, bool IsLocalConnection)
        {
            if (IsLocalConnection)
                return;

            var address = connection.Client.GetRemoteEndPointIpAddress(false);
            var username = connectedUserEntries.Find((x) => x.ipAddress.Equals(address)).username;

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

                    logger.Log(normalizedMessage);
                    chatHistory.Add(normalizedMessage);
                }
            }
            finally { OnUserDisconnected(username, address); }
        }

        private void OnUserDisconnected(string username, IPAddress address)
        {
            var message = $"User {username} disconnected.";
            logger.Log(message);
            chatHistory.Add(message);

            connectedUserEntries.RemoveAll(x =>
                x.ipAddress.ToString().Equals(address.ToString())
            );
        }

        private string GetNormalizedMessage(string username, string message)
        {
            return $"{DateTime.Now.ToString("HH:MM")} " +
                            $"{username} : " +
                            $"{message}";
        }

        public void SendMessage(string message)
        {
            connectedUserEntries.ForEach(async client =>
            {
                var clientStream = client.chatConnection.GetStream();
                var messageBytes = Encoding.ASCII.GetBytes(message + "\r\n");
                await clientStream.WriteAsync(messageBytes, 0, messageBytes.Length);
            });

        }

        //TODO::
        private void Reconnect()
        {

        }
    }
}