namespace ipk_l4_scan.Exeptions;

public class InvalidInterfaceName: Exception
{
    
    public InvalidInterfaceName(string message) : base(message) { }
    
}