using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Chat.Events {
    public class UserConnectedEventArgs {
        public IPEndPoint EndPoint;
        public string username;
    }
}
