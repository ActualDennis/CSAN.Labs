using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using HttpProxy.Firewalls;
using System.Threading.Tasks;

namespace HttpProxy.Listener {
    public class HttpListener : IListener<RequestReceivedEventArgs> {
        public HttpListener(int port)
        {
            this.port = port;
            Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
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

        public void StartReceivingData(TcpClient client)
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
    }
}
