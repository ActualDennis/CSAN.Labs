using http_filetransfer.Data;
using http_filetransfer.FileSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace http_filetransfer.Commands
{
    public class PutCommand : HttpCommand
    {
        public PutCommand(DefaultFileSystemProvider fsProvider)
        {
            this.fsProvider = fsProvider;
        }

        private DefaultFileSystemProvider fsProvider { get; set; }

        public override void Execute(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            string fullPath = DefaultValues.ServerBaseDirectory + request.RawUrl;

            try
            {
                var copyTo = request.Headers[DefaultValues.CopyToHeader];

                if (copyTo == null)
                {
                    var dirname = Path.GetDirectoryName(fullPath);

                    if (!Directory.Exists(dirname))
                    {
                        Directory.CreateDirectory(dirname);
                    }

                    using (var newFile = new FileStream(fullPath, FileMode.Create))
                    {
                        request.InputStream.CopyTo(newFile, DefaultValues.BufferSize);
                    }

                    return;
                }

                fsProvider.Move(fullPath, DefaultValues.ServerBaseDirectory + "/" + copyTo.TrimStart('/'));
            }
            catch (FileNotFoundException)
            {
                response.StatusCode = 404;
            }
            catch (DirectoryNotFoundException)
            {
                response.StatusCode = 404;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex.Message}");
                response.StatusCode = 400;
            }
            finally { response.OutputStream.Close(); }
        }
    }
}
