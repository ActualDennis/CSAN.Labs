using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace http_filetransfer.Commands
{
    public abstract class HttpCommand
    {
        public abstract void Execute(HttpListenerRequest request, ref HttpListenerResponse response);
    }
}
