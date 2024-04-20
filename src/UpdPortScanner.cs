//using System;
using System.Net;
//using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ipk_l4_scan;

public class UpdPortScanner
{
    public UdpPortScanResult Scan(IPAddress target, int port, int timeout)
        {
            try
            {
                using (Socket icmpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp))
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Client.ReceiveTimeout = timeout;
                    icmpSocket.Bind(new IPEndPoint(IPAddress.Any, 0));

                    // Send an empty UDP packet to the target port
                    udpClient.Send(new byte[] { 0 }, 1, target.ToString(), port);

                    // Check if an ICMP message is received
                    try
                    {
                        byte[] buffer = new byte[1024];
                        EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
                        icmpSocket.ReceiveTimeout = timeout;
                        int receivedBytes = icmpSocket.ReceiveFrom(buffer, ref remoteEp);

                        // Check if the received ICMP message is a "port unreachable" message
                        if (receivedBytes >= 20 && buffer[20] == 3 && buffer[21] == 3)
                        {
                            // If an ICMP "port unreachable" message is received, consider the port closed
                            return UdpPortScanResult.Closed;
                        }
                    }
                    catch (SocketException e)
                    {
                        // Handle the SocketException that is thrown when no ICMP message is received
                        if (e.SocketErrorCode != SocketError.TimedOut)
                        {
                            throw;
                        }
                    }
                    
                    // If no ICMP message is received, and the UDP timeout occurs, consider the port open
                    return UdpPortScanResult.Open;
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"Error scanning port: {ex.Message}");
                return UdpPortScanResult.Error;
            }
        }

    public enum UdpPortScanResult
    {
        Open,
        Closed,
        Error
    }
}