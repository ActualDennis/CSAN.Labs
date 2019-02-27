using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Ethernet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace traceroute_pcap {
    public static class ArpHelper {
        public static string GetMacAddress()
        {
            string macAddresses = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses = string.Join(":", (from z in nic.GetPhysicalAddress().GetAddressBytes() select z.ToString("X2")).ToArray());
                    break;
                }
            }

            return macAddresses;
        }

        public static IPAddress GetRouterIp()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic.GetIPProperties().DhcpServerAddresses[0];
                }
            }

            return null;
        }

        public static string GetRouterMacAddress(string thisMachineLocalIp, string routerLocalIp, PacketDevice captureDevice, int maxTries)
        {
            using (PacketCommunicator Communicator =
               captureDevice.Open(65536,
               PacketDeviceOpenAttributes.None,
               1000))
            using (PacketCommunicator inputCommunicator =
               captureDevice.Open(65536,
               PacketDeviceOpenAttributes.None,
               1000))
            {
                inputCommunicator.SetFilter($"ether proto \\arp and dst host \\{thisMachineLocalIp}");

                Communicator.SendPacket(BuildArpPacket(thisMachineLocalIp, routerLocalIp));

                for (int trie = 0; trie < maxTries; ++trie)
                {
                    inputCommunicator.ReceivePacket(out var receivedPacket);

                    if (receivedPacket == null)
                        continue;

                    var arpDataGram = receivedPacket.Ethernet.Arp;

                    if (arpDataGram.Operation.HasFlag(ArpOperation.Reply))
                    {
                        return string.Join(":", (from z in arpDataGram.SenderHardwareAddress select z.ToString("X2")).ToArray());
                    }
                }

                return null;
            }
        }

        private static Packet BuildArpPacket(string source, string routerLocalIp)
        {
            var macAddr = GetMacAddress();
            var arp = new ArpLayer
            {
                ProtocolType = EthernetType.IpV4,
                SenderHardwareAddress = PhysicalAddress.Parse(macAddr.Replace(":", "-")).GetAddressBytes().AsReadOnly(),
                TargetProtocolAddress = IPAddress.Parse(routerLocalIp).GetAddressBytes().AsReadOnly(),
                TargetHardwareAddress = PhysicalAddress.Parse(macAddr.Replace(":", "-")).GetAddressBytes().AsReadOnly(),
                SenderProtocolAddress = IPAddress.Parse(source).GetAddressBytes().AsReadOnly(),
                Operation = ArpOperation.Request
            };
            var eth = new EthernetLayer
            {
                Source = new MacAddress(macAddr),
                Destination = new MacAddress("ff:ff:ff:ff:ff:ff"),
                EtherType = EthernetType.Arp
            };

            return PacketBuilder.Build(DateTime.Now, eth, arp);
        }
    }
}
