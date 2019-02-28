using PcapDotNet.Packets.Ethernet;

namespace traceroute_pcap {
    public class PingReply {
        public EthernetDatagram ReplyPacket;
        public long RoundtripTime;
    }
}
