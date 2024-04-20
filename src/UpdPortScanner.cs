//using System;
using System.Net;
//using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

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

                // Send an empty UDP packet to the target IP address and port
                udpClient.Send(new byte[] { 0 }, 1, target.ToString(), port);

                // Wait for a response
                IPEndPoint remoteEndPoint = new IPEndPoint(target, port);
                try
                { 
                    byte[] receivedData = udpClient.Receive(ref remoteEndPoint);
                    // If data is received, consider the port open
                    Console.WriteLine("Received data: " + Encoding.ASCII.GetString(receivedData));
                    return UdpPortScanResult.Open;
                }
                catch (SocketException ex)
                {
                    // If a "connection reset" error is received, consider the port closed
                    if (ex.SocketErrorCode == SocketError.ConnectionReset) 
                        return UdpPortScanResult.Closed;

                    // If a timeout error is received, check if an ICMP message is received
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        // Wait for an ICMP message
                        try
                        { byte[] buffer = new byte[1024];
                            EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
                            icmpSocket.ReceiveTimeout = timeout;
                            int receivedBytes = icmpSocket.ReceiveFrom(buffer, ref remoteEp);

                            // Check if the received ICMP message is a "port unreachable" message
                            if (receivedBytes >= 20 && buffer[20] == 3 && buffer[21] == 3)
                            {
                                // If an ICMP "port unreachable" message is received, consider the port closed
                                Console.WriteLine("Received ICMP message: Port unreachable");
                                return UdpPortScanResult.Closed;
                            }
                        }
                        catch (SocketException)
                        {
                                // If no ICMP message is received, consider the port open
                                return UdpPortScanResult.Open;
                        }
                    }

                    // Other exceptions may indicate an error during scanning
                    Console.WriteLine($"Error scanning port: {ex.Message}");
                    return UdpPortScanResult.Error;
                }
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