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
                
                Environment.Exit(1);
            }
            catch (InvalidInterfaceName e)
            {
                Console.WriteLine(e.Message);
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

            Console.WriteLine("Interface: " + parser.Interface);
            Console.WriteLine("Target: " + parser.Target);
            Console.WriteLine("Timeout: " + parser.Timeout);
            Console.WriteLine("TCP ports: " + string.Join(", ", parser.TcpPorts));
            Console.WriteLine("UDP ports: " + string.Join(", ", parser.UdpPorts));
            Console.WriteLine("Interface IP: " + parser.InterfaceAddress);
            
            IPAddress[] addresses = Dns.GetHostAddresses(parser.Target);
            
            foreach (IPAddress address in addresses)
            {
                Console.WriteLine($"{address} | {address.AddressFamily}");
            }

            if (parser.InterfaceAddress == null)
            {
                Environment.Exit(1);
            }
            
            UpdPortScanner scanner = new UpdPortScanner(parser.InterfaceAddress, parser.RetransmissionCount);
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
