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
            
            IPAddress[] addresses = Dns.GetHostAddresses(parser.Target);
            
            foreach (IPAddress address in addresses)
            {
                Console.WriteLine($"{address} | {address.AddressFamily}");
            }
        }
        
    }
}
