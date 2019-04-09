using http_filetransfer.Commands;
using http_filetransfer.FileSystems;
using System;
using System.Threading.Tasks;

namespace http_filetransfer {
    class Program {
        static void Main(string[] args)
        {
            var handler = new HttpRequestsHandler(new DefaultFileSystemProvider(), new CommandFactory());
            Task.Run(() => handler.Start());
            Console.WriteLine("Http file server is running!. Hit enter to close it.");
            Console.ReadLine();
        }
    }
}
