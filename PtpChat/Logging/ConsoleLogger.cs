using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Logging {
    public class ConsoleLogger : ILocalLogger {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void LogLocal(string message)
        {
            Console.WriteLine($"(Local) {message}");
        }
    }
}
