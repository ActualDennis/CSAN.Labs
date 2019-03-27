using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace http_filetransfer.Data {
    public static class DefaultValues {
        public static IPAddress DefaultListenIp => IPAddress.Parse("127.0.0.1");

        public static string ServerBaseDirectory => "H:/httpft";

        public static int BufferSize => 15096;
    }
}
