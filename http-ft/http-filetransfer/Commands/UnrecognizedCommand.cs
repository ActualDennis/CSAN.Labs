using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace http_filetransfer.Commands
{
    public class UnrecognizedCommand : HttpCommand
    {
        public override void Execute(HttpListenerRequest request, ref HttpListenerResponse response) => throw new NotImplementedException();
    }
}
