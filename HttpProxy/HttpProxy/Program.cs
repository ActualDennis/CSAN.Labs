using HttpProxy.Firewalls;
using HttpProxy.Listener;
using HttpProxy.Loggers;
using System;
using System.Threading.Tasks;

namespace HttpProxy {
    class Program {
        static void Main(string[] args)
        {
            var listener = new HttpListener(55100);
            var logger = new HttpRequestsLogger();
            var client = new HttpClient(new HttpFirewall(), listener, logger);

            Task.Run(() => listener.Listen());

            Console.WriteLine("Available commands: display ");
            Console.WriteLine("Display - shows logs");

            while (true)
            {
                var command = Console.ReadLine().ToUpper();
                switch (command)
                {
                    case "DISPLAY":
                        {
                            PrintLogs(logger);
                            break;
                        }
                }
            }
        }

        static void PrintLogs(HttpRequestsLogger logger)
        {
            foreach(var log in logger.Entries)
            {
                Console.WriteLine($"Host: {log.Hostname} Response code: {log.ResponseCode}");
            }
        }
    }
}
