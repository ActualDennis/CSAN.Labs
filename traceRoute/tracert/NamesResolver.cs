using System.Net;

namespace tracert {
    public static class NamesResolver {

        public static IPAddress Resolve(string hostNameOrIpAddress)
        {
            if (IPAddress.TryParse(hostNameOrIpAddress, out var ipAddress))
                return ipAddress;

            return Dns.GetHostEntry(hostNameOrIpAddress).AddressList[0];
        }

        public static string GetHostNameByIp(IPAddress address)
        {
            try
            {
                return Dns.GetHostEntry(address).HostName;
            }
            catch
            {
                return null;
            }
        }
    }
}
