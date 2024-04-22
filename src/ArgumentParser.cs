using System.Net;
using System.Net.NetworkInformation;
using ipk_l4_scan.Exeptions;
using ipk_l4_scan.tests;

namespace ipk_l4_scan;

/// <summary>
/// Parses command line arguments for port scanning.
/// </summary>
public class ArgumentParser
{
    /// <summary>
    /// Property for the network interface to use.
    /// </summary>
    public string? Interface { get; private set; }
    
    public bool Debug { get; private set; }
    
    /// <summary>
    /// Ip address of the network interface.
    /// </summary>
    public IPAddress? InterfaceAddress { get; private set; }
    
    public IPAddress? InterfaceAddressV6 { get; private set; }
    
    
    /// <summary>
    /// List of ports to scan via TCP.
    /// </summary>
    public List<int> TcpPorts { get; private set; }
    
    /// <summary>
    /// List of ports to scan via UDP.
    /// </summary>
    public List<int> UdpPorts { get; private set; }
    
    /// <summary>
    /// Timeout for each port scan.
    /// </summary>
    public int Timeout { get; private set; }
    
    /// <summary>
    /// Property for the target IP address or hostname.
    /// </summary>
    public string Target { get; private set; }

    /// <summary>
    /// Is the number of retransmissions used for UDP scanning. Default is 1. Maximum is 10.
    /// </summary>
    public int RetransmissionCount = 1;
    
    /// <summary>
    /// Defines the maximum port number.
    /// </summary>
    private const int MaxPortNumber = 65535;
    
    /// <summary>
    /// Initializes a new instance of the ArgumentParser class.
    /// </summary>
    public ArgumentParser()
    {
        TcpPorts = new List<int>();
        UdpPorts = new List<int>();
        Timeout = 5000;
        Target = "";
        InterfaceAddress = IPAddress.None;
        Debug = false;

        //ParseArguments(args);
    }
    
    /// <summary>
    /// Parses the command-line arguments.
    /// </summary>
    /// <param name="args"></param>
    public void ParseArguments(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-i":
                case "--interface":
                    // Check if the next argument is not another flag (starts with "-")
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-")) 
                    {
                        if (IsValidInterface(args[i+1]))
                        {
                            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                            {
                                if (netInterface.Name.Equals(args[i + 1], StringComparison.OrdinalIgnoreCase))
                                {
                                    Interface = netInterface.Name;
                                    // TODO check if the interface has an IP address assigned
                                    InterfaceAddress = netInterface.GetIPProperties().UnicastAddresses
                                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.Address;
                                    InterfaceAddressV6 = netInterface.GetIPProperties().UnicastAddresses
                                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)?.Address;
                                }
                            }
                        }
                        else
                        {   
                            throw new InterfaceNotActive($"Erorr: Nework interface {args[++i]} not found or not active.");
                        }
                        
                    }
                    else
                    {
                        throw new InvalidInterfaceName("Error: Missing network interface name.");
                    }
                    break;
                case "--test":
                    if (Interface == null)
                    {
                        Environment.Exit(99);
                    }
                    ArgumentTester test = new ArgumentTester(Interface);
                    test.RunTests();
                    Environment.Exit(0);
                    break;
                case "-d":
                case "--debug":
                    Debug = true;
                    break;
                case "-t":
                case "--pt":
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        ParsePortRange(args[++i], TcpPorts);
                    }
                    else
                    {
                        throw new InvalidPortRangeException("Error: Missing port range.");
                    }                    
                    break;
                case "-u":
                case "--pu":
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        ParsePortRange(args[++i], UdpPorts);
                    }
                    else
                    {
                        throw new InvalidPortRangeException("Error: Missing port range.");
                    }                    
                    break;
                case "-w":
                case "--wait":
                    Timeout = int.Parse(args[++i]);
                    break;
                case "-r":
                    try 
                    {
                        RetransmissionCount = int.Parse(args[++i]);
                        if (RetransmissionCount > 10 || RetransmissionCount < 1)
                        {
                            RetransmissionCount = 1;
                        }
                    }
                    catch (FormatException)
                    {
                        RetransmissionCount = 1;
                    }
                    break;
                default:
                    Target = args[i];
                    break;
            }
        }

        ValidateArguments();
        
    }
    
    /// <summary>
    /// Checks if necessary arguments are provided.
    /// </summary>
    private void ValidateArguments()
    {
        // Check if network interface is specified
        if (Interface == null)
        {
            throw new InterfaceNotActive("Error: Network interface not specified.");
        }
        
        // Check if at least one port is specified
        if (TcpPorts.Count == 0 && UdpPorts.Count == 0)
        {
            throw new InvalidAgruments("Error: No ports specified.");
        }
        
        // Check if target is specified
        if (Target == "")
        {
            throw new InvalidAgruments("Error: Target not specified.");
        }
    }

    /// <summary>
    /// Parses a port range string and adds the ports to the specified list.
    /// </summary>
    /// <param name="range">The port range string ("80, 22-25, 22,23")</param>
    /// <param name="ports">The list to which parsed ports will be added</param>
    private void ParsePortRange(string range, List<int> ports)
    {
        string[] parts = range.Split(',');
        foreach (string part in parts)
        {
            if (part.Contains("-"))
            {
                string[] rangeParts = part.Split('-');
                IsValidRange(rangeParts);
                
                int start = int.Parse(rangeParts[0]);
                int end = int.Parse(rangeParts[1]);
                ports.AddRange(Enumerable.Range(start, end - start + 1));
            }
            else
            {   
                // Check if the part is not empty and is a valid integer
                if (part == "" || int.TryParse(part, out int _) == false)
                {
                    throw new InvalidPortRangeException("Invalid port range.");
                }
                ports.Add(int.Parse(part));
                
                
            }
        }
    }
    
    /// <summary>
    /// Prints a list of active network interfaces and their details.
    /// </summary>
    public void PrintActiveInterfaces()
    {
        Console.WriteLine("List of active network interfaces:");

        foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (netInterface.OperationalStatus == OperationalStatus.Up)
            {
                Console.WriteLine($"{netInterface.Name}:");
                Console.WriteLine($"    Description: {netInterface.Description}");
                Console.WriteLine($"    ID: {netInterface.Id}");
                Console.WriteLine($"    Speed: {netInterface.Speed} bps");
                Console.WriteLine($"    MAC Address: {FormatMacAddress(netInterface.GetPhysicalAddress())}");
                
                Console.WriteLine($"    IPv4 Addresses:");
                foreach (UnicastIPAddressInformation ip in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        Console.WriteLine($"     {ip.Address}");
                    }
                }
                
                Console.WriteLine($"    IPv6 Addresses");
                foreach (UnicastIPAddressInformation ip in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        Console.WriteLine($"     {ip.Address}");
                    }
                }

                Console.WriteLine();
                
            }
        }
    }
    
    /// <summary>
    /// Formats the MAC address as a string in the MAC address format (e.g., "00:1A:2B:3C:4D:5E").
    /// </summary>
    /// <param name="address">The MAC address to format.</param>
    /// <returns>The formatted MAC address string.</returns>
    private string FormatMacAddress(PhysicalAddress address)
    {
        return BitConverter.ToString(address.GetAddressBytes()).Replace("-", ":");
    }
    
    /// <summary>
    /// Checks if the specified network interface exists.
    /// </summary>
    /// <param name="interfaceName">The name of the network interface to check.</param>
    /// <returns>True if the interface exists and its active, otherwise false.</returns>
    private bool IsValidInterface(string interfaceName)
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Any(n => n.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase) && n.OperationalStatus == OperationalStatus.Up);
    }
    
    /// <summary>
    /// Checks if the specified port range is valid.
    /// </summary>
    /// <param name="rangeParts"></param>
    /// <returns></returns>
    private void IsValidRange(string[] rangeParts)
    {
        // Check if the range has two parts
        if (rangeParts.Length != 2)
        {
            throw new InvalidPortRangeException("Invalid port range.");
        }
        
        // Check if the range parts are valid integers
        if (int.TryParse(rangeParts[0], out int start) == false || int.TryParse(rangeParts[1], out int end) == false)
        {
            throw new InvalidPortRangeException("Invalid port range.");
        }
        
        // Check if not negative
        if (start < 0 || end < 0)
        {
            throw new InvalidPortRangeException("Invalid port range.");
        }
        
        // Check if not greater than 65535
        if (start > MaxPortNumber || end > MaxPortNumber)
        {
            throw new InvalidPortRangeException("Invalid port range.");
        }
        
        // Check if start is less than or equal to end
        if (start > end)
        {
            throw new InvalidPortRangeException("Invalid port range.");
        }
    }
}