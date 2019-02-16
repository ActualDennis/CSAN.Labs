using Chat.Logging;
using System;

namespace Chat
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the p2p chat!");
            Console.WriteLine("Please, type in your username.");
            var input = Console.ReadLine().ToUpper();

            var client = new PtpClient(new ConsoleLogger());
            client.ClientName = input;
            client.Initialize();
            while (true)
            {
                var message = Console.ReadLine();
                client.SendMessage(message);
            }
        }
    }
}