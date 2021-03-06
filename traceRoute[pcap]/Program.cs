﻿using PcapDotNet.Core;
using PcapDotNet.Packets.Icmp;
using System;
using System.Net;
using traceroute_pcap_;

namespace traceroute_pcap {
    class Program {
        static void Main(string[] args)
        {
            ArgsInfo argsInfo = ArgsResolver.Resolve(new string[] { "google.by", "30", "-er" }); //ArgsResolver.Resolve(args);

            if (argsInfo == null)
            {
                Usage();
                return;
            }

            const int _maxTries = 3;

            PacketDevice selectedCaptureDevice;

            try
            {
                selectedCaptureDevice = PcapDevicesChooser.PromptPacketDevice();

                if (selectedCaptureDevice == null)
                    throw new Exception();
            }
            catch { Console.WriteLine("Couldn't initialize packet device."); Console.ReadLine(); return; }

            bool IsCompleted = false;

            Console.WriteLine($"Tracing to {argsInfo.Destination.ToString()}");
            Console.WriteLine();

            bool IsAnyResponseReceived = false;
            var lastCapturedSource = string.Empty;

            using (var pingHelper = new Ping(selectedCaptureDevice, 500))
            {
                try
                {
                    pingHelper.Initialize();
                }
                catch (SystemException e)
                {
                    Console.WriteLine($"Program couldn't start. Reason: {e.Message}");
                    Console.ReadLine();
                    return;
                }

                int trie = 0;
                int errorPackets = 0;
                int maxErrorPackets = 10;
                bool isSourceCaptured = false;

                for (byte ttl = 1; ttl < argsInfo.HopsCount && !IsCompleted; ++ttl)
                {
                    Console.Write($"{ttl}. ");
                    for (trie = 0; trie < _maxTries; ++trie)
                    {
                        var packet = pingHelper.Send(argsInfo.Destination, 10, ttl, true);

                        if (packet == null)
                        {
                            Console.Write($"   *   ");
                            continue;
                        }

                        var ipDatagram = packet.ReplyPacket.IpV4;

                        lastCapturedSource = ipDatagram.Source.ToString();

                        isSourceCaptured = true;

                        var icmpDatagram = packet.ReplyPacket.Ip.Icmp;

                        if (icmpDatagram.MessageType == IcmpMessageType.TimeExceeded || icmpDatagram.MessageType == IcmpMessageType.EchoReply)
                        {

                            var serverReplyTime = pingHelper.GetServerResponseTime(10, ipDatagram.Source.ToString());

                            if (serverReplyTime.Equals(-1))
                            {
                                Console.Write($"   *   ");
                                continue;
                            }

                            Console.Write($" {serverReplyTime} ms  ");
                            IsAnyResponseReceived = true;
                            lastCapturedSource = ipDatagram.Source.ToString();

                        }
                        else
                        {
                            ++errorPackets;

                            if (errorPackets > maxErrorPackets)
                                continue;

                            --trie;
                        }

                    }

                    if (IsAnyResponseReceived)
                    {
                        Console.Write($" {lastCapturedSource} ");

                        if (argsInfo.IsReversedLookupEnabled)
                        {
                            try
                            {
                                var hostName = NamesResolver.GetHostNameByIp(IPAddress.Parse(lastCapturedSource));
                                Console.Write(hostName);
                            }
                            catch { }
                        }

                        if (lastCapturedSource == argsInfo.Destination.ToString())
                        {
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine("Trace completed.");
                            IsCompleted = true;
                        }
                    }
                    else if (isSourceCaptured)
                        Console.Write($"{lastCapturedSource} didn't reply to ping request.");
                    else
                        Console.Write("No response was received.");

                    isSourceCaptured = false;

                    IsAnyResponseReceived = false;

                    Console.WriteLine();
                    Console.WriteLine();
                }
            }

            Console.ReadLine();

            

        }


        private static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("<host name or IP address of endpoint> <hops count> [-ER]");
            Console.WriteLine("-[E]nable [R]everse LookUp - enables reverse dns requests.");
            Console.ReadLine();
        }
    }
}
