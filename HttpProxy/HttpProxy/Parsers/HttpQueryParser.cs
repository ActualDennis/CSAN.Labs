using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpProxy.Parsers {
    public static class HttpQueryParser {

        public static string[] GetHeaders(byte[] httpResponse)
        {
            return Encoding.ASCII.GetString(httpResponse).Trim('\0').Split("\r\n"); 
        }

        public static string GetHostName(byte[] httpRequest)
        {
            string[] queryStrings = Encoding.ASCII.GetString(httpRequest).Trim('\0').Split("\r\n");

            string host = queryStrings.FirstOrDefault(queryString => queryString.Contains("Host:"));

            try
            {
                host = host.Substring(host.IndexOf(" ") + 1).GetNormalizedWebsitePath();
            }
            catch (NullReferenceException)
            {
                return null;
            }

            return host;
        }
    }
}
