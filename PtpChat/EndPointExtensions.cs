using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat {
    public static class SocketExtensions {
        public static string GetLocalEndPointIpAddress(this Socket socket)
        {
            return ((IPEndPoint)socket.LocalEndPoint).Address.ToString();
        }

        public static string GetRemoteEndPointIpAddress(this Socket socket)
        {
            return ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
        }

        public static IPAddress GetRemoteEndPointIpAddress(this Socket socket, bool dummyVariable)
        {
            return ((IPEndPoint)socket.RemoteEndPoint).Address;
        }
        public static IPAddress GetLocalEndPointIpAddress(this Socket socket, bool dummyVariable)
        {
            return ((IPEndPoint)socket.LocalEndPoint).Address;
        }
    }
}
