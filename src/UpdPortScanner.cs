//using System;
using System.Net;
//using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ipk_l4_scan;

public class UpdPortScanner
{

    private IPAddress _localAddress;

    private int _retranmissionCount; 
    
    //Constructor
    public UpdPortScanner(IPAddress localAddress, int retransmissionCount)
    {
        _localAddress = localAddress;
        _retranmissionCount = retransmissionCount;
    }
    
    public UdpPortScanResult Scan(IPAddress target, int port, int timeout)
        {
            try
            {
                using (Socket icmpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp))
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Client.ReceiveTimeout = timeout;
                    
                    // Bind the ICMP socket and the UDP client to the local address
                    icmpSocket.Bind(new IPEndPoint(_localAddress, 0));
                    udpClient.Client.Bind(new IPEndPoint(_localAddress, 0));
                    
                    for (int attempt = 0; attempt <= _retranmissionCount; attempt++)
                    {
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
                                // Close the ICMP socket
                                icmpSocket.Close();
                            
                                // Close the UDP client
                                udpClient.Close();
                            
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
                    }
                    // If no ICMP message is received, and the UDP timeout occurs, consider the port open
                    return UdpPortScanResult.Open;
                }
            }
            catch (SocketException e)
            {
                // Handle SocketExceptions
                if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    // If a SocketException with the "ConnectionReset" error code is thrown, consider the port closed
                    return UdpPortScanResult.Closed;
                }

                if (e.SocketErrorCode == SocketError.InvalidArgument)
                {
                    return UdpPortScanResult.Unreachable;
                }
                else
                {
                    // If another SocketException is thrown, handle it
                    Console.WriteLine($"Error scanning port: {e.Message}");
                    Console.WriteLine(e);
                    return UdpPortScanResult.Error;
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
        Error,
        Unreachable
    }
}