using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProxy.Loggers {
    public  class HttpRequestsLogger : Logger<HttpRequestEntry> {
        public HttpRequestsLogger()
        {
            Entries = new List<HttpRequestEntry>();
        }
        public override List<HttpRequestEntry> Entries { get; set; }

        public override void Log(HttpRequestEntry value)
        {
            if (value == null)
                return;

            Entries.Add(value);
        }
    }
}
