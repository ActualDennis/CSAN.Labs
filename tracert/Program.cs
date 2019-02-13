using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace tracert {
    class Program {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("This program requeres command line arguments , usage: tracert.exe hostOrIpAddress [max hops count]");
                Console.ReadLine();
                return;
            }
            int maxHopsCount = 0;

            if(args.Length == 2)
            {
                if (!int.TryParse(args[1], out maxHopsCount))
                {
                    maxHopsCount = 30;
                }
            }

            try
            {
                var remoteAddress = NamesResolver.Resolve(args[0]);
                Console.WriteLine($"Trying to start tracing of the host {remoteAddress.ToString()}");
                Console.WriteLine($"Maximum hops is set to {maxHopsCount}");
                var pinger = new Ping();

                for (int i = 1; i < maxHopsCount; ++i)
                {
                    var y = pinger.Send(remoteAddress, 2000, new byte[64], new PingOptions(i, false));
                    if (y.Status == IPStatus.TtlExpired || y.Status == IPStatus.Success)
                    {
                        var firstRes = GetServerResponseTime(y.Address);
                        var secondRes = GetServerResponseTime(y.Address);
                        var thirdRes = GetServerResponseTime(y.Address);

                        Console.Write($"{i} {y.Address} " +
                            $": {(firstRes == 0 ? "*" : firstRes.ToString())}ms " +
                            $": {(secondRes == 0 ? "*" : secondRes.ToString())}ms " +
                            $": {(thirdRes == 0 ? "*" : thirdRes.ToString())}ms");

                        if (firstRes == 0 && secondRes == 0 && thirdRes == 0)
                            Console.Write(" Request timed out.");
                        else
                            Console.Write($" {NamesResolver.GetHostNameByIp(y.Address) ?? string.Empty}");


                         Console.WriteLine();

                        if (y.Address.ToString() == remoteAddress.ToString())
                            break;
                    }
                    else
                    {
                        Console.WriteLine($"{i} {y.Address} * * * Request timed out.");
                    }

                    if(i == maxHopsCount - 1)
                        throw new ArgumentException($"Couldn't find route to the {remoteAddress.ToString()}");
                }

                Console.WriteLine();
                Console.WriteLine("Trace complete.");

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally { Console.ReadLine(); }
        }

        static long GetServerResponseTime(IPAddress address)
        {
            return new Ping().Send(address, 64, new byte[32]).RoundtripTime;
        }

    }
}
