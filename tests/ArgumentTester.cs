using ipk_l4_scan.Exeptions;

namespace ipk_l4_scan.tests;

public class ArgumentTester
{
    /// <summary>
    /// Local active interface
    /// </summary>
    private string _interface;
    
    //Contructor
    public ArgumentTester(string localInterface)
    {
        _interface = localInterface;
        //RunTests();
    }

    public void RunTests()
    {
        TestInterface();
        TestInvalidInterface();
        
        // Testing range of ports
        TestRangeOutOfScope(); 
        TestPortRangeList();
        TestPortRangeMissingSecondPort();
        TestPortRangeNegative();
        TestPortRangeInvalidFormat();
        TestPortRangeNotNumber();
        
        
        
        TestUnspecifiedInterface();
    }
    
    private void TestInterface()
    {
        ArgumentParser parser = new ArgumentParser();
        try
        {
            parser.ParseArguments(new string[] { "-i" });
        }
        catch (InvalidInterfaceName)
        {
            Console.WriteLine("TestInterface: Passed",Console.ForegroundColor = ConsoleColor.Green);
        }
        catch (Exception)
        {
            Console.WriteLine("TestInterface: Failed", Console.ForegroundColor = ConsoleColor.Red);
        }
    }
    
    private void TestInvalidInterface()
    {
        ArgumentParser parser = new ArgumentParser();
        try
        {
            parser.ParseArguments(new string[] { "-i", "invalidInterface" });
        }
        catch (InterfaceNotActive)
        {
            Console.WriteLine("TestInvalidInterface: Passed", Console.ForegroundColor = ConsoleColor.Green);
        }
        catch (Exception)
        {
            Console.WriteLine("TestInvalidInterface: Failed", Console.ForegroundColor = ConsoleColor.Red);
        }
    }
    
    /// <summary>
    /// Port range out of scope
    /// Usable port range is 1-65535
    /// </summary>
    private void TestRangeOutOfScope()
    {
        ArgumentParser parser = new ArgumentParser();
        try
        {
            parser.ParseArguments(new string[] { "-i", _interface, "-u", "0-65536" });
        }
        catch (InvalidPortRangeException)
        {
            Console.WriteLine("TestInvalidPortRange: Passed" , Console.ForegroundColor = ConsoleColor.Green);
        }
        catch (Exception e)
        {   
            Console.WriteLine(e.Message);
            Console.WriteLine("TestInvalidPortRange: Failed", Console.ForegroundColor = ConsoleColor.Red);
        }
    }
    
    /// <summary>
    /// Port range list unsupported format, missing second port
    /// </summary>
    private void TestPortRangeList()
    {
        ArgumentParser parser = new ArgumentParser();
        try
        {
            parser.ParseArguments(new string[] { "-i", _interface, "-u", "1," });
        }
        catch (InvalidPortRangeException)
        {
            Console.WriteLine("TestPortRangeList: Passed" , Console.ForegroundColor = ConsoleColor.Green);
        }
        catch (Exception)
        {
            Console.WriteLine("TestPortRangeList: Failed", Console.ForegroundColor = ConsoleColor.Red);
        }
    }
    
    /// <summary>
    /// Port range missing second port (22-)
    /// </summary>
    private void TestPortRangeMissingSecondPort()
    {
        ArgumentParser parser = new ArgumentParser();
        try
        {
            parser.ParseArguments(new string[] { "-i", _interface, "-u", "22-" });
        }
        catch (InvalidPortRangeException)
        {
            Console.WriteLine("TestPortRangeMissingSecondPort: Passed" , Console.ForegroundColor = ConsoleColor.Green);
        }
        catch (Exception)
        {
            Console.WriteLine("TestPortRangeMissingSecondPort: Failed", Console.ForegroundColor = ConsoleColor.Red);
        }
    }
    
    /// <summary>
    /// Usage of negative port range
    /// </summary>
    private void TestPortRangeNegative()
    {
        ArgumentParser parser = new ArgumentParser();
        try
        {
            parser.ParseArguments(new string[] { "-i", _interface, "-u", "-22-22" });
        }
        catch (InvalidPortRangeException)
        {
            Console.WriteLine("TestPortRangeNegative: Passed", Console.ForegroundColor = ConsoleColor.Green);
        }
        catch (Exception)
        {
            Console.WriteLine("TestPortRangeNegative: Failed", Console.ForegroundColor = ConsoleColor.Red);
        }
    }
    
    private void TestPortRangeInvalidFormat()
    {
        ArgumentParser parser = new ArgumentParser();
        try
        {
            parser.ParseArguments(new string[] { "-i", _interface, "-u", "22-22-22" });
        }
        catch (InvalidPortRangeException)
        {
            Console.WriteLine("TestPortRangeInvalidFormat: Passed", Console.ForegroundColor = ConsoleColor.Green);
        }
        catch (Exception)
        {
            Console.WriteLine("TestPortRangeInvalidFormat: Failed", Console.ForegroundColor = ConsoleColor.Red);
        }
    }
    
    private void TestPortRangeNotNumber()
    {
        ArgumentParser parser = new ArgumentParser();
        try
        {
            parser.ParseArguments(new string[] { "-i", _interface, "-u", "22-22a" });
        }
        catch (InvalidPortRangeException)
        {
            Console.WriteLine("TestPortRangeNotNumber: Passed", Console.ForegroundColor = ConsoleColor.Green);
        }
        catch (Exception)
        {
            Console.WriteLine("TestPortRangeNotNumber: Failed", Console.ForegroundColor = ConsoleColor.Red);
        }
    }
    
    /// <summary>
    /// Unspecified local interface
    /// </summary>
    private void TestUnspecifiedInterface()
    {
        ArgumentParser parser = new ArgumentParser();
        try
        {
            parser.ParseArguments(new string[] { "-u", "23", "-t", "1", "127.0.0.1"});
        }
        catch (InterfaceNotActive)
        {
            Console.WriteLine("UnspecifiedInterface: Passed", Console.ForegroundColor = ConsoleColor.Green);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("UnspecifiedInterface: Failed" , Console.ForegroundColor = ConsoleColor.Red);
        }
    }
    
}