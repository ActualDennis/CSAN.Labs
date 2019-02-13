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
                    var response = TraceRouteHelper.GetServerResponse(i, 2000, remoteAddress);
                    if (response.Status == IPStatus.TtlExpired || response.Status == IPStatus.Success)
                    {
                        var firstRes = TraceRouteHelper.GetServerResponseTime(response.Address);
                        var secondRes = TraceRouteHelper.GetServerResponseTime(response.Address);
                        var thirdRes = TraceRouteHelper.GetServerResponseTime(response.Address);

                        Console.Write($"{i} {response.Address} " +
                            $": {(firstRes == 0 ? "*" : firstRes.ToString())}ms " +
                            $": {(secondRes == 0 ? "*" : secondRes.ToString())}ms " +
                            $": {(thirdRes == 0 ? "*" : thirdRes.ToString())}ms");

                        if (firstRes == 0 && secondRes == 0 && thirdRes == 0)
                            Console.Write(" Request timed out.");
                        else
                            Console.Write($" {NamesResolver.GetHostNameByIp(response.Address) ?? string.Empty}");


                         Console.WriteLine();

                        if (response.Address.ToString() == remoteAddress.ToString())
                            break;
                    }
                    else
                    {
                        Console.WriteLine($"{i} {response.Address} * * * Request timed out.");
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

    }
}
