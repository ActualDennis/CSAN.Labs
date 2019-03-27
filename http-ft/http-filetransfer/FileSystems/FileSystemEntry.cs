using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace http_filetransfer.FileSystems {
    public class FileSystemEntry
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public FileSystemEntryType EntryType;

        public long OccupiedSpace;

        public bool IsReadOnly;

        public DateTime LastWriteTime;
        public string Name;
    }
}