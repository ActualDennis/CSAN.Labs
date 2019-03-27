using Chat.Chat_history;
using Chat.Connections;
using Chat.Logging;
using Chat.Messages_Tracer;
using System;

namespace Chat
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the p2p chat!");
            Console.WriteLine("Please, type in your username.");
            var input = Console.ReadLine();

            try
            {

                var connManager = new PtpConnectionManager(
                    input,
                    new MessagesTracer(),
                    new ChatHistoryContainer(),
                    new ChatHistoryReceiver()
                    );

                var client = 
                    new PtpClient(
                    new ConsoleLogger(),
                    connManager) 
                    {
                        ClientName = input
                    };

                while (true)
                {
                    var message = Console.ReadLine();
                    client.SendMessage(message);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}