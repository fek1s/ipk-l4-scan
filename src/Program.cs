using ipk_l4_scan.Exeptions;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
            
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (parser.InterfaceAddress == null)
            {
                Environment.Exit(99);
            }
            
            // // This will establish full TCP connection with the target and send a message
            // try
            // {
            //     // In this case, we are binding the socket to the interface address and port 0 (random port)
            //     IPEndPoint endPoint = new IPEndPoint(parser.InterfaceAddress, 0);
            //     socket.Bind(endPoint);
            //     
            //     Console.WriteLine($"Socket bound to {endPoint.Address}:{endPoint.Port}");
            //     socket.Connect(parser.Target, 2000);
            //     Console.WriteLine($"Connected to {parser.Target}");
            //     
            //     // Send a message to the target
            //     string message = "Hello from scanner\n";
            //     byte[] data = Encoding.ASCII.GetBytes(message);
            //     int bytesSent = socket.Send(data);
            //     
            //     // Receive a response from the target
            //     byte[] buffer = new byte[1024];
            //     int bytesReceived = socket.Receive(buffer);
            //     string response = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            //     Console.WriteLine($"Received: {response}");
            //     
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine(e);
            // }
            // finally
            // {
            //     socket.Close();
            // }
            
            
            TcpSynScanner scanner = new TcpSynScanner("192.168.0.17", 33689, parser.Timeout);
            PortScanResult result = scanner.Scan(parser.Target, 2000);
            
            switch (result)
            {
                case PortScanResult.Open:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Port {2000} is open");
                    Console.ResetColor(); // Reset the color to default after printing the message
                    break;
                case PortScanResult.ClosedOrFiltered:
                    Console.WriteLine($"Port {2000} is closed or filtered");
                    break;
                case PortScanResult.Error:
                    Console.WriteLine("Error occurred during scanning");
                    break;
            }
            
        }
        
    }
}
