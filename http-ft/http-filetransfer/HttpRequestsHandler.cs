

using http_filetransfer.Data;
using http_filetransfer.FileSystems;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace http_filetransfer {
    public class HttpRequestsHandler {

        public HttpRequestsHandler(DefaultFileSystemProvider fileSystemProvider)
        {
            //this.listener = listener;
            //this.listener.OnNewRequestReceived += Listener_OnNewRequestReceived;
            this.fileSystemProvider = fileSystemProvider;
        }

        private DefaultFileSystemProvider fileSystemProvider { get; set; }

        public void Start()
        {
            //Task.Run(() => listener.Listen());

            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:80/");
            listener.Start();

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                // Obtain a response object.
                HttpListenerResponse response = context.Response;

                Stream output = response.OutputStream;

                var writer = new StreamWriter(output);

                string fullPath = DefaultValues.ServerBaseDirectory + request.RawUrl;

                switch (request.HttpMethod)
                {
                    case HttpMethod.Put:
                        {
                            try
                            {
                                var copyfromHeader = request.Headers["Copy-To"];

                                if (copyfromHeader != null)
                                {
                                    fileSystemProvider.Move(fullPath, DefaultValues.ServerBaseDirectory + "/" + copyfromHeader.TrimStart('/'));
                                }
                                else
                                {

                                }
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
                                response.StatusCode = 400;
                            }
                            finally { output.Close(); }
                            break;

                        }
                    case HttpMethod.Get:
                        {
                            try
                            {
                                //if this is a directory
                                if (Path.GetFileName(fullPath) == string.Empty)
                                {
                                    var directoryListing = fileSystemProvider.EnumerateDirectory(fullPath);

                                    foreach (var entry in directoryListing)
                                    {
                                        writer.Write(JsonConvert.SerializeObject(entry));
                                    }
                                    writer.Flush();
                                }
                                else
                                {
                                    Stream file = fileSystemProvider.GetFileStream(fullPath);
                                    file.CopyTo(output, 15096);
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
                            catch(Exception ex)
                            {
                                response.StatusCode = 400;
                                writer.Write($"Local error happened: {ex.Message}.");
                            }
                            finally { output.Close(); }

                            break;
                        }
                    case HttpMethod.Delete:
                        {
                            try
                            {
                                fileSystemProvider.Delete(request.RawUrl);
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
                                response.StatusCode = 400;
                            }
                            finally { output.Close(); }
                            break;
                        }
                    case HttpMethod.Head:
                        {
                            try
                            {
                                //if this is a directory
                                var info = fileSystemProvider.GetFileorDirectoryInfo(fullPath);
                                response.Headers.Add("Name", info.Name);
                                response.Headers.Add("FileLength", info.OccupiedSpace.ToString());
                                response.Headers.Add("LastWriteTime", info.LastWriteTime.ToString("yyyy/MM/dd hh:mm"));
                                response.Headers.Add("IsReadOnly", info.IsReadOnly ? "Readonly" : "FullAccess");
                                response.Headers.Add("EntryType", info.EntryType ==  FileSystemEntryType.FILE ? "File" : "Folder");
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
                                response.StatusCode = 400;
                            }
                            finally { output.Close(); }

                            break;
                        }
                }

                writer.Dispose();
            }

        }

        private void Listener_OnNewRequestReceived(object sender, RequestReceivedEventArgs e)
        {
           // var request = HttpRequestParser.Parse(e.Request);

            //switch (request.HttpMethod)
            //{
            //    case Data.HttpMethod.Post:
            //        {
            //            break;
            //        }
            //    case Data.HttpMethod.Get:
            //        {
            //            try
            //            {
                            
            //                var fileStream = fileSystemProvider.GetFileStream(DefaultValues.ServerBaseDirectory + request.Request);

            //                NetworkStream clientStream = e.User.GetStream();

            //                var writer = new StreamWriter(clientStream);

            //                writer.WriteLine($"HTTP/1.1 {(int)HttpResponseCode.Okay}");

            //                fileStream.CopyTo(fileStream, 15096);
                            


            //            }
            //            catch
            //            {

            //            }
            //            break;
            //        }
            //}
        }

        private void OpenConnectionAndSendFile(string filePath)
        {

        }
    }
}
