using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProxy {
    public static class StringExtensions {
        public static string GetNormalizedWebsitePath(this string value)
        {
            value = value.Trim(new char[2] { '\\', '/' });

            if (value.Contains("http://www."))
                value = value.Replace("http://www.", string.Empty);

            if (value.Contains("http://"))
                value = value.Replace("http://", string.Empty);

            if(value.Contains("www."))
                value = value.Replace("www.", string.Empty);

            return value;
        }

    }
}
