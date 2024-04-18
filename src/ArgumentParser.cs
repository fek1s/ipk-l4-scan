using System;
using System.Net.NetworkInformation;

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
    public string? Target { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the ArgumentParser class.
    /// </summary>
    /// <param name="args">The command-line arguments provided to the program.</param>
    public ArgumentParser(string[] args)
    {
        TcpPorts = new List<int>();
        UdpPorts = new List<int>();
        Timeout = 5000;
        
        ParseArguments(args);
    }
    
    /// <summary>
    /// Parses the command-line arguments.
    /// </summary>
    /// <param name="args"></param>
    private void ParseArguments(string[] args)
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
                            Interface = args[++i];    
                        }
                        else
                        {
                            Console.WriteLine($"Erorr: Nework interface {args[++i]} not found or not active.");
                            PrintActiveInterfaces();
                            Environment.Exit(1);
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("Error: Missing network interface name.");
                        PrintActiveInterfaces();
                        Environment.Exit(2);
                    }
                    break;
                case "-t":
                case "--pt":
                    ParsePortRange(args[++i], TcpPorts);
                    break;
                case "-u":
                case "--pu":
                    ParsePortRange(args[++i], UdpPorts);
                    break;
                case "w":
                case "--wait":
                    Timeout = int.Parse(args[++i]);
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
            Console.WriteLine("Error: Network interface not specified.");
            PrintActiveInterfaces();
            Environment.Exit(4);
        }
        
        // Check if at least one port is specified
        if (TcpPorts.Count == 0 && UdpPorts.Count == 0)
        {
            Console.WriteLine("Error: No ports specified.");
            Environment.Exit(4);
        }
        
        // Check if target is specified
        if (Target == null)
        {
            Console.WriteLine("Error: Target not specified.");
            Environment.Exit(4);
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
                if (IsValidRange(rangeParts) == false)
                {
                    Console.WriteLine("Error: Invalid port range.");
                    Environment.Exit(3);
                }
                int start = int.Parse(rangeParts[0]);
                int end = int.Parse(rangeParts[1]);
                ports.AddRange(Enumerable.Range(start, end - start + 1));
            }
            else
            {   
                // Check if the part is not empty
                if (part == "")
                {
                    Console.WriteLine("Error: Invalid port range.");
                    Environment.Exit(3);
                }
                else
                {
                    ports.Add(int.Parse(part));
                }
                
            }
        }
    }
    
    /// <summary>
    /// Prints a list of active network interfaces and their details.
    /// </summary>
    private void PrintActiveInterfaces()
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
    private bool IsValidRange(string[] rangeParts)
    {
        // Check if the range has two parts
        if (rangeParts.Length != 2)
        {
            return false;
        }
        
        // Check if the range parts are valid integers
        if (int.TryParse(rangeParts[0], out int start) == false || int.TryParse(rangeParts[1], out int end) == false)
        {
            return false;
        }
        
        // Check if not negative
        if (start < 0 || end < 0)
        {
            return false;
        }
        
        // Check if not greater than 65535
        if (start > 65535 || end > 65535)
        {
            return false;
        }
        
        // Check if start is less than or equal to end
        return start <= end;
    }
}