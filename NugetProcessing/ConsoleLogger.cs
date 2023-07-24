using NuGet.Common;

public class ConsoleLogger : ILogger
{
    public void LogDebug(string data)
    {
      //  Console.WriteLine(data);
    }

    public void LogVerbose(string data)
    {
      //  Console.WriteLine(data);
    }

    public void LogInformation(string data)
    {
      //  Console.WriteLine(data);
    }

    public void LogMinimal(string data)
    {
        Console.WriteLine(data);
    }

    public void LogWarning(string data)
    {
        ConsoleReverter.Warning(data);
    }

    public void LogError(string data)
    {
        ConsoleReverter.Error(data);
    }

    public void LogInformationSummary(string data)
    {
        Console.WriteLine(data);
    }

    public void Log(LogLevel level, string data)
    {
        Console.WriteLine(data);
    }

    public Task LogAsync(LogLevel level, string data)
    {
        throw new NotImplementedException();
    }

    public void Log(ILogMessage message)
    {
        throw new NotImplementedException();
    }

    public Task LogAsync(ILogMessage message)
    {
        throw new NotImplementedException();
    }
}