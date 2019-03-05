using http_filetransfer.Data;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace http_filetransfer.listeners {
    public class HttpListener : IListener<RequestReceivedEventArgs>, IDisposable {
        public HttpListener(int port)
        {
            this.port = port;
            Listener = new TcpListener(DefaultValues.DefaultListenIp, port);
        }

        public int port { get; private set; }

        private TcpListener Listener { get; set; }

        public event EventHandler<RequestReceivedEventArgs> OnNewRequestReceived;

        public void Listen()
        {
            Listener.Start();

            while (true)
            {
                var client = Listener.AcceptTcpClient();

                Task.Run(() => StartReceivingData(client));      
            }
            
        }

        private void StartReceivingData(TcpClient client)
        {
            NetworkStream clientStream = client.GetStream();

            var buffer = new byte[16000];

            while (true)
            {
                try
                {
                    if (!clientStream.CanRead)
                        return;

                    //connection is closed
                    if (clientStream.Read(buffer).Equals(0))
                        return;

                    OnNewRequestReceived?.Invoke(this, new RequestReceivedEventArgs() { User = client, Request = buffer });
                } // when clientStream is disposed, exception is thrown.
                catch { return; }
            }
        }

        public void Dispose()
        {
            Listener.Stop();
        }
    }
}
