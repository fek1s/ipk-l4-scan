using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ipk_l4_scan
{
    public class TcpPortScanner
    {
        /// <summary>
        /// Scan a TCP port on a target IP address.
        /// </summary>
        /// <param name="target">The target IP address to scan.</param>
        /// <param name="port">The port to scan.</param>
        /// <returns>A TcpPortScanResult indicating the result of the scan.</returns>
        public TcpPortScanResult Scan(IPAddress target, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    // Begin the initial connection attempt
                    var connectResult = client.BeginConnect(target, port, null, null);
                    
                    // End the initial connection attempt
                    client.EndConnect(connectResult);

                    // Connection successful, close the connection
                    client.Close();
                    return TcpPortScanResult.Open;
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    // RST response received, mark the port as closed
                    return TcpPortScanResult.Closed;
                }
                else
                {
                    // Handle other socket exceptions
                    Console.WriteLine($"Error scanning port {port}: {ex.Message}");
                    return TcpPortScanResult.Error;
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"Error scanning port {port}: {ex.Message}");
                return TcpPortScanResult.Error;
            }
        }

        // Define the possible results of a TCP port scan
        public enum TcpPortScanResult
        {
            Open,       // The port is open and accepting connections
            Closed,     // The port is closed
            Error       // An error occurred during the scan
        }
    }
}
