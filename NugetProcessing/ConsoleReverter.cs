public class ConsoleReverter : IDisposable
{
    public ConsoleReverter(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }
    public void Dispose()
    {
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static IDisposable Done()
    {
        return new ConsoleReverter(ConsoleColor.Green);
    }
    public static IDisposable Warning()
    {
        return new ConsoleReverter(ConsoleColor.DarkYellow);
    }
    public static IDisposable Error()
    {
        return new ConsoleReverter(ConsoleColor.DarkRed);
    }

    public static void Error(string message)
    {
        using (Error())
        {
            Console.WriteLine(message);
        }
    }

    public static void Error(Exception exception)
    {
        using (Error())
        {
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
        }
    }

    public static void Message(string message)
    {
        using (Done())
        {
            Console.WriteLine(message);
        }
    }

    public static void Warning(string message)
    {
        using (Warning())
        {
            Console.WriteLine(message);
        }
    }

}