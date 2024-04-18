
namespace ipk_l4_scan
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ArgumentParser parser = new ArgumentParser(args);
            Console.WriteLine(parser.Interface);
            Console.WriteLine(string.Join(", ", parser.TcpPorts));
            Console.WriteLine(string.Join(", ", parser.UdpPorts));
            Console.WriteLine(parser.Target);
        }
    }
}
