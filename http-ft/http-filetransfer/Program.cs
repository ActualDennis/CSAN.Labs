using http_filetransfer.FileSystems;
using System;
using System.Threading.Tasks;

namespace http_filetransfer {
    class Program {
        static void Main(string[] args)
        {
            var handler = new HttpRequestsHandler(new DefaultFileSystemProvider());
            Task.Run(() => handler.Start());
            Console.ReadLine();
        }
    }
}
