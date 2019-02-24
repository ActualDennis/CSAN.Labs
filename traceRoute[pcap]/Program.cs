using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using traceroute_pcap_;

namespace traceroute_pcap {
    class Program {
        static void Main(string[] args)
        {
            ArgsInfo argsInfo = ArgsResolver.Resolve(args); //ArgsResolver.Resolve(new string[] { "192.168.0.1", "google.com" });

            if (argsInfo == null)
            {
                Usage();
                return;
            }

            const int _maxTries = 3;

            // Retrieve the device list from the local machine
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
                return;
            }

            // Print the list
            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                Console.Write((i + 1) + ". " + device.Name);
                if (device.Description != null)
                    Console.WriteLine(" (" + device.Description + ")");
                else
                    Console.WriteLine(" (No description available)");
            }

            int deviceIndex = 0;
            do
            {
                Console.WriteLine("Enter the interface number (1-" + allDevices.Count + "):");
                string deviceIndexString = Console.ReadLine();
                if (!int.TryParse(deviceIndexString, out deviceIndex) ||
                    deviceIndex < 1 || deviceIndex > allDevices.Count)
                {
                    deviceIndex = 0;
                }
            } while (deviceIndex == 0);

            // Take the selected adapter
            PacketDevice selectedCaptureDevice = allDevices[deviceIndex - 1];

            string[] ipV4Address = selectedCaptureDevice.Addresses[1].ToString().Split(' ');

            var localIpAddress = ipV4Address[2];

            string routerMac = ArpHelper.GetRouterMacAddress(localIpAddress, argsInfo.RouterIP, selectedCaptureDevice, _maxTries);

            string thisMachineMac = ArpHelper.GetMacAddress();

            if (routerMac == null)
            {
                Console.WriteLine("Cannot identify router's MAC.");
                Console.ReadLine();
                return;
            }


            using (PacketCommunicator sendCommunicator =
                selectedCaptureDevice.Open(65536,
                PacketDeviceOpenAttributes.None,
                1000))
            using (PacketCommunicator inputCommunicator =
                selectedCaptureDevice.Open(65536, // portion of the packet to capture
                                                 // 65536 guarantees that the whole packet will be captured on all the link layers
                                         PacketDeviceOpenAttributes.None,
                                         1500)) // read timeout
            {
                var timeStampSent =  new DateTime();
                inputCommunicator.SetFilter($"ip proto \\icmp and dst host \\{localIpAddress}");
                bool IsCompleted = false;
                byte maxErrorPackets = 10;
                Console.WriteLine($"Tracing to {argsInfo.Destination.ToString()}");
                Console.WriteLine();

                for(byte ttl = 1; ttl < 20 && !IsCompleted; ++ttl)
                {
                    for (int trie = 0; trie < _maxTries; ++trie)
                    {                     
                        sendCommunicator.SendPacket(BuildIcmpPacket(argsInfo.Destination.ToString(), thisMachineMac, routerMac, ttl, localIpAddress));
     
                        inputCommunicator.ReceivePacket(out var receivedPacket);

                        if (receivedPacket == null)
                        {
                            Console.Write($" Timed out.");
                            continue;
                        }

                        var ipDatagram = receivedPacket.Ethernet.IpV4;

                        var icmpDatagram = receivedPacket.Ethernet.Ip.Icmp;

                        if (icmpDatagram.MessageType == IcmpMessageType.TimeExceeded || icmpDatagram.MessageType == IcmpMessageType.EchoReply)
                        {

                            //send ping to determine latency

                            timeStampSent = DateTime.UtcNow;

                            sendCommunicator.SendPacket(BuildIcmpPacket(ipDatagram.Source.ToString(), thisMachineMac, routerMac, 64, localIpAddress));

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
                                    Console.Write($" {(echoPacket.Timestamp - timeStampSent).Milliseconds} ms  ");
                                    break;
                                }
                            }

                            if (errorPacketsCounter == maxErrorPackets)
                                Console.Write("    *    ");

                            //on the third ping, output ip address [and hostname] of the source.

                            if (trie == _maxTries - 1)
                            {

                                Console.Write($" {ipDatagram.Source.ToString()} ");

                                if (argsInfo.IsReversedLookupEnabled)
                                {
                                    try
                                    {
                                        var hostName = NamesResolver.GetHostNameByIp(IPAddress.Parse(ipDatagram.Source.ToString()));
                                        Console.Write(hostName);
                                    }
                                    catch { }
                                }

                                if (ipDatagram.Source.ToString() == argsInfo.Destination.ToString())
                                {
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    Console.WriteLine("Trace completed.");
                                    IsCompleted = true;
                                    break;
                                }
                            }
                        }
                
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                }

                Console.ReadLine();

            }

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

        private static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("<your router local address> <host name or its IP address of endpoint> [-ER]");
            Console.WriteLine("-[E]nable [R]everse LookUp - enables reverse dns requests.");
            Console.ReadLine();
        }
    }
}
