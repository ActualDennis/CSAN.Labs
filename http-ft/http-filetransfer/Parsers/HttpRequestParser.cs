using System;
using System.Collections.Generic;
using System.Text;

namespace http_filetransfer.Parsers {
    public static class HttpRequestParser {
        public static HttpRequest Parse(byte[] rawRequest)
        {
            var data = Encoding.UTF8.GetString(rawRequest);

            string[] headers = data.Split("\r\n");

            string request = data.Substring(0, data.IndexOf("\r\n")).Split(" ")[1];
            string commandSpecificHeader = null;
            int headerIndex = 0;

            foreach(var receivedHeader in headers)
            {
                for(headerIndex = 0; headerIndex < Headers.Special.Length; ++headerIndex)
                {
                    if(receivedHeader.Contains(Headers.Special[headerIndex]))
                    {
                        commandSpecificHeader = receivedHeader.Substring(receivedHeader.IndexOf(" ") + 1);
                        break;
                    }
                }
            }

            SpecialHeader typeofSpecialHeader = SpecialHeader.Reserved;
            switch (headerIndex)
            {
                case (int)SpecialHeader.CopyFrom:
                    {
                        typeofSpecialHeader = SpecialHeader.CopyFrom;
                        break;
                    }
            }

            return new HttpRequest()
            {
                HttpMethod = data.Substring(0, data.IndexOf("\r\n")).Split(" ")[0],
                Request = request,
                SpecialHeaderValue = commandSpecificHeader,
                SpecialHeaderName = typeofSpecialHeader
            };
        }
    }
}
