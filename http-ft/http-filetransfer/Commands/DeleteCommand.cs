using http_filetransfer.FileSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace http_filetransfer.Commands
{
    public class DeleteCommand : HttpCommand
    {
        public DeleteCommand(DefaultFileSystemProvider fsProvider)
        {
            this.fsProvider = fsProvider;
        }

        private DefaultFileSystemProvider fsProvider { get; set; }
        public override void Execute(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            try
            {
                fsProvider.Delete(request.RawUrl);
            }
            catch (FileNotFoundException)
            {
                response.StatusCode = 404;
            }
            catch (DirectoryNotFoundException)
            {
                response.StatusCode = 404;
            }
            catch (Exception)
            {
                response.StatusCode = 400;
            }
            finally { response.OutputStream.Close(); }
        }
    }
}
