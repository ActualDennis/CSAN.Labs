using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace tracert {
    public static class TraceRouteHelper {
        public static long GetServerResponseTime(IPAddress address)
        {
            return new Ping().Send(address, 64, new byte[32]).RoundtripTime;
        }

        public static PingReply GetServerResponse(int ttl, int waitFor, IPAddress hostAddress)
        {
            return new Ping().Send(hostAddress, 2000, new byte[32], new PingOptions(ttl, false));
        }
    }
}
