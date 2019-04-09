using http_filetransfer.Data;
using http_filetransfer.FileSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace http_filetransfer.Commands
{
    public class HeadCommand : HttpCommand
    {
        public HeadCommand(DefaultFileSystemProvider fsProvider)
        {
            this.fsProvider = fsProvider;
        }
        
        private DefaultFileSystemProvider fsProvider { get; set; }

        public override void Execute(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            string fullPath = DefaultValues.ServerBaseDirectory + request.RawUrl;

            try
            {
                //if this is a directory
                var info = fsProvider.GetFileorDirectoryInfo(fullPath);
                response.Headers.Add("Name", info.Name);
                response.Headers.Add("FileLength", info.OccupiedSpace.ToString());
                response.Headers.Add("LastWriteTime", info.LastWriteTime.ToString("yyyy/MM/dd hh:mm"));
                response.Headers.Add("IsReadOnly", info.IsReadOnly ? "Readonly" : "FullAccess");
                response.Headers.Add("EntryType", info.EntryType == FileSystemEntryType.FILE ? "File" : "Folder");
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
