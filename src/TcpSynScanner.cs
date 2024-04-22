using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ipk_l4_scan;

public class TcpSynScanner
{
    private string _sourceIpAddress;
    private int _sourcePort;
    private int _timeout;

    public TcpSynScanner(string sourceIpAddress, int sourcePort, int timeout = 500)
    {
        _sourceIpAddress = sourceIpAddress;
        _sourcePort = sourcePort;
        _timeout = timeout;
    }

    /// <summary>
    /// This method creates a raw socket and sends a SYN packet to the target IP address and port.
    /// But it does not receive any response.
    /// </summary>
    /// <param name="targetIpAddress"></param>
    /// <param name="portToScan"></param>
    /// <returns></returns>
    public SynTcpPortScanResult Scan(string targetIpAddress, int portToScan)
    {
        try
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Parse(_sourceIpAddress), _sourcePort));
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

                byte[] synPacket = BuildSynPacket(targetIpAddress, portToScan);
                socket.SendTo(synPacket, new IPEndPoint(IPAddress.Parse(targetIpAddress), portToScan));


                byte[] buffer = new byte[65536];
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int receivedBytes = socket.ReceiveFrom(buffer, ref remoteEndPoint);
                
                    
                Thread.Sleep(_timeout);
                if (IsPortOpen(buffer, receivedBytes))
                {
                    return SynTcpPortScanResult.Open;
                }
                else
                {
                    return SynTcpPortScanResult.ClosedOrFiltered;
                }
            }
        }
        catch (SocketException)
        {
            return SynTcpPortScanResult.Error;
        }
    }

    private byte[] BuildSynPacket(string targetIpAddress, int port)
    {
        byte[] packet = new byte[44];

        // IP header
        packet[0] = 0x45; // Version and header length
        packet[1] = 0; // Differentiated Services Field
        packet[2] = 0; // Total Length (will be filled later)
        packet[3] = 0; // Total Length (will be filled later)
        packet[4] = 0; // Identification
        packet[5] = 0; // Identification
        packet[6] = 0; // Flags and Fragment Offset
        packet[7] = 0; // Flags and Fragment Offset
        packet[8] = 64; // TTL
        packet[9] = 6; // Protocol (TCP)
        packet[10] = 0; // Header checksum (will be filled later)
        packet[11] = 0;
        Array.Copy(IPAddress.Parse(_sourceIpAddress).GetAddressBytes(), 0, packet, 12, 4); // Source IP
        Array.Copy(IPAddress.Parse(targetIpAddress).GetAddressBytes(), 0, packet, 16, 4); // Destination IP

        // TCP header
        packet[20] = 0; // Source port (will be filled later)
        packet[21] = 0;
        packet[22] = 0; // Destination port (will be filled later)
        packet[23] = 0;
        packet[24] = 0; // Sequence number
        packet[25] = 0;
        packet[26] = 0;
        packet[27] = 0;
        packet[28] = 0; // Acknowledgment number
        packet[29] = 0;
        packet[30] = 0;
        packet[31] = 0;
        packet[32] = 0x60; // Data offset (5) + reserved (3)
        packet[33] = 0x02; // Flags (SYN)
        packet[34] = 4; // Window size
        packet[35] = 0;
        packet[36] = 0; // Checksum (will be filled later)
        packet[37] = 0;
        packet[38] = 0; // Urgent pointer
        packet[39] = 0;
        packet[40] = 0x2; // Options
        packet[41] = 0x4; // Option kind (Maximum segment size)
        packet[42] = 0x5; // Option length
        packet[43] = 0b10110100; // Option value
        
        
        
        

        // Fill in IP total length and checksum
        ushort ipLength = (ushort)(packet.Length - 14); // Ethernet header length is 14
        packet[2] = (byte)(ipLength >> 8);
        packet[3] = (byte)ipLength;
        ushort ipChecksum = CalculateChecksum(packet, 0, 20);
        packet[10] = (byte)(ipChecksum >> 8);
        packet[11] = (byte)ipChecksum;

        // Fill in TCP source and destination ports
        ushort tcpSourcePort = (ushort)_sourcePort;
        ushort tcpDestinationPort = (ushort)port;
        packet[20] = (byte)(tcpSourcePort >> 8);
        packet[21] = (byte)tcpSourcePort;
        packet[22] = (byte)(tcpDestinationPort >> 8);
        packet[23] = (byte)tcpDestinationPort;

        // Fill in TCP checksum
        ushort tcpChecksum = CalculateChecksum(packet, 20, packet.Length - 20);
        packet[36] = (byte)(tcpChecksum >> 8);
        packet[37] = (byte)tcpChecksum;

        return packet;
    }

    private ushort CalculateChecksum(byte[] buffer, int offset, int length)
    {
        ushort word16;
        uint sum = 0;

        for (int i = offset; i < offset + length; i += 2)
        {
            word16 = (ushort)(((buffer[i] << 8) & 0xFF00) + (buffer[i + 1] & 0xFF));
            sum += (uint)word16;
        }

        while ((sum >> 16) != 0)
        {
            sum = (sum & 0xFFFF) + (sum >> 16);
        }

        sum = ~sum;

        return (ushort)sum;
    }

    private bool IsPortOpen(byte[] buffer, int length)
    {
        // Assuming that if any response is received, the port is open
        return length > 0;
    }
}

public enum SynTcpPortScanResult
{
    Open,
    ClosedOrFiltered,
    Error
}