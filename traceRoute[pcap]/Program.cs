using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace traceroute_pcap {
    class Program {
        static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length > 2)
            {
                Usage();
                return;
            }

            var destinationIpAddress = NamesResolver.Resolve(args[0]);
            var IsReversedLookupEnabled = args.FirstOrDefault(x => x.ToUpper() == "-ENABLEREVLOOKUP") != null; 
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

            using (PacketCommunicator sendCommunicator =
                selectedCaptureDevice.Open(65536,
                PacketDeviceOpenAttributes.None,
                1000))
            using (PacketCommunicator inputCommunicator =
                selectedCaptureDevice.Open(65536, // portion of the packet to capture
                                                 // 65536 guarantees that the whole packet will be captured on all the link layers
                                         PacketDeviceOpenAttributes.None,
                                         500)) // read timeout
            {
                var timeStampSent =  new DateTime();
                inputCommunicator.SetFilter("ip proto \\icmp and dst host \\192.168.0.104");
                bool IsCompleted = false;
                byte maxErrorPackets = 10;

                for(byte ttl = 1; ttl < 20 && !IsCompleted; ++ttl)
                {
                    for (int trie = 0; trie < _maxTries; ++trie)
                    {                     
                        sendCommunicator.SendPacket(BuildIcmpPacket(destinationIpAddress.ToString(), ttl));
     
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

                            sendCommunicator.SendPacket(BuildIcmpPacket(ipDatagram.Source.ToString(), 64));

                            byte errorPacketsCounter = 0;

                            //server may send some random packets, which are not echo replies,
                            //wait for {maxErrorPackets} packets for our packet to arrive

                            while (errorPacketsCounter < maxErrorPackets)
                            {
                                inputCommunicator.ReceivePacket(out var echoPacket);

                                if (echoPacket == null)
                                {
                                    ++errorPacketsCounter;
                                    continue;
                                }

                                var echoDgram = echoPacket.Ethernet.Ip.Icmp;

                                if (echoDgram.MessageType == IcmpMessageType.EchoReply)
                                {
                                    Console.Write($" {(echoPacket.Timestamp - timeStampSent).Milliseconds} ms  ");
                                    break;
                                }
                                else
                                    ++errorPacketsCounter;
                            }

                            if (errorPacketsCounter == maxErrorPackets)
                                Console.Write("    *    ");

                            //on the third ping, output ip address [and hostname] of the source.

                            if (trie == _maxTries - 1)
                            {

                                Console.Write($" {ipDatagram.Source.ToString()} : ");

                                if (IsReversedLookupEnabled)
                                {
                                    Console.Write($"{(NamesResolver.GetHostNameByIp(IPAddress.Parse(ipDatagram.Source.ToString()))) ?? string.Empty} ");
                                }

                                if (ipDatagram.Source.ToString() == destinationIpAddress.ToString())
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
                }

                Console.ReadLine();

            }

        }

        static Packet BuildIcmpPacket(string ipDestination, byte ttl)
        {

            EthernetLayer ethernetLayer =
                 new EthernetLayer
                 {
                     Source = new MacAddress("F4:96:34:37:4D:D0"),
                     Destination = new MacAddress("60:E3:27:B1:C8:4C"),
                     EtherType = EthernetType.None, // Will be filled automatically.
                };

            IpV4Layer ipV4Layer =
                new IpV4Layer
                {
                    Source = new IpV4Address("192.168.0.104"),
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
            Console.WriteLine("<host name or its IP address> [-EnableRevLookUp]");
            Console.WriteLine("-EnableRevLookUp - enables reverse dns requests.");
            Console.ReadLine();
        }
    }
}
