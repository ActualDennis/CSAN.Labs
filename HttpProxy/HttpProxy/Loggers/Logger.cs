using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProxy.Loggers {
    public abstract class Logger<T> {
        public abstract List<T> Entries { get; set; }

        public abstract void Log(T value);
    } 
}
