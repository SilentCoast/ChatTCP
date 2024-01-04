using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ChatTCPlib.Extensions
{
    public static class Extension
    {
        public static TcpState GetState(this TcpClient client)
        {
            //IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            //TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections()
            //    .Where(x => x.LocalEndPoint.Equals(client.Client.LocalEndPoint) 
            //    || x.RemoteEndPoint.Equals(client.Client.RemoteEndPoint))
            //    .ToArray();

            //if (tcpConnections != null && tcpConnections.Length > 0)
            //{
            //    TcpState stateOfConnection = tcpConnections.First().State;
            //    if (stateOfConnection == TcpState.Established)
            //    {
            //        // Connection is OK
            //    }
            //    else
            //    {
            //        // No active TCP Connection to hostName:port
            //    }
            //    return stateOfConnection;
            //}
            if(client.Connected)
            {
                return TcpState.Established;
            }
            return TcpState.Unknown;
        }
    }
}
