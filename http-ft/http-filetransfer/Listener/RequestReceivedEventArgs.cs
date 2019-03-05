using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace http_filetransfer {
    public class RequestReceivedEventArgs {
        public TcpClient User;
        public byte[] Request;
    }
}
