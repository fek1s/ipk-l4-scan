namespace ipk_l4_scan.Exeptions;

public class InterfaceNotActive: Exception
{
    public InterfaceNotActive() {}

    public InterfaceNotActive(string message) : base(message) { }
    
    public InterfaceNotActive(string message, Exception inner) : base(message, inner) { }
    
}