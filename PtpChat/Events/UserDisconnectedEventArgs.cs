using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Chat.Events {
    public class UserDisconnectedEventArgs {
        public string username;
        public IPAddress address;
    }
}
