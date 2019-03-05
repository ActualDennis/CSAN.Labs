using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace http_filetransfer.FileSystems {
    /// <summary>
    /// Abstract class defining minimum necessary operations for any fileSystem
    /// </summary>
    public class DefaultFileSystemProvider {

        public virtual void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
        public virtual void Delete(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path);
            else if (File.Exists(path))
                File.Delete(path);
            else
                throw new FileNotFoundException("File or directory was not found.");
        }

        public virtual IEnumerable<FileSystemEntry> EnumerateDirectory(string path)
        {
            var folders = Directory.EnumerateDirectories(path)
               .Select(x => new DirectoryInfo(x))
               .Select(x => new FileSystemEntry
               {
                   EntryType = FileSystemEntryType.FOLDER,
                   OccupiedSpace = GetOccupiedDirectoryorFileSpace(path),
                   IsReadOnly = (x.Attributes & FileAttributes.ReadOnly).Equals(FileAttributes.ReadOnly),
                   LastWriteTime = x.LastWriteTime,
                   Name = x.Name
               });

            var files = Directory.EnumerateFiles(path)
                .Select(x => new FileInfo(x))
                .Select(x => new FileSystemEntry
                {
                    EntryType = FileSystemEntryType.FILE,
                    OccupiedSpace = x.Length,
                    IsReadOnly = (x.Attributes & ~FileAttributes.ReadOnly).Equals(FileAttributes.ReadOnly),
                    LastWriteTime = x.LastWriteTime,
                    Name = x.Name
                });

            return folders.Concat(files);
        }

        public virtual FileSystemEntry GetFileorDirectoryInfo(string path)
        {
            if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);

                return new FileSystemEntry()
                {
                    EntryType = FileSystemEntryType.FOLDER,
                    OccupiedSpace = GetOccupiedDirectoryorFileSpace(path),
                    IsReadOnly = (dirInfo.Attributes & FileAttributes.ReadOnly).Equals(FileAttributes.ReadOnly),
                    LastWriteTime = dirInfo.LastWriteTime,
                    Name = dirInfo.Name
                };
            }
            else if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);

                return new FileSystemEntry()
                {
                    EntryType = FileSystemEntryType.FOLDER,
                    OccupiedSpace = GetOccupiedDirectoryorFileSpace(path),
                    IsReadOnly = (fileInfo.Attributes & FileAttributes.ReadOnly).Equals(FileAttributes.ReadOnly),
                    LastWriteTime = fileInfo.LastWriteTime,
                    Name = fileInfo.Name
                };
            }
            else
                throw new DirectoryNotFoundException($"Did not find the path {path}.");
        }
        public virtual Stream GetFileStream(string pathToFile)
        {
            if (File.Exists(pathToFile))
                return new FileStream(pathToFile, FileMode.Open);

            throw new FileNotFoundException("Could not find the file.");
        }
        /// <summary>
        /// Do not use 'virtual' keyword because it's being used
        /// by some functions in this class and we need default implementation of it
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public long GetOccupiedDirectoryorFileSpace(string path)
        {
            if (Directory.Exists(path))
            {
                long result = 0;
                var fileInfos = Directory.EnumerateFiles(path)
                    .Select(x => new FileInfo(x));

                foreach (var file in fileInfos) result += file.Length;

                return result;
            }
            else if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }

            throw new DirectoryNotFoundException($"Did not find the path.");
        }

        public virtual string GetFileLastModifiedTime(string path)
        {
            if (File.Exists(path))
            {
                return new FileInfo(path).LastWriteTime.ToString("yyyyMMddhhmmss");
            }
            throw new DirectoryNotFoundException("Did not find the path.");
        }

        public virtual void Move(string from, string to)
        {
            if (File.Exists(from))
                File.Move(from, to);
            else if (Directory.Exists(from))
                Directory.Move(from, to);
            else
                throw new DirectoryNotFoundException("Could not find what to rename.");
        }

        public virtual FileStream CreateNewFileorOverwrite(string path)
        {
            return new FileStream(path, FileMode.Create);
        }

        public virtual FileStream CreateNewFile(string path)
        {
            while (File.Exists(path)) { path += "(1)"; }
            return new FileStream(path, FileMode.CreateNew);
        }
    }
}
