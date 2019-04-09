

using http_filetransfer.Commands;
using http_filetransfer.Data;
using http_filetransfer.FileSystems;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace http_filetransfer {
    public class HttpRequestsHandler {

        public HttpRequestsHandler(DefaultFileSystemProvider fileSystemProvider, CommandFactory HttpCommandfactory)
        {
            this.fileSystemProvider = fileSystemProvider;
            this.HttpCommandfactory = HttpCommandfactory;
        }

        private DefaultFileSystemProvider fileSystemProvider { get; set; }

        private CommandFactory HttpCommandfactory { get; set; }

        public void Start()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:80/");
            listener.Start();

            while (true)
            {
                HttpListenerContext context = listener.GetContext();

                var command = HttpCommandfactory.GetCommand(context.Request.HttpMethod, fileSystemProvider);

                if(command is UnrecognizedCommand)
                {
                    context.Response.StatusCode = DefaultValues.NotImplementedResponseCode;
                    continue;
                }

                HttpListenerResponse response = context.Response;

                command.Execute(context.Request, ref response);
            }

        }
    }
}
