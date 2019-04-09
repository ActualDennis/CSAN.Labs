using http_filetransfer.FileSystems;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace http_filetransfer.Commands
{
    public class CommandFactory
    {
        public HttpCommand GetCommand(string commandName, DefaultFileSystemProvider defaultFileSystem)
        {
            switch (commandName)
            {
                case "PUT": return new PutCommand(defaultFileSystem);
                case "GET": return new GetCommand(defaultFileSystem);
                case "DELETE": return new DeleteCommand(defaultFileSystem);
                case "HEAD": return new HeadCommand(defaultFileSystem);
                default: return new UnrecognizedCommand();
            }
        }
    }
}
