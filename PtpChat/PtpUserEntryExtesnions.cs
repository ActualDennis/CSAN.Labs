using System;
using System.Collections.Generic;
using System.Text;

namespace Chat {
    public static class PtpUserEntryExtensions {
        public static bool IsLocalUser(this PtpUserEntry value)
        {
            try
            {
                return value.ipAddress.ToString() == value.chatConnection.Client.GetLocalEndPointIpAddress();
            }
            catch
            {
                return true;
            }
        }
    }
}
