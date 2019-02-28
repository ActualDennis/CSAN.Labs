using System.Net;

namespace traceroute_pcap {
    public class ArgsInfo {
        public IPAddress Destination;
        public bool IsReversedLookupEnabled;
        public int HopsCount;
    }
}
