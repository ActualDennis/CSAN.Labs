using System;
using System.Collections.Generic;
using System.Text;

namespace Chat {
    public interface ILocalLogger {
        void Log(string message);

        void LogLocal(string message);
    }
}
