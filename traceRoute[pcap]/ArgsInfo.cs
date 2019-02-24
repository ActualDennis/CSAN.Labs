using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace traceroute_pcap_ {
    public class ArgsInfo {
        public IPAddress Destination;
        public string RouterIP;
        public bool IsReversedLookupEnabled;
    }
}
