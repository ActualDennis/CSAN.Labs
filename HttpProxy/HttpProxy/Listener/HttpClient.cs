using HttpProxy.Firewalls;
using HttpProxy.Loggers;
using HttpProxy.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpProxy.Listener {
    public class HttpClient {
        public HttpClient(Firewall firewall, IListener<RequestReceivedEventArgs> listener, Logger<HttpRequestEntry> logger)
        {
            this.firewall = firewall;
            this.listener = listener;
            this.listener.OnNewRequestReceived += Listener_OnNewConnectionReceived;
            this.logger = logger;
        }

        private Firewall firewall { get; set; }
        private IListener<RequestReceivedEventArgs> listener { get; set; }

        private Logger<HttpRequestEntry> logger { get; set; }

        private void Listener_OnNewConnectionReceived(object sender, RequestReceivedEventArgs e)
        {
            string hostname = HttpQueryParser.GetHostName(e.Request);

            try
            {
                if (firewall.CheckIfBlocked(hostname))
                {
                    Console.WriteLine("This website is blocked.");
                    return;
                }

                var targetServer = new TcpClient(hostname, 80);

                NetworkStream targetServerStream = targetServer.GetStream();

                NetworkStream proxyClientStream = e.User.GetStream();

                targetServerStream.Write(e.Request);

                var builder = new StringBuilder();

                var responseBuffer = new byte[81920];

                for (int offsetCounter = 0; true; ++offsetCounter)
                {
                    var bytesRead = targetServerStream.Read(responseBuffer, 0, 81920);

                    if (bytesRead.Equals(0))
                        return;

                    proxyClientStream.Write(responseBuffer, 0, 81920);

                    builder.Append(Encoding.UTF8.GetString(responseBuffer));

                    if (offsetCounter.Equals(0))
                    {
                        var headers = builder.ToString().Split("\r\n");

                        logger.Log(new HttpRequestEntry()
                        {
                            ResponseCode = headers[0].Substring(headers[0].IndexOf(" ") + 1),
                            Hostname = hostname
                        });
                    }
                }
                      
            }
            catch { return; }

           
        }

    }
}
