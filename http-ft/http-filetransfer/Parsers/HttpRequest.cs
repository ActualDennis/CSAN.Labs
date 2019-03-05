using System;
using System.Collections.Generic;
using System.Text;

namespace http_filetransfer.Parsers {
    public class HttpRequest {
        public string HttpMethod;

        public string Request;

        public string SpecialHeaderValue;

        public SpecialHeader SpecialHeaderName;
    }
}
