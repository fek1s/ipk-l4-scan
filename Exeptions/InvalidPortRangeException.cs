
namespace ipk_l4_scan.Exeptions;

public class InvalidPortRangeException : Exception
{
    public InvalidPortRangeException() {}

    public InvalidPortRangeException(string message) : base(message) { }
    
    public InvalidPortRangeException(string message, Exception inner) : base(message, inner) { }

}