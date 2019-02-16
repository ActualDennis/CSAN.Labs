namespace Chat
{
    public static class DefaultValues
    {
        public static int UdpPort => 3254;

        public static int TcpPort => 3200;

        public static string IpAdrress => "127.0.0.1";

        public static int BUFFER_SIZE => 2048;

        public static int NAME_BYTES_MAX => 64;

        public static char ServiceSymbol => ':';

        public static int MAX_CLIENTS => 25;
    }
}