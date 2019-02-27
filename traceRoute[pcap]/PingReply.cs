using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Icmp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace traceroute_pcap {
    public class PingReply {
        public EthernetDatagram ReplyPacket;
        public long RoundtripTime;
    }
}
