using ipk_l4_scan.Exeptions;
using System.Net;

namespace ipk_l4_scan
{
    internal class Program
    {
        /// <summary>
        /// The entry point of the program.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ArgumentParser parser = new ArgumentParser();

            try
            {
                parser.ParseArguments(args);
            }
            catch (InterfaceNotActive e)
            {
                Console.Error.WriteLine(e.Message);
                parser.PrintActiveInterfaces();
                
                Environment.Exit(1);
            }
            catch (InvalidInterfaceName e)
            {
                Console.WriteLine(e.Message);
                parser.PrintActiveInterfaces();
                
                Environment.Exit(2);
            }
            catch (InvalidPortRangeException e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(3);
            }
            catch (InvalidAgruments e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(4);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(99);
            }
            
            IPAddress[] addresses = Dns.GetHostAddresses(parser.Target);

            if (parser.Debug)
            {
                Console.WriteLine("Interface: " + parser.Interface);
                Console.WriteLine("Target: " + parser.Target);
                Console.WriteLine("Timeout: " + parser.Timeout);
                Console.WriteLine("TCP ports: " + string.Join(", ", parser.TcpPorts));
                Console.WriteLine("UDP ports: " + string.Join(", ", parser.UdpPorts));
                Console.WriteLine("Interface IP: " + parser.InterfaceAddress);
                Console.WriteLine("Interface IPv6: " + parser.InterfaceAddressV6);
                Console.WriteLine("Retransmission count: " + parser.RetransmissionCount);
                foreach (IPAddress address in addresses)
                {
                    Console.WriteLine($"Host resolved as: {address} ");
                }
                Console.WriteLine("==============================================");
            }
            
            if (parser.InterfaceAddress == null)
            {
                Environment.Exit(1);
            }
            
            // Check if the target IP address was resolved to IPv6 address
            bool isIpv6 = addresses[0].AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
            
            TcpPortScanner tcpScanner = new TcpPortScanner();
            
            foreach (int port in parser.TcpPorts)
            {
                TcpPortScanner.TcpPortScanResult result = tcpScanner.Scan(addresses[0], port);
                Console.WriteLine($"{port}/tcp: {result}");
            }

            UpdPortScanner scanner;
            
            if (isIpv6)
            {      
                // If the interface does not have IPv6 address, we need to exit the program
                if (parser.InterfaceAddressV6 == null)
                {
                    Console.Error.WriteLine("Local Interface does not have IPv6 address");
                    Environment.Exit(6);
                }
                scanner = new UpdPortScanner(parser.InterfaceAddressV6, parser.RetransmissionCount);
            }
            else
            {
                scanner = new UpdPortScanner(parser.InterfaceAddress, parser.RetransmissionCount);
            }
            
            foreach (int port in parser.UdpPorts)
            {
                UpdPortScanner.UdpPortScanResult result = scanner.Scan(addresses[0], port, parser.Timeout);
                if (result == UpdPortScanner.UdpPortScanResult.Unreachable)
                {
                    Console.Error.WriteLine($"{parser.Target} is not reachable from {parser.InterfaceAddress}");
                    Environment.Exit(5);
                }
                Console.WriteLine($"{port}/udp: {result}");
                
            }
        }
        
    }
}
