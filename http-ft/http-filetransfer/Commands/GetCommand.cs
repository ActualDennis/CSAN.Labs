using http_filetransfer.Data;
using http_filetransfer.FileSystems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace http_filetransfer.Commands
{
    public class GetCommand : HttpCommand
    {
        public GetCommand(DefaultFileSystemProvider fsProvider)
        {
            this.fsProvider = fsProvider;
        }

        private DefaultFileSystemProvider fsProvider { get; set; }

        public override void Execute(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            Stream output = response.OutputStream;

            var writer = new StreamWriter(output);

            string fullPath = DefaultValues.ServerBaseDirectory + request.RawUrl;

            try
            {
                //if this is a directory
                if (!File.Exists(fullPath))
                {
                    var directoryListing = fsProvider.EnumerateDirectory(fullPath);

                    foreach (var entry in directoryListing)
                    {
                        writer.Write(
                            JsonConvert
                            .SerializeObject(entry,
                            new JsonSerializerSettings()
                            {
                                DateFormatString = "yyyy/MM/dd HH:mm",
                                Formatting = Formatting.Indented
                            }));
                    }
                    writer.Flush();
                }
                else
                {
                    Stream file = fsProvider.GetFileStream(fullPath);
                    file.CopyTo(output, DefaultValues.BufferSize);
                }
            }
            catch (FileNotFoundException)
            {
                response.StatusCode = 404;
                writer.Write("File was not found on the server.");
            }
            catch (DirectoryNotFoundException)
            {
                response.StatusCode = 404;
                writer.Write("Directory was not found on the server.");
            }
            catch (Exception ex)
            {
                response.StatusCode = 400;
                writer.Write($"Local error happened: {ex.Message}.");
            }
            finally { output.Close(); writer.Dispose(); }
        }
    }
}
