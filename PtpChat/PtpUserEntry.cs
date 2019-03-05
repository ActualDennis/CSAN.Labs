using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat {
    public class PtpUserEntry {
        public string username;
        public IPAddress ipAddress;
        public TcpClient chatConnection;
        public bool IsConnected;
    }
}
