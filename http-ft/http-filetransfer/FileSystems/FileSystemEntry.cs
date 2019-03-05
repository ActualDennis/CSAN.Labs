using System;

namespace http_filetransfer.FileSystems {
    public class FileSystemEntry
    {
        public FileSystemEntryType EntryType;

        public long OccupiedSpace;

        public bool IsReadOnly;

        public DateTime LastWriteTime;
        public string Name;
    }
}