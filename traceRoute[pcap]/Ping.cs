using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace traceroute_pcap {
    public class Ping : IDisposable {
        public Ping(PacketDevice captureDevice, int receiveTimeout)
        {
            this.captureDevice = captureDevice;
            sendCommunicator =
                this.captureDevice.Open(65536,
                PacketDeviceOpenAttributes.None,
                receiveTimeout);
            inputCommunicator =
                this.captureDevice.Open(65536, // portion of the packet to capture
                                                  // 65536 guarantees that the whole packet will be captured on all the link layers
                PacketDeviceOpenAttributes.None,
                receiveTimeout);

            this.receiveTimeout = receiveTimeout;
        }

        /// <summary>
        /// This method is out of constructor 
        /// because it may be executed slowly or throw an exception
        /// </summary>
        public void Initialize()
        {
            localIpAddress = captureDevice.Addresses[1].ToString().Split(' ')[2];

            inputCommunicator.SetFilter($"ip proto \\icmp and dst host \\{localIpAddress}");

            routerMac = ArpHelper.GetRouterMacAddress(localIpAddress, ArpHelper.GetRouterIp().ToString(), captureDevice, _maxTries);

            thisMachineMac = ArpHelper.GetMacAddress();

            if (routerMac == null)
            {
                throw new SystemException("Cannot identify router's MAC.");
            }
        }

        public PacketDevice captureDevice { get; private set; }

        public string localIpAddress { get; private set; }

        public string routerMac { get; private set; }

        public string thisMachineMac { get; private set; }

        private const int _maxTries = 4;

        private const byte defaultTtl = 64;

        private int receiveTimeout { get; set; }

        private PacketCommunicator sendCommunicator { get; set; }

        private PacketCommunicator inputCommunicator { get; set; }


        public PingReply Send(IPAddress hostIp, int waitFor, byte ttl, bool ttlNeeded)
        {
            var timeStampSent = DateTime.Now;
            sendCommunicator.SendPacket(BuildIcmpPacket(hostIp.ToString(), thisMachineMac, routerMac, ttlNeeded ? ttl : defaultTtl, localIpAddress));
            inputCommunicator.ReceivePacket(out Packet receivedPacket);

            if (receivedPacket == null)
            {
                return null;
            }

            return new PingReply()
            {
                ReplyPacket = receivedPacket.Ethernet,
                RoundtripTime = (timeStampSent - receivedPacket.Timestamp).Milliseconds
            };
        }

        /// <summary>
        /// Sends icmp packet to server with <see cref="defaultTtl"/>
        /// ttl value, measures response time.
        /// -1 is returned when server didn't reply in <see cref="receiveTimeout"/>
        /// </summary>
        /// <param name="maxErrorPackets"></param>
        /// <param name="serverIp"></param>
        /// <returns></returns>
        public int GetServerResponseTime(int maxErrorPackets, string serverIp)
        {
            var timeStampSent = DateTime.Now;
            sendCommunicator.SendPacket(BuildIcmpPacket(serverIp, thisMachineMac, routerMac, defaultTtl, localIpAddress));
            //server may send some random packets, which are not echo replies,
            //wait for {maxErrorPackets} packets for our packet to arrive

            byte errorPacketsCounter = 0;

            for (; errorPacketsCounter < maxErrorPackets; ++errorPacketsCounter)
            {
                inputCommunicator.ReceivePacket(out var echoPacket);

                if (echoPacket == null)
                    continue;

                var echoDgram = echoPacket.Ethernet.Ip.Icmp;

                if (echoDgram.MessageType == IcmpMessageType.EchoReply)
                {
                    return (echoPacket.Timestamp - timeStampSent).Milliseconds;
                }
            }

            return -1;
        }

        static Packet BuildIcmpPacket(string ipDestination, string thisMachineMac, string routerMac, byte ttl, string source)
        {

            EthernetLayer ethernetLayer =
                 new EthernetLayer
                 {
                     Source = new MacAddress(thisMachineMac),
                     Destination = new MacAddress(routerMac),
                     EtherType = EthernetType.None, // Will be filled automatically.
                 };

            IpV4Layer ipV4Layer =
                new IpV4Layer
                {
                    Source = new IpV4Address(source),
                    CurrentDestination = new IpV4Address(ipDestination),
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null, // Will be filled automatically.
                    Identification = (ushort)(ttl * 2 % 8192),
                    Options = IpV4Options.None,
                    Protocol = null, // Will be filled automatically.
                    Ttl = ttl,
                    TypeOfService = 0,
                };

            IcmpEchoLayer icmpLayer =
                new IcmpEchoLayer
                {
                    Checksum = null, // Will be filled automatically.
                    Identifier = 1,
                    SequenceNumber = (ushort)(ttl * 2 % 8192),
                };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, icmpLayer);

            return builder.Build(DateTime.Now);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    captureDevice = null;
                }

                sendCommunicator.Dispose();
                inputCommunicator.Dispose();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
